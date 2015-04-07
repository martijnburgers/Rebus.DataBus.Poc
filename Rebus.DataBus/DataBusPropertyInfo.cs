using System;

namespace Rebus.DataBus
{
    public class DataBusPropertyInfo
    {  
        public DataBusPropertyInfo(string name, bool isCompressedProperty, Func<object, object> getPropertyInstance, Action<object, object> setPropertyInstance)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (getPropertyInstance == null) throw new ArgumentNullException("getPropertyInstance");
            if (setPropertyInstance == null) throw new ArgumentNullException("setPropertyInstance");
            Name = name;
            IsCompressedProperty = isCompressedProperty;
            GetPropertyInstance = getPropertyInstance;
            SetPropertyInstance = setPropertyInstance;
        }

        public string Name { get; private set; }
        public bool IsCompressedProperty { get; set; }
        public Func<object, object> GetPropertyInstance { get; private set; }
        public Action<object, object> SetPropertyInstance { get; private set; }        
    }
}
