using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.AutoTagging.Events
{
    public class AutoTagAddedEvent : IEvent
    {
        public AutoTag AutoTag { get; }

        public AutoTagAddedEvent(AutoTag autoTag)
        {
            AutoTag = autoTag;
        }
    }
}
