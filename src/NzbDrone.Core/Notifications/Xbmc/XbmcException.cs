using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class XbmcException : NzbDroneException
    {
        public XbmcException(string message, params object[] args)
            : base(message, args)
        {
        }

        public XbmcException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
