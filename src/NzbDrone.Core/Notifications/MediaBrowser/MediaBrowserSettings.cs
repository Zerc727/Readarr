using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.MediaBrowser
{
    public class MediaBrowserSettingsValidator : AbstractValidator<MediaBrowserSettings>
    {
        public MediaBrowserSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class MediaBrowserSettings : IProviderConfig
    {
        private static readonly MediaBrowserSettingsValidator Validator = new MediaBrowserSettingsValidator();

        public MediaBrowserSettings()
        {
            Port = 8096;
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Number)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Connect to MediaBrowser over HTTPS")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "API Key", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
