namespace NzbDrone.Core.CustomFormats
{
    public class AuthorNameSpecification : RegexSpecificationBase
    {
        public override int Order => 5;
        public override string ImplementationName => "Author Name";

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return MatchString(input.BookInfo?.AuthorName);
        }
    }
}
