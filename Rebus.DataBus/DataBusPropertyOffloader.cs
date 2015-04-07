using System;
using System.Collections.Generic;
using System.IO;
using System.Transactions;
using Rebus.DataBus.Configuration;
using Rebus.DataBus.Util;
using Rebus.DataBus.Util.Reflection;
using Rebus.Logging;

namespace Rebus.DataBus
{
    public interface IDataBusPropertyOffloader
    {
        void Offload(object message);
        void Offload(DataBusPropertyInfo propertyInfo, object message);        
    }
    
    public class DataBusPropertyOffloader : IDataBusPropertyOffloader
    {
        private readonly IDataBus _dataBus;
        private readonly IDataBusSerializer _dataBusSerializer;
        private readonly IDataBusSettings _dataBusSettings;

        private static ILog _log;

        public DataBusPropertyOffloader(
            IDataBus dataBus,
            IDataBusSerializer dataBusSerializer,
            IDataBusSettings dataBusSettings)
        {
            if (dataBus == null) throw new ArgumentNullException("dataBus");
            if (dataBusSerializer == null) throw new ArgumentNullException("dataBusSerializer");
            if (dataBusSettings == null) throw new ArgumentNullException("dataBusSettings");
            _dataBus = dataBus;
            _dataBusSerializer = dataBusSerializer;
            _dataBusSettings = dataBusSettings;
        }
        
        static DataBusPropertyOffloader()
        {
            RebusLoggerFactory.Changed += f => _log = f.GetCurrentClassLogger();
        }

        public void Offload(object message)
        {
            if (message == null) throw new ArgumentNullException("message");

            _log.Info("Offloading databus properties on message of type {0}", message.GetType().FullName);

            IEnumerable<DataBusPropertyInfo> props = DataBusPropertiesExtractor.GetDataBusProperties(message);

            foreach (DataBusPropertyInfo dataBusPropertyInfo in props)
            {
                Offload(dataBusPropertyInfo, message); //todo parallel things?
            }
        }


        public void Offload(DataBusPropertyInfo propertyInfo, object message)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            if (message == null) throw new ArgumentNullException("message");

            _log.Info(
              "Offloading databus property '{0}' on message of type {1}.",
              propertyInfo.Name,
              message.GetType().FullName);

            IDataBusProperty propertyInstance = propertyInfo.GetPropertyInstance(message) as IDataBusProperty;

            if (propertyInstance == null)
            {
                _log.Warn(
                    String.Format(
                        "Databus property '{0}' on message of type '{1}'. is null.",
                        propertyInfo.Name,
                        message.GetType().FullName));

                return;
            }

            object valueToPutOnBus = propertyInstance.GetValue();

            if (valueToPutOnBus != null)
            {
                PutValueOnDataBus(propertyInstance, valueToPutOnBus, propertyInfo);
            }
            else
            {
                _log.Warn("Value of databus property '{0} ' is null.", propertyInfo.Name);
            }
        }

        private void PutValueOnDataBus(IDataBusProperty propertyInstance, object valueToPutOnBus, DataBusPropertyInfo dataBusPropertyInfo)
        {
            propertyInstance.HasValue = true;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                _dataBusSerializer.Serialize(valueToPutOnBus, memoryStream);

                if (_dataBusSettings.IsChecksummingEnabled)
                {
                    memoryStream.Position = 0;
                    propertyInstance.Checksum = Checksum.GetSha256HashBuffered(memoryStream);

                    _log.Info("SHA256 checksum of databus property '{0}' is '{1}'.", dataBusPropertyInfo.Name, propertyInstance.Checksum);
                }

                string claimKey;
                                
                using (new TransactionScope(_dataBusSettings.TransactionScope))
                {
                    memoryStream.Position = 0;
                    claimKey = _dataBus.Put(memoryStream, dataBusPropertyInfo.IsCompressedProperty);
                }

                if (String.IsNullOrWhiteSpace(claimKey))
                {
                    string errorMsg = String.Format("The claimkey for databus property '{0}' returned by the databus is null or whitespace! You can't claim anything without a claim key.", dataBusPropertyInfo.Name);
                    
                    _log.Error(errorMsg);
                    
                    throw new DataBusPropertyOffoadException(errorMsg);
                }

                propertyInstance.ClaimKey = claimKey;
            }
        }        
    }
}