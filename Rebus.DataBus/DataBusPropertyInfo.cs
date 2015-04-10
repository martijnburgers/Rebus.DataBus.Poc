using System;

namespace Rebus.DataBus
{
    public class DataBusPropertyInfo
    {  
        public DataBusPropertyInfo(string name, bool isCompressedProperty, Func<object, object> getPropertyInstance)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (getPropertyInstance == null) throw new ArgumentNullException("getPropertyInstance");
            
            Name = name;
            IsCompressedProperty = isCompressedProperty;
            GetPropertyInstance = getPropertyInstance;            
        }

        public string Name { get; private set; }
        public bool IsCompressedProperty { get; set; }
        public Func<object, object> GetPropertyInstance { get; private set; }        
    }
}
