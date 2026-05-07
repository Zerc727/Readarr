using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class DiscographySpecification : CustomFormatSpecificationBase
    {
        public override int Order => 13;
        public override string ImplementationName => "Discography";

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return input.BookInfo?.Discography == true;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult();
        }
    }
}
