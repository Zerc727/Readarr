namespace NzbDrone.Core.CustomFormats
{
    public class FilenameSpecification : RegexSpecificationBase
    {
        public override int Order => 7;
        public override string ImplementationName => "Filename";

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return MatchString(input.Filename);
        }
    }
}
