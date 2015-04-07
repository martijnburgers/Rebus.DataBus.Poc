using System;

namespace Rebus.DataBus.Configuration
{
    public class DataBusResolverContext
    {
        //todo access to the service locator object?

        private readonly DataBusConfigurer _configurer;

        public DataBusResolverContext(DataBusConfigurer configurer)
        {
            if (configurer == null) throw new ArgumentNullException("configurer");
            _configurer = configurer;
        }

        public IDataBusSerializer ResolveDataBusSerializer()
        {
            return _configurer.DataBusSerializer;
        }

        public IDataBusPropertyLoader ResolveDataBusPropertyLoader()
        {
            return _configurer.DataBusPropertyLoader;
        }

        public IDataBusPropertyOffloader ResolveDataBusPropertyOffloader()
        {
            return _configurer.DataBusPropertyOffloader;
        }

        public IDataBus ResolveDataBus()
        {
            return _configurer.DataBus;                    
        }

        public IDataBusSettings ResolveDataBusSettings()
        {
            return _configurer.DataBusSettings;
        }
    }
}