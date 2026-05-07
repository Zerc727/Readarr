using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.AutoTagging
{
    public interface IAutoTagRepository : IBasicRepository<AutoTag>
    {
    }

    public class AutoTagRepository : BasicRepository<AutoTag>, IAutoTagRepository
    {
        public AutoTagRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
