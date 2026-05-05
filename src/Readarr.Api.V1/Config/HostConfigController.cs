using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Update;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.Http.REST.Attributes;
using Readarr.Http;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Config
{
    [V1ApiController("config/host")]
    public class HostConfigController : RestController<HostConfigResource>
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        private readonly IUserService _userService;

        public HostConfigController(IConfigFileProvider configFileProvider,
                                    IConfigService configService,
                                    IUserService userService,
                                    FileExistsValidator fileExistsValidator)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _userService = userService;

            SharedValidator.RuleFor(c => c.BindAddress)
                           .ValidIpAddress()
                           .When(c => c.BindAddress != "*" && c.BindAddress != "localhost");

            SharedValidator.RuleFor(c => c.Port).ValidPort();

            SharedValidator.RuleFor(c => c.UrlBase).ValidUrlBase();
            SharedValidator.RuleFor(c => c.InstanceName).ContainsReadarr().When(c => c.InstanceName.IsNotNullOrWhiteSpace());

            SharedValidator.RuleFor(c => c.Username).NotEmpty().When(c => c.AuthenticationMethod == AuthenticationType.Basic ||
                                                                          c.AuthenticationMethod == AuthenticationType.Forms);

            // Password is required only on first-run (no user exists yet).  When a user
            // already exists, an empty password means "keep the existing password".
            SharedValidator.RuleFor(c => c.Password).NotEmpty().When(c =>
                (c.AuthenticationMethod == AuthenticationType.Basic ||
                 c.AuthenticationMethod == AuthenticationType.Forms)
                && _userService.FindUser() == null);

            SharedValidator.RuleFor(c => c.PasswordConfirmation)
                .Must((resource, p) => IsMatchingPassword(resource)).WithMessage("Must match Password");

            SharedValidator.RuleFor(c => c.SslPort).ValidPort().When(c => c.EnableSsl);
            SharedValidator.RuleFor(c => c.SslPort).NotEqual(c => c.Port).When(c => c.EnableSsl);

            SharedValidator.RuleFor(c => c.SslCertPath)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .IsValidPath()
                .SetValidator(fileExistsValidator)
                .Must((resource, path) => IsValidSslCertificate(resource)).WithMessage("Invalid SSL certificate file or password")
                .When(c => c.EnableSsl);

            SharedValidator.RuleFor(c => c.Branch).NotEmpty().WithMessage("Branch name is required, 'master' is the default");
            SharedValidator.RuleFor(c => c.UpdateScriptPath).IsValidPath().When(c => c.UpdateMechanism == UpdateMechanism.Script);

            SharedValidator.RuleFor(c => c.BackupFolder).IsValidPath().When(c => Path.IsPathRooted(c.BackupFolder));
            SharedValidator.RuleFor(c => c.BackupInterval).InclusiveBetween(1, 7);
            SharedValidator.RuleFor(c => c.BackupRetention).InclusiveBetween(1, 90);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // When only auth fields are submitted (e.g. first-run setup modal), required
            // non-auth fields arrive as zero/empty.  Back-fill from the current config so
            // validators don't fire on unrelated fields and existing values aren't silently
            // overwritten with defaults.
            if (Request.Method == "PUT")
            {
                foreach (var value in context.ActionArguments.Values)
                {
                    if (value is HostConfigResource resource)
                    {
                        if (resource.Id == 0)
                        {
                            resource.Id = 1;
                        }

                        if (resource.Port == 0)
                        {
                            resource.Port = _configFileProvider.Port;
                        }

                        if (resource.SslPort == 0)
                        {
                            resource.SslPort = _configFileProvider.SslPort;
                        }

                        if (resource.BindAddress.IsNullOrWhiteSpace())
                        {
                            resource.BindAddress = _configFileProvider.BindAddress;
                        }

                        if (resource.Branch.IsNullOrWhiteSpace())
                        {
                            resource.Branch = _configFileProvider.Branch;
                        }

                        if (resource.UrlBase == null)
                        {
                            resource.UrlBase = _configFileProvider.UrlBase;
                        }

                        if (resource.SslCertPath == null)
                        {
                            resource.SslCertPath = _configFileProvider.SslCertPath;
                        }

                        if (resource.SslCertPassword == null)
                        {
                            resource.SslCertPassword = _configFileProvider.SslCertPassword;
                        }

                        if (resource.UpdateScriptPath == null)
                        {
                            resource.UpdateScriptPath = _configFileProvider.UpdateScriptPath;
                        }

                        if (resource.BackupInterval == 0)
                        {
                            resource.BackupInterval = _configService.BackupInterval;
                        }

                        if (resource.BackupRetention == 0)
                        {
                            resource.BackupRetention = _configService.BackupRetention;
                        }
                    }
                }
            }

            base.OnActionExecuting(context);
        }

        private bool IsValidSslCertificate(HostConfigResource resource)
        {
            X509Certificate2 cert;
            try
            {
                cert = X509CertificateLoader.LoadPkcs12FromFile(resource.SslCertPath, resource.SslCertPassword, X509KeyStorageFlags.DefaultKeySet);
            }
            catch
            {
                return false;
            }

            return cert != null;
        }

        private bool IsMatchingPassword(HostConfigResource resource)
        {
            // Empty password = user is keeping their existing password (valid when a user exists).
            if (resource.Password.IsNullOrWhiteSpace())
            {
                return _userService.FindUser() != null;
            }

            return resource.Password == resource.PasswordConfirmation;
        }

        protected override HostConfigResource GetResourceById(int id)
        {
            return GetHostConfig();
        }

        [HttpGet]
        public HostConfigResource GetHostConfig()
        {
            var resource = HostConfigResourceMapper.ToResource(_configFileProvider, _configService);
            resource.Id = 1;

            var user = _userService.FindUser();

            resource.Username = user?.Username ?? string.Empty;
            resource.Password = string.Empty;
            resource.PasswordConfirmation = string.Empty;

            return resource;
        }

        [RestPutById]
        public ActionResult<HostConfigResource> SaveHostConfig(HostConfigResource resource)
        {
            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configFileProvider.SaveConfigDictionary(dictionary);
            _configService.SaveConfigDictionary(dictionary);

            if (resource.Username.IsNotNullOrWhiteSpace() && resource.Password.IsNotNullOrWhiteSpace())
            {
                _userService.Upsert(resource.Username, resource.Password);
            }

            return Accepted(resource.Id);
        }
    }
}
