using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Pushcut
{
    public class PushcutSettingsValidator : AbstractValidator<PushcutSettings>
    {
        public PushcutSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.NotificationName).NotEmpty();
        }
    }

    public class PushcutSettings : IProviderConfig
    {
        private static readonly PushcutSettingsValidator Validator = new PushcutSettingsValidator();

        [FieldDefinition(0, Label = "API Key", Privacy = PrivacyLevel.ApiKey, HelpText = "Pushcut API key from the Pushcut app")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "Notification Name", HelpText = "Name of the Pushcut notification to trigger")]
        public string NotificationName { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
