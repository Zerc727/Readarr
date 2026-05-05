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
    public class InitializeJsonController : Controller
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAnalyticsService _analyticsService;

        public InitializeJsonController(IConfigFileProvider configFileProvider,
                                        IAnalyticsService analyticsService)
        {
            _configFileProvider = configFileProvider;
            _analyticsService = analyticsService;
        }

        [HttpGet("/initialize.json")]
        public IActionResult Index()
        {
            return Content(GetContent(), "application/json");
        }

        private string GetContent()
        {
            var urlBase = _configFileProvider.UrlBase;
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"apiRoot\": {Js(urlBase + "/api/v1")},");
            sb.AppendLine($"  \"apiKey\": {Js(_configFileProvider.ApiKey)},");
            sb.AppendLine($"  \"release\": {Js(BuildInfo.Release)},");
            sb.AppendLine($"  \"version\": {Js(BuildInfo.Version.ToString())},");
            sb.AppendLine($"  \"instanceName\": {Js(_configFileProvider.InstanceName)},");
            sb.AppendLine($"  \"theme\": {Js(_configFileProvider.Theme)},");
            sb.AppendLine($"  \"branch\": {Js(_configFileProvider.Branch.ToLower())},");
            sb.AppendLine($"  \"analytics\": {_analyticsService.IsEnabled.ToString().ToLowerInvariant()},");
            sb.AppendLine($"  \"userHash\": {Js(HashUtil.AnonymousToken())},");
            sb.AppendLine($"  \"urlBase\": {Js(urlBase)},");
            sb.AppendLine($"  \"isProduction\": {RuntimeInfo.IsProduction.ToString().ToLowerInvariant()}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string Js(string value) => JsonSerializer.Serialize(value);
    }
}
