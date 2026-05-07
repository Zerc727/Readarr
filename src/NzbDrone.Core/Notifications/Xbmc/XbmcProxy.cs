using System;
using System.Net;
using System.Net.Http;
using System.Text;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public interface IXbmcProxy
    {
        void SendNotification(string title, string message, XbmcSettings settings);
    }

    public class XbmcProxy : IXbmcProxy
    {
        private readonly IHttpClient _httpClient;

        public XbmcProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SendNotification(string title, string message, XbmcSettings settings)
        {
            try
            {
                var scheme = settings.UseSsl ? "https" : "http";
                var url = $"{scheme}://{settings.Host}:{settings.Port}/jsonrpc";

                var request = new HttpRequestBuilder(url)
                    .Accept(HttpAccept.Json)
                    .Build();

                if (!string.IsNullOrWhiteSpace(settings.Username))
                {
                    var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}"));
                    request.Headers.Add("Authorization", $"Basic {credentials}");
                }

                request.Method = HttpMethod.Post;
                request.Headers.ContentType = "application/json";
                request.SetContent(new XbmcJsonRpcRequest
                {
                    JsonRpc = "2.0",
                    Method = "GUI.ShowNotification",
                    Params = new XbmcNotificationParams
                    {
                        Title = title,
                        Message = message,
                        DisplayTime = settings.DisplayTime * 1000
                    },
                    Id = 1
                }.ToJson());

                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new XbmcException("Unauthorized - credentials are invalid");
                }

                throw new XbmcException("Unable to connect to Kodi/XBMC. Status Code: {0}", ex);
            }
        }
    }

    public class XbmcJsonRpcRequest
    {
        public string JsonRpc { get; set; }
        public string Method { get; set; }
        public XbmcNotificationParams Params { get; set; }
        public int Id { get; set; }
    }

    public class XbmcNotificationParams
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public int DisplayTime { get; set; }
    }
}
