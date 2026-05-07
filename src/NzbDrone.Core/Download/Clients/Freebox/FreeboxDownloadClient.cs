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

namespace NzbDrone.Core.Download.Clients.Freebox
{
    public class FreeboxDownloadClient : TorrentClientBase<FreeboxSettings>
    {
        private readonly IFreeboxProxy _proxy;

        public FreeboxDownloadClient(IFreeboxProxy proxy,
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

        public override string Name => "Freebox Download";

        protected override string AddFromTorrentFile(RemoteBook remoteBook, string hash, string filename, byte[] fileContent)
        {
            return _proxy.AddByTorrent(fileContent, Settings) ?? hash;
        }

        protected override string AddFromMagnetLink(RemoteBook remoteBook, string hash, string magnetLink)
        {
            return _proxy.AddByUrl(magnetLink, Settings) ?? hash;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var downloads = _proxy.GetDownloads(Settings);

            foreach (var dl in downloads)
            {
                var status = dl.Status switch
                {
                    "downloading" => DownloadItemStatus.Downloading,
                    "seeding" => DownloadItemStatus.Completed,
                    "done" => DownloadItemStatus.Completed,
                    "stopped" => DownloadItemStatus.Paused,
                    "queued" or "checking" => DownloadItemStatus.Queued,
                    "error" => DownloadItemStatus.Failed,
                    _ => DownloadItemStatus.Downloading
                };

                var outputPath = dl.DownloadDir.IsNotNullOrWhiteSpace()
                    ? _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(dl.DownloadDir))
                    : default;

                yield return new DownloadClientItem
                {
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false),
                    DownloadId = dl.InfoHash?.ToUpper() ?? dl.Id.ToString(),
                    Title = dl.Name ?? string.Empty,
                    TotalSize = dl.Size,
                    RemainingSize = dl.Size - dl.RxBytes,
                    RemainingTime = dl.Eta > 0 ? TimeSpan.FromSeconds(dl.Eta) : (TimeSpan?)null,
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
            if (int.TryParse(item.DownloadId, out var id))
            {
                _proxy.RemoveDownload(id, deleteData, Settings);
            }
            else
            {
                var downloads = _proxy.GetDownloads(Settings);
                foreach (var d in downloads)
                {
                    if (string.Equals(d.InfoHash, item.DownloadId, StringComparison.OrdinalIgnoreCase))
                    {
                        _proxy.RemoveDownload(d.Id, deleteData, Settings);
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
                failures.Add(new NzbDroneValidationFailure("Host", "Unable to connect to Freebox")
                {
                    DetailedDescription = ex.Message
                });
            }
        }
    }
}
