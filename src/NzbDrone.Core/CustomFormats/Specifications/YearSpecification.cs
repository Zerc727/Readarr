using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class YearSpecificationValidator : AbstractValidator<YearSpecification>
    {
        public YearSpecificationValidator()
        {
            RuleFor(c => c.Min).GreaterThanOrEqualTo(1800);
            RuleFor(c => c.Max).GreaterThan(c => c.Min);
        }
    }

    public class YearSpecification : CustomFormatSpecificationBase
    {
        private static readonly YearSpecificationValidator Validator = new ();

        public override int Order => 12;
        public override string ImplementationName => "Year";

        [FieldDefinition(1, Label = "Minimum Year", HelpText = "Release must be published on or after this year", Type = FieldType.Number)]
        public int Min { get; set; }

        [FieldDefinition(2, Label = "Maximum Year", HelpText = "Release must be published on or before this year", Type = FieldType.Number)]
        public int Max { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            var releaseDate = input.BookInfo?.ReleaseDate;
            if (releaseDate == null)
            {
                return false;
            }

            if (!DateTime.TryParse(releaseDate, out var parsed))
            {
                return false;
            }

            return parsed.Year >= Min && parsed.Year <= Max;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
