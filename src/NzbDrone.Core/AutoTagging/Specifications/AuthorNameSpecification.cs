using NzbDrone.Core.Books;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class AuthorNameSpecification : RegexAutoTagSpecificationBase
    {
        public override int Order => 1;
        public override string ImplementationName => "Author Name";

        protected override bool IsSatisfiedByWithoutNegate(Author author)
        {
            return MatchString(author.Name);
        }
    }
}
