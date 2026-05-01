using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Books;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.OpenLibrary.Resources;

namespace NzbDrone.Core.MetadataSource.OpenLibrary
{
    public class OpenLibraryProxy : IProvideAuthorInfo, IProvideBookInfo, ISearchForNewBook, ISearchForNewAuthor, ISearchForNewEntity
    {
        private const string BaseUrl = "https://openlibrary.org";
        private const string CoversUrl = "https://covers.openlibrary.org";
        private const int EditionsPerWork = 50;
        private const int WorksPerAuthor = 100;
        private const int SearchLimit = 20;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new OLTextValueConverter() }
        };

        private readonly IHttpClient _httpClient;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IEditionService _editionService;
        private readonly Logger _logger;
        private readonly ICached<HashSet<string>> _changedCache;

        public OpenLibraryProxy(IHttpClient httpClient,
                                IAuthorService authorService,
                                IBookService bookService,
                                IEditionService editionService,
                                ICacheManager cacheManager,
                                Logger logger)
        {
            _httpClient = httpClient;
            _authorService = authorService;
            _bookService = bookService;
            _editionService = editionService;
            _logger = logger;
            _changedCache = cacheManager.GetCache<HashSet<string>>(GetType());
        }

        // ── IProvideAuthorInfo ────────────────────────────────────────────

        public HashSet<string> GetChangedAuthors(DateTime startTime)
        {
            // OpenLibrary does not provide a changed-since feed — return null
            // so the caller falls back to a full refresh.
            return null;
        }

        public Author GetAuthorInfo(string foreignAuthorId, bool useCache = true)
        {
            _logger.Debug("Getting author info from OpenLibrary for {0}", foreignAuthorId);

            var authorResource = FetchJson<OLAuthorResource>($"/authors/{foreignAuthorId}.json");
            if (authorResource == null)
            {
                throw new AuthorNotFoundException(foreignAuthorId);
            }

            var worksResource = FetchJson<OLAuthorWorksResponse>($"/authors/{foreignAuthorId}/works.json?limit={WorksPerAuthor}");
            var works = worksResource?.Entries ?? new List<OLWorkResource>();

            return MapAuthor(authorResource, works);
        }

        // ── IProvideBookInfo ──────────────────────────────────────────────

        public Tuple<string, Book, List<AuthorMetadata>> GetBookInfo(string foreignBookId)
        {
            _logger.Debug("Getting book info from OpenLibrary for {0}", foreignBookId);

            var workResource = FetchJson<OLWorkResource>($"/works/{foreignBookId}.json");
            if (workResource == null)
            {
                throw new BookNotFoundException(foreignBookId);
            }

            var editionsResource = FetchJson<OLWorkEditionsResponse>($"/works/{foreignBookId}/editions.json?limit={EditionsPerWork}");
            var editions = editionsResource?.Entries ?? new List<OLEditionResource>();

            var book = MapWork(workResource, editions);

            var authorId = ExtractOlId(workResource.Authors?.FirstOrDefault()?.Author?.Key);
            AuthorMetadata metadata;

            if (authorId.IsNotNullOrWhiteSpace())
            {
                var authorResource = FetchJson<OLAuthorResource>($"/authors/{authorId}.json");
                metadata = authorResource != null
                    ? MapAuthorMetadata(authorResource)
                    : new AuthorMetadata { ForeignAuthorId = authorId, Name = "Unknown" };
            }
            else
            {
                authorId = "unknown";
                metadata = new AuthorMetadata { ForeignAuthorId = authorId, Name = "Unknown" };
            }

            book.AuthorMetadata = metadata;
            var authorMetadataList = new List<AuthorMetadata> { metadata };
            var authorDict = new Dictionary<string, AuthorMetadata> { { authorId, metadata } };
            AddDbIds(authorId, book, authorDict);

            return Tuple.Create(authorId, book, authorMetadataList);
        }

        // ── ISearchForNewBook ─────────────────────────────────────────────

        public List<Book> SearchForNewBook(string title, string author, bool getAllEditions = true)
        {
            var query = title;
            if (author.IsNotNullOrWhiteSpace())
            {
                query += " " + author;
            }

            var lowerTitle = title.ToLowerInvariant().Trim();
            var split = lowerTitle.Split(':');
            var prefix = split[0];

            if (split.Length == 2 && new[] { "author", "work", "edition", "isbn", "asin" }.Contains(prefix))
            {
                var slug = split[1].Trim();
                if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace))
                {
                    return new List<Book>();
                }

                switch (prefix)
                {
                    case "author":
                        return SearchByAuthorOlId(slug);
                    case "work":
                    case "edition":
                        return SearchByWorkOlId(slug);
                    case "isbn":
                        return SearchByIsbn(slug);
                }
            }

            return SearchOL(query, getAllEditions);
        }

        public List<Book> SearchByIsbn(string isbn)
        {
            try
            {
                var editionResource = FetchJson<OLEditionResource>($"/isbn/{isbn}.json");
                if (editionResource == null)
                {
                    return new List<Book>();
                }

                var workKey = editionResource.Works?.FirstOrDefault()?.Key;
                if (workKey.IsNullOrWhiteSpace())
                {
                    return new List<Book>();
                }

                var workId = ExtractOlId(workKey);
                return SearchByWorkOlId(workId);
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Error searching by ISBN {0}", isbn);
                return new List<Book>();
            }
        }

        public List<Book> SearchByAsin(string asin)
        {
            // OpenLibrary does not index by ASIN — fall back to title search
            return SearchOL(asin, false);
        }

        public List<Book> SearchByGoodreadsBookId(int goodreadsId, bool getAllEditions)
        {
            // Goodreads IDs are not supported by OpenLibrary
            _logger.Warn("SearchByGoodreadsBookId called but OpenLibrary does not support Goodreads IDs");
            return new List<Book>();
        }

        // ── ISearchForNewAuthor ───────────────────────────────────────────

        public List<Author> SearchForNewAuthor(string title)
        {
            var books = SearchForNewBook(title, null);
            return books
                .Select(x => x.Author.Value)
                .DistinctBy(x => x.ForeignAuthorId)
                .ToList();
        }

        // ── ISearchForNewEntity ───────────────────────────────────────────

        public List<object> SearchForNewEntity(string title)
        {
            var books = SearchForNewBook(title, null, false);
            var result = new List<object>();
            foreach (var book in books)
            {
                var bookAuthor = book.Author.Value;
                if (!result.Contains(bookAuthor))
                {
                    result.Add(bookAuthor);
                }

                result.Add(book);
            }

            return result;
        }

        // ── Private search helpers ────────────────────────────────────────

        private List<Book> SearchOL(string query, bool getAllEditions)
        {
            try
            {
                var url = $"/search.json?q={Uri.EscapeDataString(query)}" +
                          $"&fields=key,title,author_key,author_name,cover_i,first_publish_year,subject,isbn,number_of_pages_median" +
                          $"&limit={SearchLimit}";

                var searchResponse = FetchJson<OLSearchResponse>(url);
                if (searchResponse?.Docs == null || !searchResponse.Docs.Any())
                {
                    return new List<Book>();
                }

                var books = new List<Book>();
                foreach (var doc in searchResponse.Docs.Take(SearchLimit))
                {
                    try
                    {
                        var book = MapSearchDoc(doc);
                        if (book != null)
                        {
                            books.Add(book);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Warn(e, "Error mapping search result for {0}", doc.Key);
                    }
                }

                return books;
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Error searching OpenLibrary for {0}", query);
                return new List<Book>();
            }
        }

        private List<Book> SearchByAuthorOlId(string authorId)
        {
            try
            {
                var author = GetAuthorInfo(authorId);
                return author.Books.Value;
            }
            catch (AuthorNotFoundException)
            {
                return new List<Book>();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Error searching by author OL ID {0}", authorId);
                return new List<Book>();
            }
        }

        private List<Book> SearchByWorkOlId(string workId)
        {
            try
            {
                var tuple = GetBookInfo(workId);
                AddDbIds(tuple.Item1, tuple.Item2, tuple.Item3.ToDictionary(x => x.ForeignAuthorId));
                return new List<Book> { tuple.Item2 };
            }
            catch (BookNotFoundException)
            {
                return new List<Book>();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Error searching by work OL ID {0}", workId);
                return new List<Book>();
            }
        }

        // ── Mapping ───────────────────────────────────────────────────────

        private Author MapAuthor(OLAuthorResource resource, List<OLWorkResource> works)
        {
            var authorId = ExtractOlId(resource.Key);
            var metadata = MapAuthorMetadata(resource);

            var books = new List<Book>();
            foreach (var work in works)
            {
                try
                {
                    var workId = ExtractOlId(work.Key);
                    if (workId.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    var editionsResource = FetchJson<OLWorkEditionsResponse>($"/works/{workId}/editions.json?limit={EditionsPerWork}");
                    var editions = editionsResource?.Entries ?? new List<OLEditionResource>();
                    var book = MapWork(work, editions);
                    book.AuthorMetadata = metadata;
                    books.Add(book);
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Error fetching editions for work {0}", work.Key);
                }
            }

            var authorDict = new Dictionary<string, AuthorMetadata> { { authorId, metadata } };
            foreach (var book in books)
            {
                AddDbIds(authorId, book, authorDict);
            }

            return new Author
            {
                Metadata = metadata,
                CleanName = Parser.Parser.CleanAuthorName(metadata.Name),
                Books = books,
                Series = new List<Series>()
            };
        }

        private static AuthorMetadata MapAuthorMetadata(OLAuthorResource resource)
        {
            var authorId = ExtractOlId(resource.Key);
            var metadata = new AuthorMetadata
            {
                ForeignAuthorId = authorId,
                TitleSlug = authorId,
                Name = (resource.Name ?? "Unknown").CleanSpaces(),
                Overview = resource.Bio,
                Ratings = new Ratings { Votes = 0, Value = 0 },
                Status = AuthorStatusType.Continuing
            };

            metadata.SortName = metadata.Name.ToLower();
            metadata.NameLastFirst = metadata.Name.ToLastFirst();
            metadata.SortNameLastFirst = metadata.NameLastFirst.ToLower();

            if (resource.Photos?.Any() == true)
            {
                var photoId = resource.Photos.First();
                if (photoId > 0)
                {
                    metadata.Images.Add(new MediaCover.MediaCover
                    {
                        Url = $"https://covers.openlibrary.org/a/id/{photoId}-L.jpg",
                        CoverType = MediaCoverTypes.Poster
                    });
                }
            }

            metadata.Links.Add(new Links
            {
                Url = $"https://openlibrary.org/authors/{authorId}",
                Name = "OpenLibrary"
            });

            return metadata;
        }

        private Book MapWork(OLWorkResource resource, List<OLEditionResource> editionResources)
        {
            var workId = ExtractOlId(resource.Key);

            var book = new Book
            {
                ForeignBookId = workId,
                Title = resource.Title ?? "Unknown",
                TitleSlug = workId,
                CleanTitle = Parser.Parser.CleanAuthorName(resource.Title ?? string.Empty),
                Genres = resource.Subjects?.Take(5).ToList() ?? new List<string>(),
                RelatedBooks = new List<int>()
            };

            book.Links.Add(new Links
            {
                Url = $"https://openlibrary.org/works/{workId}",
                Name = "OpenLibrary Editions"
            });

            var editions = editionResources.Select(e => MapEdition(e, resource)).ToList();
            book.Editions = editions;

            if (editions.Any())
            {
                // Monitor the edition with the most pages (best proxy for "complete" edition)
                var best = editions.OrderByDescending(e => e.PageCount).First();
                best.Monitored = true;

                if (book.Title.IsNullOrWhiteSpace())
                {
                    book.Title = best.Title;
                }

                // Use earliest publish date as book release date
                var dated = editions.Where(e => e.ReleaseDate.HasValue).ToList();
                if (dated.Any())
                {
                    book.ReleaseDate = dated.Min(e => e.ReleaseDate);
                }
            }

            book.Ratings = new Ratings { Votes = 0, Value = 0 };
            book.AnyEditionOk = true;

            return book;
        }

        private static Edition MapEdition(OLEditionResource resource, OLWorkResource work)
        {
            var editionId = ExtractOlId(resource.Key);

            DateTime? releaseDate = null;
            if (resource.PublishDate.IsNotNullOrWhiteSpace())
            {
                if (DateTime.TryParse(resource.PublishDate, out var parsed))
                {
                    releaseDate = parsed;
                }
                else if (int.TryParse(resource.PublishDate, out var year))
                {
                    releaseDate = new DateTime(year, 1, 1);
                }
            }

            var langKey = resource.Languages?.FirstOrDefault()?.Key;
            var language = langKey != null ? ExtractOlId(langKey) : null;

            var edition = new Edition
            {
                ForeignEditionId = editionId,
                TitleSlug = editionId,
                Title = (resource.Title ?? work.Title ?? "Unknown").CleanSpaces(),
                Isbn13 = resource.Isbn13?.FirstOrDefault(),
                Asin = resource.Identifiers?.Amazon?.FirstOrDefault(),
                Publisher = resource.Publishers?.FirstOrDefault(),
                PageCount = resource.NumberOfPages ?? 0,
                ReleaseDate = releaseDate,
                Language = language,
                Format = resource.PhysicalFormat,
                IsEbook = resource.PhysicalFormat?.ToLowerInvariant().Contains("ebook") ?? false,
                Overview = resource.Description,
                Ratings = new Ratings { Votes = 0, Value = 0 }
            };

            var coverId = resource.Covers?.FirstOrDefault(c => c > 0);
            if (coverId.HasValue)
            {
                edition.Images.Add(new MediaCover.MediaCover
                {
                    Url = $"https://covers.openlibrary.org/b/id/{coverId.Value}-L.jpg",
                    CoverType = MediaCoverTypes.Cover
                });
            }

            edition.Links.Add(new Links
            {
                Url = $"https://openlibrary.org/books/{editionId}",
                Name = "OpenLibrary Book"
            });

            return edition;
        }

        private Book MapSearchDoc(OLSearchDoc doc)
        {
            var workId = ExtractOlId(doc.Key);
            if (workId.IsNullOrWhiteSpace())
            {
                return null;
            }

            var authorId = doc.AuthorKey?.FirstOrDefault() != null
                ? ExtractOlId(doc.AuthorKey.First())
                : "unknown";

            var authorName = doc.AuthorName?.FirstOrDefault() ?? "Unknown";

            var metadata = new AuthorMetadata
            {
                ForeignAuthorId = authorId,
                TitleSlug = authorId,
                Name = authorName,
                SortName = authorName.ToLower(),
                Ratings = new Ratings { Votes = 0, Value = 0 },
                Status = AuthorStatusType.Continuing
            };
            metadata.NameLastFirst = metadata.Name.ToLastFirst();
            metadata.SortNameLastFirst = metadata.NameLastFirst.ToLower();

            var edition = new Edition
            {
                ForeignEditionId = workId,
                TitleSlug = workId,
                Title = (doc.Title ?? "Unknown").CleanSpaces(),
                PageCount = doc.PageCount ?? 0,
                Monitored = true,
                Ratings = new Ratings { Votes = 0, Value = 0 }
            };

            if (doc.CoverId.HasValue && doc.CoverId.Value > 0)
            {
                edition.Images.Add(new MediaCover.MediaCover
                {
                    Url = $"https://covers.openlibrary.org/b/id/{doc.CoverId.Value}-L.jpg",
                    CoverType = MediaCoverTypes.Cover
                });
            }

            edition.Links.Add(new Links
            {
                Url = $"https://openlibrary.org/works/{workId}",
                Name = "OpenLibrary Book"
            });

            var releaseDate = doc.FirstPublishYear.HasValue
                ? new DateTime(doc.FirstPublishYear.Value, 1, 1)
                : (DateTime?)null;

            var book = new Book
            {
                ForeignBookId = workId,
                Title = doc.Title ?? "Unknown",
                TitleSlug = workId,
                CleanTitle = Parser.Parser.CleanAuthorName(doc.Title ?? string.Empty),
                ReleaseDate = releaseDate,
                Genres = doc.Subject?.Take(5).ToList() ?? new List<string>(),
                RelatedBooks = new List<int>(),
                Editions = new List<Edition> { edition },
                Ratings = new Ratings { Votes = 0, Value = 0 },
                AnyEditionOk = true,
                AuthorMetadata = metadata
            };

            book.Links.Add(new Links
            {
                Url = $"https://openlibrary.org/works/{workId}",
                Name = "OpenLibrary Editions"
            });

            var author = new Author
            {
                Metadata = metadata,
                CleanName = Parser.Parser.CleanAuthorName(authorName),
                Books = new List<Book> { book },
                Series = new List<Series>()
            };

            book.Author = author;
            book.AuthorMetadataId = 0;

            AddDbIds(authorId, book, new Dictionary<string, AuthorMetadata> { { authorId, metadata } });

            return book;
        }

        // ── DB enrichment (same logic as original BookInfoProxy) ──────────

        private void AddDbIds(string authorId, Book book, Dictionary<string, AuthorMetadata> authors)
        {
            var dbBook = _bookService.FindById(book.ForeignBookId);
            if (dbBook != null)
            {
                book.UseDbFieldsFrom(dbBook);

                var dbEditions = _editionService.GetEditionsByBook(dbBook.Id).ToDictionary(x => x.ForeignEditionId);

                foreach (var edition in book.Editions.Value)
                {
                    edition.Monitored = false;
                    if (dbEditions.TryGetValue(edition.ForeignEditionId, out var dbEdition))
                    {
                        edition.UseDbFieldsFrom(dbEdition);
                    }
                }

                if (book.Editions.Value.Any() && !book.Editions.Value.Any(x => x.Monitored))
                {
                    book.Editions.Value.First().Monitored = true;
                }
            }

            var dbAuthor = _authorService.FindById(authorId);
            if (dbAuthor == null)
            {
                if (!authors.TryGetValue(authorId, out var metadata))
                {
                    _logger.Warn("No author metadata found for id {0} in book {1}", authorId, book.ForeignBookId);
                    metadata = new AuthorMetadata
                    {
                        ForeignAuthorId = authorId,
                        Name = "Unknown"
                    };
                }

                dbAuthor = new Author
                {
                    CleanName = Parser.Parser.CleanAuthorName(metadata.Name),
                    Metadata = metadata
                };
            }

            book.Author = dbAuthor;
            book.AuthorMetadata = dbAuthor.Metadata.Value;
            book.AuthorMetadataId = dbAuthor.AuthorMetadataId;
        }

        // ── HTTP helpers ──────────────────────────────────────────────────

        private T FetchJson<T>(string path) where T : class
        {
            var url = BaseUrl + path;
            var request = new HttpRequest(url);
            request.SuppressHttpError = true;

            HttpResponse response;
            for (var attempt = 0; attempt < 3; attempt++)
            {
                response = _httpClient.Get(request);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.ContainsKey("Retry-After")
                        ? int.TryParse(response.Headers["Retry-After"], out var s) ? s : 5
                        : 5;
                    _logger.Info("OpenLibrary rate-limited, backing off {0}s", retryAfter);
                    Thread.Sleep(TimeSpan.FromSeconds(retryAfter));
                    continue;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (response.HasHttpError)
                {
                    _logger.Warn("OpenLibrary returned {0} for {1}", response.StatusCode, url);
                    return null;
                }

                return JsonSerializer.Deserialize<T>(response.Content, JsonOptions);
            }

            _logger.Warn("OpenLibrary: exhausted retries for {0}", url);
            return null;
        }

        private static string ExtractOlId(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return null;
            }

            // "/authors/OL23919A" → "OL23919A"
            var parts = key.TrimStart('/').Split('/');
            return parts.Length >= 2 ? parts[1] : parts[0];
        }
    }
}
