using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NzbDrone.Core.MetadataSource.OpenLibrary.Resources
{
    public class OLSearchResponse
    {
        [JsonPropertyName("docs")]
        public List<OLSearchDoc> Docs { get; set; } = new();

        [JsonPropertyName("numFound")]
        public int NumFound { get; set; }
    }

    public class OLSearchDoc
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("author_key")]
        public List<string> AuthorKey { get; set; } = new();

        [JsonPropertyName("author_name")]
        public List<string> AuthorName { get; set; } = new();

        [JsonPropertyName("cover_i")]
        public int? CoverId { get; set; }

        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("subject")]
        public List<string> Subject { get; set; } = new();

        [JsonPropertyName("isbn")]
        public List<string> Isbn { get; set; } = new();

        [JsonPropertyName("language")]
        public List<string> Language { get; set; } = new();

        [JsonPropertyName("number_of_pages_median")]
        public int? PageCount { get; set; }
    }
}
