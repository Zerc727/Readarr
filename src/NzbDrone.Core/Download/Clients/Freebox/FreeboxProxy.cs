using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Download.Clients.Freebox
{
    public interface IFreeboxProxy
    {
        List<FreeboxDownloadItem> GetDownloads(FreeboxSettings settings);
        string AddByUrl(string url, FreeboxSettings settings);
        string AddByTorrent(byte[] fileContent, FreeboxSettings settings);
        void RemoveDownload(int id, bool deleteData, FreeboxSettings settings);
    }

    public class FreeboxProxy : IFreeboxProxy
    {
        private readonly IHttpClient _httpClient;

        public FreeboxProxy(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<FreeboxDownloadItem> GetDownloads(FreeboxSettings settings)
        {
            var token = GetSessionToken(settings);
            var request = BuildRequest(settings, "/api/v6/downloads/", token).Build();
            var response = _httpClient.Get<FreeboxApiResponse<List<FreeboxDownloadItem>>>(request);
            return response.Resource?.Result ?? new List<FreeboxDownloadItem>();
        }

        public string AddByUrl(string url, FreeboxSettings settings)
        {
            var token = GetSessionToken(settings);
            var request = BuildRequest(settings, "/api/v6/downloads/add", token).Build();
            request.Method = HttpMethod.Post;
            request.Headers.ContentType = "application/json";
            request.SetContent(Json.ToJson(new { download_url = url }));

            var response = _httpClient.Execute(request);
            var result = Json.Deserialize<FreeboxApiResponse<FreeboxAddResponse>>(response.Content);
            return result?.Result?.InfoHash;
        }

        public string AddByTorrent(byte[] fileContent, FreeboxSettings settings)
        {
            var token = GetSessionToken(settings);
            var url = BuildBaseUrl(settings) + "/api/v6/downloads/add";

            var request = new HttpRequest(url)
            {
                Method = HttpMethod.Post,
                ContentData = fileContent
            };
            request.Headers.ContentType = "application/x-bittorrent";
            request.Headers.Add("X-Fbx-App-Auth", token);

            var response = _httpClient.Execute(request);
            var result = Json.Deserialize<FreeboxApiResponse<FreeboxAddResponse>>(response.Content);
            return result?.Result?.InfoHash;
        }

        public void RemoveDownload(int id, bool deleteData, FreeboxSettings settings)
        {
            var token = GetSessionToken(settings);
            var request = BuildRequest(settings, $"/api/v6/downloads/{id}", token).Build();
            request.Method = HttpMethod.Delete;
            _httpClient.Execute(request);
        }

        private string GetSessionToken(FreeboxSettings settings)
        {
            // Get challenge
            var loginRequest = BuildRequest(settings, "/api/v6/login/").Build();
            var loginResponse = _httpClient.Get<FreeboxApiResponse<FreeboxLoginResponse>>(loginRequest);
            var challenge = loginResponse.Resource?.Result?.Challenge
                ?? throw new Exception("Failed to get Freebox login challenge");

            // Compute HMAC-SHA1 password
            var password = ComputeHmacSha1(settings.AppToken, challenge);

            // Open session
            var sessionRequest = BuildRequest(settings, "/api/v6/login/session/").Build();
            sessionRequest.Method = HttpMethod.Post;
            sessionRequest.Headers.ContentType = "application/json";
            sessionRequest.SetContent(Json.ToJson(new FreeboxSessionRequest
            {
                AppId = settings.AppId,
                AppVersion = "1.0.0",
                Password = password
            }));

            var sessionResponse = _httpClient.Execute(sessionRequest);
            var session = Json.Deserialize<FreeboxApiResponse<FreeboxSessionResponse>>(sessionResponse.Content);
            return session?.Result?.SessionToken
                ?? throw new Exception("Failed to create Freebox session token");
        }

        private static string ComputeHmacSha1(string key, string message)
        {
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private string BuildBaseUrl(FreeboxSettings settings)
        {
            var scheme = settings.UseSsl ? "https" : "http";
            return $"{scheme}://{settings.Host}:{settings.Port}";
        }

        private HttpRequestBuilder BuildRequest(FreeboxSettings settings, string resource, string sessionToken = null)
        {
            var builder = new HttpRequestBuilder(BuildBaseUrl(settings))
                .Resource(resource)
                .Accept(HttpAccept.Json);

            if (sessionToken != null)
            {
                builder = builder.SetHeader("X-Fbx-App-Auth", sessionToken);
            }

            return builder;
        }
    }
}
