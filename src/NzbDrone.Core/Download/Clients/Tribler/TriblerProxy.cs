using System.Collections.Generic;
using System.Net.Http;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Download.Clients.Tribler
{
    public interface ITriblerProxy
    {
        List<TriblerDownload> GetDownloads(TriblerSettings settings);
        string AddByMagnet(string magnetUrl, TriblerSettings settings);
        string AddByTorrent(byte[] fileContent, TriblerSettings settings);
        void RemoveDownload(string infohash, bool deleteData, TriblerSettings settings);
    }

    public class TriblerProxy : ITriblerProxy
    {
        private readonly IHttpClient _httpClient;

        public TriblerProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<TriblerDownload> GetDownloads(TriblerSettings settings)
        {
            var request = BuildRequest(settings, "/api/v1/downloads").Build();
            var response = _httpClient.Get<TriblerDownloadList>(request);
            return response.Resource?.Downloads ?? new List<TriblerDownload>();
        }

        public string AddByMagnet(string magnetUrl, TriblerSettings settings)
        {
            var request = BuildRequest(settings, "/api/v1/downloads").Build();
            request.Method = HttpMethod.Put;
            request.Headers.ContentType = "application/json";
            request.SetContent(Json.ToJson(new
            {
                uri = magnetUrl,
                destination = settings.DownloadDirectory ?? string.Empty,
                anon_hops = 0
            }));

            var response = _httpClient.Execute(request);
            var result = Json.Deserialize<TriblerAddResponse>(response.Content);
            return result?.Infohash;
        }

        public string AddByTorrent(byte[] fileContent, TriblerSettings settings)
        {
            var url = BuildBaseUrl(settings) + "/api/v1/downloads";

            var request = new HttpRequest(url)
            {
                Method = HttpMethod.Put,
                ContentData = fileContent
            };
            request.Headers.ContentType = "application/x-bittorrent";

            if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                request.Headers.Add("X-Api-Key", settings.ApiKey);
            }

            var response = _httpClient.Execute(request);
            var result = Json.Deserialize<TriblerAddResponse>(response.Content);
            return result?.Infohash;
        }

        public void RemoveDownload(string infohash, bool deleteData, TriblerSettings settings)
        {
            var request = BuildRequest(settings, $"/api/v1/downloads/{infohash}").Build();
            request.Method = HttpMethod.Delete;
            request.SetContent(Json.ToJson(new { remove_data = deleteData }));
            _httpClient.Execute(request);
        }

        private string BuildBaseUrl(TriblerSettings settings)
        {
            var scheme = settings.UseSsl ? "https" : "http";
            return $"{scheme}://{settings.Host}:{settings.Port}";
        }

        private HttpRequestBuilder BuildRequest(TriblerSettings settings, string resource)
        {
            var builder = new HttpRequestBuilder(BuildBaseUrl(settings))
                .Resource(resource)
                .Accept(HttpAccept.Json);

            if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                builder = builder.SetHeader("X-Api-Key", settings.ApiKey);
            }

            return builder;
        }
    }

    public class TriblerAddResponse
    {
        public string Infohash { get; set; }
    }
}
