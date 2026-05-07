using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Books;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class StatusSpecificationValidator : AbstractValidator<StatusSpecification>
    {
        public StatusSpecificationValidator()
        {
            RuleFor(c => c.Value).Custom((value, context) =>
            {
                if (!Enum.IsDefined(typeof(AuthorStatusType), value))
                {
                    context.AddFailure($"Invalid author status value: {value}");
                }
            });
        }
    }

    public class StatusSpecification : AutoTagSpecificationBase
    {
        private static readonly StatusSpecificationValidator Validator = new ();

        public override int Order => 3;
        public override string ImplementationName => "Status";

        [FieldDefinition(1, Label = "Status", Type = FieldType.Select, SelectOptions = typeof(AuthorStatusType))]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(Author author)
        {
            return (int)(author.Metadata?.Value?.Status ?? AuthorStatusType.Continuing) == Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
