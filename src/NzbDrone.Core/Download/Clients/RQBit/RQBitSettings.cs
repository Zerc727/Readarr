using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.RQBit
{
    public class RQBitSettingsValidator : AbstractValidator<RQBitSettings>
    {
        public RQBitSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
        }
    }

    public class RQBitSettings : IProviderConfig
    {
        private static readonly RQBitSettingsValidator Validator = new RQBitSettingsValidator();

        public RQBitSettings()
        {
            Host = "localhost";
            Port = 3030;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Number)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Output Directory", Type = FieldType.Textbox, HelpText = "Optional. Override the default rqbit output directory.")]
        public string OutputDirectory { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
