namespace NzbDrone.Core.Download.Clients.Freebox
{
    public class FreeboxApiResponse<T>
    {
        public bool Success { get; set; }
        public T Result { get; set; }
        public string Msg { get; set; }
        public string Uid { get; set; }
    }

    public class FreeboxLoginResponse
    {
        public string Challenge { get; set; }
        public bool LoggedIn { get; set; }
        public string PasswordSalt { get; set; }
    }

    public class FreeboxSessionRequest
    {
        public string AppId { get; set; }
        public string AppVersion { get; set; }
        public string Password { get; set; }
    }

    public class FreeboxSessionResponse
    {
        public string SessionToken { get; set; }
    }

    public class FreeboxDownloadItem
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public long RxBytes { get; set; }
        public long TxBytes { get; set; }
        public long RxRate { get; set; }
        public long TxRate { get; set; }
        public int Eta { get; set; }
        public string DownloadDir { get; set; }
        public string InfoHash { get; set; }
        public string Error { get; set; }
        public double Ratio { get; set; }
    }

    public class FreeboxAddResponse
    {
        public int Id { get; set; }
        public string InfoHash { get; set; }
    }
}
