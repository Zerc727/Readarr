using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Config
{
    public class ImportListConfigResource : RestResource
    {
        public ImportListSyncLevelType ListSyncLevel { get; set; }
    }

    public static class ImportListConfigResourceMapper
    {
        public static ImportListConfigResource ToResource(IConfigService model)
        {
            return new ImportListConfigResource
            {
                ListSyncLevel = model.ListSyncLevel,
            };
        }
    }
}
