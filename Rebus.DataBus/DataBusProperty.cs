using System;
using System.Runtime.Serialization;

namespace Rebus.DataBus
{
    [Serializable]
    public class DataBusCompressedProperty<T> : DataBusProperty<T>, IDataBusCompressedProperty where T : class
    {
        public DataBusCompressedProperty(T value) : base(value)
        {
        }

        protected DataBusCompressedProperty(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class DataBusProperty<T> : IDataBusProperty, ISerializable where T : class
    {
        public DataBusProperty(T value)
        {
            SetValue(value);
        }

        protected DataBusProperty(SerializationInfo info, StreamingContext context)
        {
            ClaimKey = info.GetString("ClaimKey");
            HasValue = info.GetBoolean("HasValue");
            Checksum = info.GetString("Checksum");            
        }

        public T Value { get; private set; }
        public string ClaimKey { get; set; }
        public bool HasValue { get; set; }
        public string Checksum { get; set; }

     

        public void SetValue(object valueToSet)
        {
            Value = valueToSet as T;
        }

        public object GetValue()
        {
            return Value;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ClaimKey", ClaimKey);
            info.AddValue("HasValue", HasValue);
            info.AddValue("Checksum", Checksum);            
        }
    }
}