using System.Collections.Generic;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public interface ITraktProxy
    {
        List<TraktListItem> GetWatchlistItems(TraktSettings settings);
        List<TraktListItem> GetCustomListItems(string username, string listSlug, TraktSettings settings);
    }

    public class TraktProxy : ITraktProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        private const string TraktBaseUrl = "https://api.trakt.tv";

        public TraktProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public List<TraktListItem> GetWatchlistItems(TraktSettings settings)
        {
            var request = BuildRequest($"/users/{settings.Username}/watchlist", settings)
                .AddQueryParam("limit", settings.Limit)
                .Build();

            return Execute(request);
        }

        public List<TraktListItem> GetCustomListItems(string username, string listSlug, TraktSettings settings)
        {
            var request = BuildRequest($"/users/{username}/lists/{listSlug}/items", settings)
                .Build();

            return Execute(request);
        }

        private HttpRequestBuilder BuildRequest(string route, TraktSettings settings)
        {
            return new HttpRequestBuilder(TraktBaseUrl + route)
                .Accept(HttpAccept.Json)
                .SetHeader("trakt-api-version", "2")
                .SetHeader("trakt-api-key", settings.ClientId)
                .KeepAlive();
        }

        private List<TraktListItem> Execute(HttpRequest request)
        {
            _logger.Debug("Fetching from Trakt: {0}", request.Url);

            var response = _httpClient.Get(request);

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                return new List<TraktListItem>();
            }

            return JsonConvert.DeserializeObject<List<TraktListItem>>(response.Content) ?? new List<TraktListItem>();
        }
    }
}
