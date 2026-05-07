using NzbDrone.Core.Configuration;
using Readarr.Http;

namespace Readarr.Api.V1.Config
{
    [V1ApiController("config/importlist")]
    public class ImportListConfigController : ConfigController<ImportListConfigResource>
    {
        public ImportListConfigController(IConfigService configService)
            : base(configService)
        {
        }

        protected override ImportListConfigResource ToResource(IConfigService model)
        {
            return ImportListConfigResourceMapper.ToResource(model);
        }
    }
}
