using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Configuration;

namespace Readarr.Http.Frontend
{
    [Authorize(Policy = "UI")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class InitializeJsController : Controller
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAnalyticsService _analyticsService;

        public InitializeJsController(IConfigFileProvider configFileProvider,
                                      IAnalyticsService analyticsService)
        {
            _configFileProvider = configFileProvider;
            _analyticsService = analyticsService;
        }

        [HttpGet("/initialize.js")]
        public IActionResult Index()
        {
            return Content(GetContent(), "application/javascript");
        }

        private string GetContent()
        {
            var urlBase = _configFileProvider.UrlBase;
            var sb = new StringBuilder();
            sb.Append("window.Readarr = {");
            sb.Append($"\"urlBase\":{Js(urlBase)},");
            sb.Append($"\"apiRoot\":{Js(urlBase + "/api/v1")},");
            sb.Append($"\"apiKey\":{Js(_configFileProvider.ApiKey)},");
            sb.Append($"\"release\":{Js(BuildInfo.Release)},");
            sb.Append($"\"version\":{Js(BuildInfo.Version.ToString())},");
            sb.Append($"\"instanceName\":{Js(_configFileProvider.InstanceName)},");
            sb.Append($"\"theme\":{Js(_configFileProvider.Theme)},");
            sb.Append($"\"branch\":{Js(_configFileProvider.Branch.ToLower())},");
            sb.Append($"\"analytics\":{(_analyticsService.IsEnabled ? "true" : "false")},");
            sb.Append($"\"userHash\":{Js(HashUtil.AnonymousToken())},");
            sb.Append($"\"isProduction\":{(RuntimeInfo.IsProduction ? "true" : "false")}");
            sb.Append("};");

            return sb.ToString();
        }

        private static string Js(string value) => JsonSerializer.Serialize(value);
    }
}
