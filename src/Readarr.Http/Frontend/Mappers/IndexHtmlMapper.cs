using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Messaging.Events;

namespace Readarr.Http.Frontend.Mappers
{
    public class IndexHtmlMapper : HtmlMapperBase, IHandle<ConfigFileSavedEvent>
    {
        // Matches the entire window.Readarr = { ... }; block regardless of how many
        // fields the template contains — [^}]* stops at the first closing brace so
        // this works for single-property objects like { urlBase: '' } as well as any
        // future-expanded template.
        private static readonly Regex WindowReadarrRegex = new Regex(
            @"window\.Readarr\s*=\s*\{[^}]*\};",
            RegexOptions.Compiled);

        private readonly IConfigFileProvider _configFileProvider;
        private readonly IAnalyticsService _analyticsService;
        private string _generatedIndexContent;

        public IndexHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               IConfigFileProvider configFileProvider,
                               IAnalyticsService analyticsService,
                               Lazy<ICacheBreakerProvider> cacheBreakProviderFactory,
                               Logger logger)
            : base(diskProvider, cacheBreakProviderFactory, logger)
        {
            _configFileProvider = configFileProvider;
            _analyticsService = analyticsService;

            HtmlPath = Path.Combine(appFolderInfo.StartUpFolder, _configFileProvider.UiFolder, "index.html");
            UrlBase = configFileProvider.UrlBase;
        }

        public override string Map(string resourceUrl)
        {
            return HtmlPath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            resourceUrl = resourceUrl.ToLowerInvariant();

            return !resourceUrl.StartsWith("/content") &&
                   !resourceUrl.StartsWith("/mediacover") &&
                   !resourceUrl.Contains(".") &&
                   !resourceUrl.StartsWith("/login");
        }

        protected override Stream GetContentStream(string filePath)
        {
            if (RuntimeInfo.IsProduction && _generatedIndexContent != null)
            {
                return ToStream(_generatedIndexContent);
            }

            // GetHtmlText() is defined on HtmlMapperBase and handles its own
            // production cache for the __URL_BASE__ substitution pass.
            var text = GetHtmlText();
            text = WindowReadarrRegex.Replace(text, BuildWindowReadarrBlock());
            _generatedIndexContent = text;

            return ToStream(text);
        }

        public void Handle(ConfigFileSavedEvent message)
        {
            _generatedIndexContent = null;
        }

        private string BuildWindowReadarrBlock()
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

        private static Stream ToStream(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static string Js(string value) => JsonSerializer.Serialize(value);
    }
}
