using System;
using System.Runtime.Serialization;

namespace Rebus.DataBus.Configuration
{
    public class DataBusConfigurationException : DataBusException
    {
        public DataBusConfigurationException()
        {
        }

        public DataBusConfigurationException(string message)
            : base(message)
        {
        }

        public DataBusConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DataBusConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}