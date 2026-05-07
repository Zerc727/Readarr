using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public class TraktException : NzbDroneException
    {
        public TraktException(string message)
            : base(message)
        {
        }

        public TraktException(string message, params object[] args)
            : base(message, args)
        {
        }

        public TraktException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
