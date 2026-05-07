using System.Linq;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class GenreSpecification : RegexAutoTagSpecificationBase
    {
        public override int Order => 2;
        public override string ImplementationName => "Genre";

        protected override bool IsSatisfiedByWithoutNegate(Author author)
        {
            var genres = author.Metadata?.Value?.Genres;
            return genres != null && genres.Any(g => MatchString(g));
        }
    }
}
