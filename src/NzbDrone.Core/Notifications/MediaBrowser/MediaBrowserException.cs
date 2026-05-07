using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.MediaBrowser
{
    public class MediaBrowserException : NzbDroneException
    {
        public MediaBrowserException(string message, params object[] args)
            : base(message, args)
        {
        }

        public MediaBrowserException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
