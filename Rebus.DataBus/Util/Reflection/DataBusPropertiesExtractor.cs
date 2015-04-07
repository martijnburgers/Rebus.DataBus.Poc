using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rebus.DataBus.Util.Reflection
{
    public static class DataBusPropertiesExtractor
    {
        private static readonly ConcurrentDictionary<Type, List<DataBusPropertyInfo>> Cache =
            new ConcurrentDictionary<Type, List<DataBusPropertyInfo>>();

        public static IEnumerable<DataBusPropertyInfo> GetDataBusProperties(object message)
        {
            Type messageType = message.GetType();
            List<DataBusPropertyInfo> list;

            if (!Cache.TryGetValue(messageType, out list))
            {
                var listOfDataBusProperties = messageType.GetProperties()
                                                         .Where(PropertyInfoExtensions.IsDataBusProperty)
                                                         .Select(PropertyInfoExtensions.PromoteToDataBusPropertyInfo)
                                                         .ToList();

                Cache[messageType] = (list = listOfDataBusProperties);
            }

            return list.AsEnumerable();
        }
    }
}
