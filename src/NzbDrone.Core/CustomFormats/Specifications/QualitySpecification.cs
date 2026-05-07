using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class QualitySpecificationValidator : AbstractValidator<QualitySpecification>
    {
        public QualitySpecificationValidator()
        {
            RuleFor(c => c.Value).GreaterThanOrEqualTo(0);
        }
    }

    public enum BookQualityType
    {
        Unknown = 0,
        PDF = 1,
        MOBI = 2,
        EPUB = 3,
        AZW3 = 4,
        MP3 = 10,
        FLAC = 11,
        M4B = 12,
        UnknownAudio = 13,
    }

    public class QualitySpecification : CustomFormatSpecificationBase
    {
        private static readonly QualitySpecificationValidator Validator = new ();

        public override int Order => 11;
        public override string ImplementationName => "Quality";

        [FieldDefinition(1, Label = "Quality", Type = FieldType.Select, SelectOptions = typeof(BookQualityType))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return input.BookInfo?.Quality?.Quality?.Id == Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
