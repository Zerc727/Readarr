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

namespace NzbDrone.Core.Download.Clients.RQBit
{
    public class RQBit : TorrentClientBase<RQBitSettings>
    {
        private readonly IRQBitProxy _proxy;

        public RQBit(IRQBitProxy proxy,
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

        public override string Name => "rqbit";

        protected override string AddFromTorrentFile(RemoteBook remoteBook, string hash, string filename, byte[] fileContent)
        {
            return _proxy.AddTorrentByFile(fileContent, Settings) ?? hash;
        }

        protected override string AddFromMagnetLink(RemoteBook remoteBook, string hash, string magnetLink)
        {
            return _proxy.AddTorrentByMagnet(magnetLink, Settings) ?? hash;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var torrents = _proxy.GetTorrents(Settings);

            foreach (var torrent in torrents)
            {
                var stats = torrent.Stats ?? new RQBitStats();
                var total = stats.TotalBytes;
                var done = stats.ProgressBytes;

                var status = stats.State switch
                {
                    "live" or "downloading" => DownloadItemStatus.Downloading,
                    "seeding" => DownloadItemStatus.Completed,
                    "paused" => DownloadItemStatus.Paused,
                    "initializing" => DownloadItemStatus.Queued,
                    _ => DownloadItemStatus.Failed
                };

                if (status == DownloadItemStatus.Downloading && total > 0 && done == total)
                {
                    status = DownloadItemStatus.Completed;
                }

                yield return new DownloadClientItem
                {
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false),
                    DownloadId = torrent.InfoHash?.ToUpper() ?? torrent.Id.ToString(),
                    Title = torrent.Name ?? string.Empty,
                    TotalSize = total,
                    RemainingSize = total - done,
                    RemainingTime = stats.DownloadSpeed > 0 ? TimeSpan.FromSeconds((total - done) / stats.DownloadSpeed) : (TimeSpan?)null,
                    SeedRatio = total > 0 ? (double)stats.UploadedBytes / total : 0,
                    Status = status,
                    CanMoveFiles = status == DownloadItemStatus.Completed,
                    CanBeRemoved = status == DownloadItemStatus.Completed,
                    OutputPath = torrent.OutputFolder.IsNotNullOrWhiteSpace()
                        ? _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.OutputFolder))
                        : default
                };
            }
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            if (int.TryParse(item.DownloadId, out var id))
            {
                _proxy.RemoveTorrent(id, deleteData, Settings);
            }
            else
            {
                var torrents = _proxy.GetTorrents(Settings);
                foreach (var t in torrents)
                {
                    if (string.Equals(t.InfoHash, item.DownloadId, StringComparison.OrdinalIgnoreCase))
                    {
                        _proxy.RemoveTorrent(t.Id, deleteData, Settings);
                        break;
                    }
                }
            }

            if (deleteData)
            {
                DeleteItemData(item);
            }
        }

        public override DownloadClientInfo GetStatus()
        {
            return new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "::1" || Settings.Host == "localhost",
                OutputRootFolders = Settings.OutputDirectory.IsNotNullOrWhiteSpace()
                    ? new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(Settings.OutputDirectory)) }
                    : new List<OsPath>()
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            try
            {
                _proxy.GetTorrents(Settings);
            }
            catch (Exception ex)
            {
                failures.Add(new NzbDroneValidationFailure("Host", "Unable to connect to rqbit")
                {
                    DetailedDescription = ex.Message
                });
            }
        }
    }
}
