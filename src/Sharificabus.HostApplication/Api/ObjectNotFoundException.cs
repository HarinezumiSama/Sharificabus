using System;
using System.Runtime.Serialization;

namespace Sharificabus.HostApplication.Api
{
    [Serializable]
    internal sealed class ObjectNotFoundException : Exception
    {
        internal ObjectNotFoundException(string message)
            : base(message)
        {
        }

        private ObjectNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}