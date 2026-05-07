namespace NzbDrone.Core.CustomFormats
{
    public class ReleaseVersionSpecification : RegexSpecificationBase
    {
        public override int Order => 9;
        public override string ImplementationName => "Release Version";

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return MatchString(input.BookInfo?.ReleaseVersion);
        }
    }
}
