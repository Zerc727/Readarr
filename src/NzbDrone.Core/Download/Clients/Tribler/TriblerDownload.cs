using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Tribler
{
    public class TriblerDownloadList
    {
        public List<TriblerDownload> Downloads { get; set; } = new List<TriblerDownload>();
    }

    public class TriblerDownload
    {
        public string Infohash { get; set; }
        public string Name { get; set; }
        public double Progress { get; set; }
        public long Size { get; set; }
        public double SpeedDown { get; set; }
        public double SpeedUp { get; set; }
        public string Status { get; set; }
        public string Destination { get; set; }
        public double Ratio { get; set; }
        public string Error { get; set; }
    }
}
