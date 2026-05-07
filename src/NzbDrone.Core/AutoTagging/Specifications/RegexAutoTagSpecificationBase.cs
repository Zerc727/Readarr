using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoTagging.Specifications
{
    public class RegexAutoTagSpecificationBaseValidator : AbstractValidator<RegexAutoTagSpecificationBase>
    {
        public RegexAutoTagSpecificationBaseValidator()
        {
            RuleFor(c => c.Value).NotEmpty().WithMessage("Regex Pattern must not be empty");
        }
    }

    public abstract class RegexAutoTagSpecificationBase : AutoTagSpecificationBase
    {
        private static readonly RegexAutoTagSpecificationBaseValidator Validator = new RegexAutoTagSpecificationBaseValidator();

        protected Regex _regex;
        protected string _raw;

        [FieldDefinition(1, Label = "Regular Expression", HelpText = "Regular expression to match")]
        public string Value
        {
            get => _raw;
            set
            {
                _raw = value;

                if (value.IsNotNullOrWhiteSpace())
                {
                    _regex = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
            }
        }

        protected bool MatchString(string compared)
        {
            if (compared == null || _regex == null)
            {
                return false;
            }

            return _regex.IsMatch(compared);
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
