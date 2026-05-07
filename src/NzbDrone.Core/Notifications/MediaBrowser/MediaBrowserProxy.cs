using System.Net;
using System.Net.Http;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.MediaBrowser
{
    public interface IMediaBrowserProxy
    {
        void SendNotification(string title, string message, MediaBrowserSettings settings);
    }

    public class MediaBrowserProxy : IMediaBrowserProxy
    {
        private readonly IHttpClient _httpClient;

        public MediaBrowserProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SendNotification(string title, string message, MediaBrowserSettings settings)
        {
            try
            {
                var scheme = settings.UseSsl ? "https" : "http";
                var url = $"{scheme}://{settings.Host}:{settings.Port}/Notifications/Admin?api_key={settings.ApiKey}";

                var request = new HttpRequestBuilder(url)
                    .Accept(HttpAccept.Json)
                    .Build();

                request.Method = HttpMethod.Post;
                request.Headers.ContentType = "application/json";
                request.SetContent(new MediaBrowserNotificationPayload
                {
                    Name = title,
                    Description = message,
                    ImageUrl = string.Empty,
                    Url = string.Empty
                }.ToJson());

                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new MediaBrowserException("Unauthorized - API key is invalid");
                }

                throw new MediaBrowserException("Unable to connect to MediaBrowser. Status Code: {0}", ex);
            }
        }
    }

    public class MediaBrowserNotificationPayload
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
    }
}
