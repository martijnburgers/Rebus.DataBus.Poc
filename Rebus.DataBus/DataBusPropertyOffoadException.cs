using System;
using System.Runtime.Serialization;

namespace Rebus.DataBus
{
    public class DataBusPropertyOffoadException : DataBusException
    {
        public DataBusPropertyOffoadException()
        {
        }

        public DataBusPropertyOffoadException(string message)
            : base(message)
        {
        }

        public DataBusPropertyOffoadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DataBusPropertyOffoadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}