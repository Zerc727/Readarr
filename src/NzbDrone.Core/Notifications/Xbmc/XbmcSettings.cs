using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class XbmcSettingsValidator : AbstractValidator<XbmcSettings>
    {
        public XbmcSettingsValidator()
        {
            RuleFor(c => c.Host).NotEmpty();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
        }
    }

    public class XbmcSettings : IProviderConfig
    {
        private static readonly XbmcSettingsValidator Validator = new XbmcSettingsValidator();

        public XbmcSettings()
        {
            Port = 8080;
            DisplayTime = 5;
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Number)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Username")]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Password", Privacy = PrivacyLevel.Password, Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Connect to Kodi/XBMC over HTTPS")]
        public bool UseSsl { get; set; }

        [FieldDefinition(5, Label = "Display Time", HelpText = "How long (in seconds) to display the notification", Type = FieldType.Number)]
        public int DisplayTime { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
