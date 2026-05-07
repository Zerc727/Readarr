namespace NzbDrone.Core.CustomFormats
{
    public class ReleaseHashSpecification : RegexSpecificationBase
    {
        public override int Order => 10;
        public override string ImplementationName => "Release Hash";

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return MatchString(input.BookInfo?.ReleaseHash);
        }
    }
}
