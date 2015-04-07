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
    public interface IDataBusPropertyLoader
    {
        void Load(object message);
        void Load(DataBusPropertyInfo propertyInfo, object message);
    }

    public class DataBusPropertyLoader : IDataBusPropertyLoader
    {
        private static ILog _log;
        private readonly IDataBus _dataBus;
        private readonly IDataBusSerializer _dataBusSerializer;
        private readonly IDataBusSettings _dataBusSettings;

        static DataBusPropertyLoader()
        {
            RebusLoggerFactory.Changed += f => _log = f.GetCurrentClassLogger();
        }

        public DataBusPropertyLoader(
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

        public void Load(object message)
        {
            _log.Info("Loading databus properties on message of type '{0}'.", message.GetType().FullName);

            IEnumerable<DataBusPropertyInfo> dataBusProperties = DataBusPropertiesExtractor.GetDataBusProperties(
                message);

            foreach (DataBusPropertyInfo dataBusPropertyInfo in dataBusProperties)
            {
                //todo make parallel?
                Load(dataBusPropertyInfo, message);
            }
        }

        public void Load(DataBusPropertyInfo propertyInfo, object message)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            if (message == null) throw new ArgumentNullException("message");

            _log.Info(
                "Loading databus property '{0}' on message of type '{1}'.",
                propertyInfo.Name,
                message.GetType().FullName);

            IDataBusProperty propertyInstance = propertyInfo.GetPropertyInstance(message) as IDataBusProperty;

            if (propertyInstance == null || !propertyInstance.HasValue)
            {
                _log.Warn(
                    String.Format(
                        "Databus property '{0}' on message of type '{1}' is null or doesn't have a value.",
                        propertyInfo.Name,
                        message.GetType().FullName));

                return;
            }
            
            LoadValueFromDataBus(propertyInstance, propertyInfo);
        }

        private void LoadValueFromDataBus(IDataBusProperty propertyInstance, DataBusPropertyInfo propertyInfo)
        {
            if (String.IsNullOrWhiteSpace(propertyInstance.ClaimKey))
            {
                string errorMsg = String.Format("Claim key of databus property '{0}' is missing. We can't fetch anything from the databus if the claimkey is null or whitespace.", propertyInfo.Name);

                _log.Error(errorMsg);

                throw new DataBusPropertyLoadException(errorMsg);
            }

            using (new TransactionScope(_dataBusSettings.TransactionScope))
            {
                //todo remove file from databus?
                using (Stream stream = _dataBus.Get(propertyInstance.ClaimKey, propertyInfo.IsCompressedProperty))
                {
                    if (_dataBusSettings.IsChecksummingEnabled)
                        DoChecksumming(propertyInstance, propertyInfo, stream);

                    stream.Position = 0;
                    object obj = _dataBusSerializer.Deserialize(stream);

                    if (obj == null)
                        _log.Warn("Deserializing databus stream resulted in a null object.");

                    propertyInstance.SetValue(obj);
                }
            }
        }

        private void DoChecksumming(IDataBusProperty propertyInstance, DataBusPropertyInfo propertyInfo, Stream stream)
        {
            if (propertyInstance.Checksum == null)
            {
                _log.Warn(
                    "Can't perform checksum validation if the checksum of the databus property '{0}' is not provided by the sender.", propertyInfo.Name);
            }
            else
            {
                stream.Position = 0;

                string actualChecksum = Checksum.GetSha256HashBuffered(stream);

                _log.Info("SHA256 checksum of databus property '{0}' is '{1}'.", propertyInfo.Name, actualChecksum);

                if (actualChecksum != propertyInstance.Checksum)
                {
                    string errorMsg = String.Format(
                        "Databus property '{0}' checksum error. Expected checksum value '{1}' but actual checksum value is '{2}'.",
                        propertyInfo.Name,
                        propertyInstance.Checksum,
                        actualChecksum);

                    _log.Error(errorMsg);

                    throw new DataBusPropertyLoadException(errorMsg);
                }
            }
        }
    }
}