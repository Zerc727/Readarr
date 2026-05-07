using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Freebox
{
    public class FreeboxSettingsValidator : AbstractValidator<FreeboxSettings>
    {
        public FreeboxSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.AppId).NotEmpty();
            RuleFor(c => c.AppToken).NotEmpty();
        }
    }

    public class FreeboxSettings : IProviderConfig
    {
        private static readonly FreeboxSettingsValidator Validator = new FreeboxSettingsValidator();

        public FreeboxSettings()
        {
            Host = "mafreebox.freebox.fr";
            Port = 80;
            AppId = "readarr";
        }

        [FieldDefinition(0, Label = "Host", HelpText = "Freebox hostname or IP (usually mafreebox.freebox.fr)")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Number)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "App ID", HelpText = "Application identifier registered with the Freebox")]
        public string AppId { get; set; }

        [FieldDefinition(4, Label = "App Token", Privacy = PrivacyLevel.ApiKey, HelpText = "Token obtained during app registration with the Freebox")]
        public string AppToken { get; set; }

        [FieldDefinition(5, Label = "Download Directory", HelpText = "Optional. Freebox download destination directory.")]
        public string DownloadDirectory { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
