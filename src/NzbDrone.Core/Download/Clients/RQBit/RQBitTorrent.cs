using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.RQBit
{
    public class RQBitTorrentList
    {
        public List<RQBitTorrent> Torrents { get; set; } = new List<RQBitTorrent>();
    }

    public class RQBitTorrent
    {
        public int Id { get; set; }
        public string InfoHash { get; set; }
        public string Name { get; set; }
        public string OutputFolder { get; set; }
        public RQBitStats Stats { get; set; }
    }

    public class RQBitStats
    {
        public long TotalBytes { get; set; }
        public long ProgressBytes { get; set; }
        public long UploadedBytes { get; set; }
        public double DownloadSpeed { get; set; }
        public string State { get; set; }
    }

    public class RQBitAddResponse
    {
        public string Id { get; set; }
        public string InfoHash { get; set; }
    }
}
