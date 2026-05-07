using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Tribler
{
    public class Tribler : TorrentClientBase<TriblerSettings>
    {
        private readonly ITriblerProxy _proxy;

        public Tribler(ITriblerProxy proxy,
                       ITorrentFileInfoReader torrentFileInfoReader,
                       IHttpClient httpClient,
                       IConfigService configService,
                       IDiskProvider diskProvider,
                       IRemotePathMappingService remotePathMappingService,
                       IBlocklistService blocklistService,
                       Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, blocklistService, logger)
        {
            _proxy = proxy;
        }

        public override string Name => "Tribler";

        protected override string AddFromTorrentFile(RemoteBook remoteBook, string hash, string filename, byte[] fileContent)
        {
            return _proxy.AddByTorrent(fileContent, Settings) ?? hash;
        }

        protected override string AddFromMagnetLink(RemoteBook remoteBook, string hash, string magnetLink)
        {
            return _proxy.AddByMagnet(magnetLink, Settings) ?? hash;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var downloads = _proxy.GetDownloads(Settings);

            foreach (var dl in downloads)
            {
                var total = dl.Size;
                var done = (long)(dl.Size * dl.Progress);

                var status = dl.Status switch
                {
                    "DLSTATUS_DOWNLOADING" => DownloadItemStatus.Downloading,
                    "DLSTATUS_SEEDING" => DownloadItemStatus.Completed,
                    "DLSTATUS_STOPPED" => DownloadItemStatus.Paused,
                    "DLSTATUS_STOPPED_ON_ERROR" => DownloadItemStatus.Failed,
                    "DLSTATUS_WAITING4HASHCHECK" or "DLSTATUS_HASHCHECKING" or "DLSTATUS_METADATA" => DownloadItemStatus.Queued,
                    _ => DownloadItemStatus.Downloading
                };

                var outputPath = dl.Destination.IsNotNullOrWhiteSpace()
                    ? _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(dl.Destination))
                    : default;

                yield return new DownloadClientItem
                {
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false),
                    DownloadId = dl.Infohash?.ToUpper() ?? string.Empty,
                    Title = dl.Name ?? string.Empty,
                    TotalSize = total,
                    RemainingSize = total - done,
                    RemainingTime = dl.SpeedDown > 0 ? TimeSpan.FromSeconds((total - done) / dl.SpeedDown) : (TimeSpan?)null,
                    SeedRatio = dl.Ratio,
                    Status = status,
                    Message = dl.Error,
                    CanMoveFiles = status == DownloadItemStatus.Completed,
                    CanBeRemoved = status == DownloadItemStatus.Completed,
                    OutputPath = outputPath
                };
            }
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            _proxy.RemoveDownload(item.DownloadId.ToLower(), deleteData, Settings);

            if (deleteData)
            {
                DeleteItemData(item);
            }
        }

        public override DownloadClientInfo GetStatus()
        {
            var folders = new List<OsPath>();

            if (Settings.DownloadDirectory.IsNotNullOrWhiteSpace())
            {
                folders.Add(_remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(Settings.DownloadDirectory)));
            }

            return new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "::1" || Settings.Host == "localhost",
                OutputRootFolders = folders
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            try
            {
                _proxy.GetDownloads(Settings);
            }
            catch (Exception ex)
            {
                failures.Add(new NzbDroneValidationFailure("Host", "Unable to connect to Tribler")
                {
                    DetailedDescription = ex.Message
                });
            }
        }
    }
}
