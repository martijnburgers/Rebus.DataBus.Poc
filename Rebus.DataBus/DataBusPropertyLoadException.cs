using System;
using System.Runtime.Serialization;

namespace Rebus.DataBus
{
    public class DataBusPropertyLoadException : DataBusException
    {
        public DataBusPropertyLoadException()
        {
        }

        public DataBusPropertyLoadException(string message) : base(message)
        {
        }

        public DataBusPropertyLoadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DataBusPropertyLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
