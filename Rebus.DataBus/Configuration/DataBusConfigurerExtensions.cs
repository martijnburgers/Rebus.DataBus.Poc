using System;
using Rebus.Configuration;

namespace Rebus.DataBus.Configuration
{
    public static class DataBusConfigurerExtensions
    {
        public static DataBusOptions UseDataBus(this RebusTransportConfigurer configurer)
        {
            if (configurer == null) throw new ArgumentNullException("configurer");
            if (configurer.Backbone == null) throw new InvalidOperationException("configurer must have a backbone");
            if (configurer.Backbone.SendMessages == null) throw new DataBusConfigurationException("Define the transport for the messagebus first");            

            

            var dataBusConfigurer = configurer.Backbone.LoadFromRegistry(() => new DataBusConfigurer(configurer.Backbone));

            return new DataBusOptions(dataBusConfigurer);
        }

        public static DataBusOptions UseDataBus(this RebusTransportConfigurer configurer, Func<DataBusResolverContext, IDataBus> dataBusResolver)
        {
            if (configurer == null) throw new ArgumentNullException("configurer");
            if (dataBusResolver == null) throw new ArgumentNullException("dataBusResolver");
            if (configurer.Backbone == null) throw new InvalidOperationException("configurer must have a backbone");
            if (configurer.Backbone.SendMessages == null) throw new DataBusConfigurationException("Define the transport for the messagebus first");

            var dataBusConfigurer = configurer.Backbone.LoadFromRegistry(() => new DataBusConfigurer(configurer.Backbone, dataBusResolver));

            return new DataBusOptions(dataBusConfigurer);
        } 
    }
}