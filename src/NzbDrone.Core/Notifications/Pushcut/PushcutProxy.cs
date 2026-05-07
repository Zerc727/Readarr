using System;
using System.Net;
using System.Net.Http;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Notifications.Pushcut
{
    public interface IPushcutProxy
    {
        void SendNotification(string title, string message, PushcutSettings settings);
    }

    public class PushcutProxy : IPushcutProxy
    {
        private readonly IHttpClient _httpClient;

        public PushcutProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SendNotification(string title, string message, PushcutSettings settings)
        {
            try
            {
                var url = $"https://api.pushcut.io/v1/notifications/{Uri.EscapeDataString(settings.NotificationName)}";

                var request = new HttpRequestBuilder(url)
                    .Accept(HttpAccept.Json)
                    .Build();

                request.Method = HttpMethod.Post;
                request.Headers.ContentType = "application/json";
                request.Headers.Add("API-Key", settings.ApiKey);
                request.SetContent(new PushcutPayload
                {
                    Title = title,
                    Text = message
                }.ToJson());

                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new PushcutException("Unauthorized - API key is invalid");
                }

                throw new PushcutException("Unable to send Pushcut notification. Status Code: {0}", ex);
            }
        }
    }

    public class PushcutPayload
    {
        public string Title { get; set; }
        public string Text { get; set; }
    }
}
