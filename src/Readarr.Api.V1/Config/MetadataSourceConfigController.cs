using NzbDrone.Core.Configuration;
using Readarr.Http;

namespace Readarr.Api.V1.Config
{
    [V1ApiController("config/metadatasource")]
    public class MetadataSourceConfigController : ConfigController<MetadataSourceConfigResource>
    {
        public MetadataSourceConfigController(IConfigService configService)
            : base(configService)
        {
        }

        protected override MetadataSourceConfigResource ToResource(IConfigService model)
        {
            return MetadataSourceConfigResourceMapper.ToResource(model);
        }
    }
}
