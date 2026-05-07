using System;
using System.Collections.Generic;
using System.Net.Http;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Download.Clients.RQBit
{
    public interface IRQBitProxy
    {
        List<RQBitTorrent> GetTorrents(RQBitSettings settings);
        string AddTorrentByFile(byte[] fileContent, RQBitSettings settings);
        string AddTorrentByMagnet(string magnetUrl, RQBitSettings settings);
        void RemoveTorrent(int id, bool deleteData, RQBitSettings settings);
    }

    public class RQBitProxy : IRQBitProxy
    {
        private readonly IHttpClient _httpClient;

        public RQBitProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<RQBitTorrent> GetTorrents(RQBitSettings settings)
        {
            var request = BuildRequest(settings, "/api/v3/torrents").Build();
            var response = _httpClient.Get<RQBitTorrentList>(request);
            return response.Resource?.Torrents ?? new List<RQBitTorrent>();
        }

        public string AddTorrentByFile(byte[] fileContent, RQBitSettings settings)
        {
            var url = BuildBaseUrl(settings) + "/api/v3/torrents";

            if (!string.IsNullOrWhiteSpace(settings.OutputDirectory))
            {
                url += $"?output_folder={Uri.EscapeDataString(settings.OutputDirectory)}";
            }

            var request = new HttpRequest(url)
            {
                Method = HttpMethod.Post,
                ContentData = fileContent
            };
            request.Headers.ContentType = "application/x-bittorrent";

            var response = _httpClient.Execute(request);
            var result = Json.Deserialize<RQBitAddResponse>(response.Content);
            return result.InfoHash;
        }

        public string AddTorrentByMagnet(string magnetUrl, RQBitSettings settings)
        {
            var url = BuildBaseUrl(settings) + "/api/v3/torrents";

            if (!string.IsNullOrWhiteSpace(settings.OutputDirectory))
            {
                url += $"?output_folder={Uri.EscapeDataString(settings.OutputDirectory)}";
            }

            var request = new HttpRequestBuilder(url)
                .Accept(HttpAccept.Json)
                .Build();

            request.Method = HttpMethod.Post;
            request.Headers.ContentType = "application/json";
            request.SetContent(Json.ToJson(new { url = magnetUrl }));

            var response = _httpClient.Execute(request);
            var result = Json.Deserialize<RQBitAddResponse>(response.Content);
            return result.InfoHash;
        }

        public void RemoveTorrent(int id, bool deleteData, RQBitSettings settings)
        {
            var request = BuildRequest(settings, $"/api/v3/torrents/{id}")
                .Build();
            request.Method = HttpMethod.Delete;
            _httpClient.Execute(request);
        }

        private string BuildBaseUrl(RQBitSettings settings)
        {
            var scheme = settings.UseSsl ? "https" : "http";
            return $"{scheme}://{settings.Host}:{settings.Port}";
        }

        private HttpRequestBuilder BuildRequest(RQBitSettings settings, string resource)
        {
            return new HttpRequestBuilder(BuildBaseUrl(settings))
                .Resource(resource)
                .Accept(HttpAccept.Json);
        }
    }
}
