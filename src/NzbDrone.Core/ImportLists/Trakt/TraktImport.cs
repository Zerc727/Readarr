using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public class TraktImport : ImportListBase<TraktSettings>
    {
        private readonly ITraktProxy _proxy;

        public override string Name => "Trakt";
        public override ImportListType ListType => ImportListType.Other;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);

        public TraktImport(ITraktProxy proxy,
                           IImportListStatusService importListStatusService,
                           IConfigService configService,
                           IParsingService parsingService,
                           Logger logger)
            : base(importListStatusService, configService, parsingService, logger)
        {
            _proxy = proxy;
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            var items = new List<TraktListItem>();

            try
            {
                switch ((TraktListType)Settings.ListType)
                {
                    case TraktListType.CustomList:
                        items = _proxy.GetCustomListItems(Settings.Username, Settings.ListName, Settings);
                        break;
                    default:
                        items = _proxy.GetWatchlistItems(Settings);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to fetch items from Trakt");
                return new List<ImportListItemInfo>();
            }

            return CleanupListItems(MapItems(items));
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                _proxy.GetWatchlistItems(Settings);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to Trakt");
                return new ValidationFailure(string.Empty, "Unable to connect to Trakt, check the log for more details");
            }

            return null;
        }

        private static IEnumerable<ImportListItemInfo> MapItems(IEnumerable<TraktListItem> items)
        {
            return items
                .Select(item =>
                {
                    if (item.Type == "person" && item.Person != null)
                    {
                        return new ImportListItemInfo { Author = item.Person.Name };
                    }

                    if (item.Type == "movie" && item.Movie != null)
                    {
                        return new ImportListItemInfo { Book = item.Movie.Title };
                    }

                    if (item.Type == "show" && item.Show != null)
                    {
                        return new ImportListItemInfo { Book = item.Show.Title };
                    }

                    return null;
                })
                .Where(x => x != null)
                .ToList();
        }
    }
}
