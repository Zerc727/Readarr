namespace NzbDrone.Core.CustomFormats
{
    public class BookTitleSpecification : RegexSpecificationBase
    {
        public override int Order => 6;
        public override string ImplementationName => "Book Title";

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return MatchString(input.BookInfo?.BookTitle);
        }
    }
}
