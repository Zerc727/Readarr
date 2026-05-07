using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.AutoTagging.Events
{
    public class AutoTagUpdatedEvent : IEvent
    {
        public AutoTag AutoTag { get; }

        public AutoTagUpdatedEvent(AutoTag autoTag)
        {
            AutoTag = autoTag;
        }
    }
}
