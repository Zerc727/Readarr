using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Books;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class RatingSpecificationValidator : AbstractValidator<RatingSpecification>
    {
        public RatingSpecificationValidator()
        {
            RuleFor(c => c.Min).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Max).GreaterThan(c => c.Min);
        }
    }

    public class RatingSpecification : AutoTagSpecificationBase
    {
        private static readonly RatingSpecificationValidator Validator = new ();

        public override int Order => 4;
        public override string ImplementationName => "Rating";

        [FieldDefinition(1, Label = "Minimum Rating", HelpText = "Rating from 0 to 10", Type = FieldType.Number)]
        public decimal Min { get; set; }

        [FieldDefinition(2, Label = "Maximum Rating", HelpText = "Rating from 0 to 10", Type = FieldType.Number)]
        public decimal Max { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Author author)
        {
            var rating = author.Metadata?.Value?.Ratings?.Value ?? 0;
            return rating >= Min && rating <= Max;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
