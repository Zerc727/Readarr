using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.AutoTagging.Events
{
    public class AutoTagDeletedEvent : IEvent
    {
        public AutoTag AutoTag { get; }

        public AutoTagDeletedEvent(AutoTag autoTag)
        {
            AutoTag = autoTag;
        }
    }
}
