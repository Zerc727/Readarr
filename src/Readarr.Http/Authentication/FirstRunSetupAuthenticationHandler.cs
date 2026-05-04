using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Readarr.Http.Authentication
{
    public class FirstRunSetupAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "FirstRunSetup";

        private readonly IConfigFileProvider _configFileProvider;
        private readonly IUserService _userService;

        public FirstRunSetupAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfigFileProvider configFileProvider,
            IUserService userService)
            : base(options, logger, encoder)
        {
            _configFileProvider = configFileProvider;
            _userService = userService;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Only bypass on PUT /api/v1/config/host when no auth method is configured
            // and no users exist yet — the strict three-way gate prevents misuse on
            // established instances.
            if (Request.Method != "PUT" ||
                !Request.Path.StartsWithSegments("/api/v1/config/host"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (_configFileProvider.AuthenticationMethod != AuthenticationType.None)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (_userService.FindUser() != null)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new Claim("user", "Setup"),
                new Claim("AuthType", SchemeName)
            };

            var identity = new ClaimsIdentity(claims, SchemeName, "user", "identifier");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
