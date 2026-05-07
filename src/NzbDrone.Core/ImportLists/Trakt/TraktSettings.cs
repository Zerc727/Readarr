using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public enum TraktListType
    {
        Watchlist = 0,
        CustomList = 1,
    }

    public class TraktSettingsValidator : AbstractValidator<TraktSettings>
    {
        public TraktSettingsValidator()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.ClientId).NotEmpty();
            RuleFor(c => c.ListName).NotEmpty().When(c => (TraktListType)c.ListType == TraktListType.CustomList);
        }
    }

    public class TraktSettings : IImportListSettings
    {
        private static readonly TraktSettingsValidator Validator = new TraktSettingsValidator();

        public TraktSettings()
        {
            Limit = 100;
        }

        public string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "Username", HelpText = "Trakt username to import from")]
        public string Username { get; set; }

        [FieldDefinition(1, Label = "Client ID", HelpText = "Trakt API client ID (from trakt.tv/oauth/applications)")]
        public string ClientId { get; set; }

        [FieldDefinition(2, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TraktListType), HelpText = "Type of Trakt list to import")]
        public int ListType { get; set; }

        [FieldDefinition(3, Label = "List Name", HelpText = "Trakt custom list slug (required when List Type is Custom List)", Advanced = true)]
        public string ListName { get; set; }

        [FieldDefinition(4, Label = "Limit", HelpText = "Maximum number of items to import (1-100)", Type = FieldType.Number, Advanced = true)]
        public int Limit { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
