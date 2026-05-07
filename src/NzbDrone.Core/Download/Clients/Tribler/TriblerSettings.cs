using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Tribler
{
    public class TriblerSettingsValidator : AbstractValidator<TriblerSettings>
    {
        public TriblerSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
        }
    }

    public class TriblerSettings : IProviderConfig
    {
        private static readonly TriblerSettingsValidator Validator = new TriblerSettingsValidator();

        public TriblerSettings()
        {
            Host = "localhost";
            Port = 52194;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Number)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "API Key", Privacy = PrivacyLevel.ApiKey, HelpText = "Tribler REST API key (set in tribler.conf)")]
        public string ApiKey { get; set; }

        [FieldDefinition(4, Label = "Download Directory", Type = FieldType.Textbox, HelpText = "Optional. Override the Tribler download directory.")]
        public string DownloadDirectory { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
