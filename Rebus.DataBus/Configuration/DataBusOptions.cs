using System;
using System.Transactions;

namespace Rebus.DataBus.Configuration
{
    public class DataBusOptions
    {
        private readonly DataBusConfigurer _dataBusConfigurer;

        internal DataBusOptions(DataBusConfigurer dataBusConfigurer)
        {
            _dataBusConfigurer = dataBusConfigurer;
        }

        public DataBusOptions UseFileShare(string basePath)
        {
            if (basePath == null) throw new ArgumentNullException("basePath");

            _dataBusConfigurer.UseDataBus(c => new FileShareDataBus(basePath));

            return this;
        }

        public DataBusOptions UseSerializer(Func<DataBusResolverContext, IDataBusSerializer> factory)
        {
            _dataBusConfigurer.UseSerializer(factory);

            return this;
        }

        public DataBusOptions UseSerializer<T>() where T : IDataBusSerializer, new()
        {
            _dataBusConfigurer.UseSerializer(c => new T());

            return this;
        }

        public DataBusOptions UseDataBusPropertyOffloader(
            Func<DataBusResolverContext, IDataBusPropertyOffloader> factory)
        {
            _dataBusConfigurer.UseDataBusPropertyOffloader(factory);

            return this;
        }

        public DataBusOptions UseDataBusPropertyOffloader<T>() where T : IDataBusPropertyOffloader, new()
        {
            _dataBusConfigurer.UseDataBusPropertyOffloader(c => new T());

            return this;
        }

        public DataBusOptions UseDataBusPropertyLoader(Func<DataBusResolverContext, IDataBusPropertyLoader> factory)
        {
            _dataBusConfigurer.UseDataBusPropertyLoader(factory);

            return this;
        }

        public DataBusOptions UseDataBusPropertyLoader<T>() where T : IDataBusPropertyLoader, new()
        {
            _dataBusConfigurer.UseDataBusPropertyLoader(c => new T());

            return this;
        }

        public DataBusOptions EnableChecksums()
        {
            _dataBusConfigurer.EnableChecksums();

            return this;
        }

        /// <summary>
        ///     If avalaible use the service locator for resolving the types <see cref="IDataBus" />,
        ///     <see cref="IDataBusSerializer" />, <see cref="IDataBusPropertyLoader" />, <see cref="IDataBusPropertyOffloader" />
        /// </summary>
        public void UseServiceLocator()
        {
            _dataBusConfigurer.UseServiceLocator();
        }

        //todo Enable Compression

        public DataBusOptions SetTransactionScope(TransactionScopeOption transactionScope)
        {
            _dataBusConfigurer.SetTransactionScope(transactionScope);

            return this;
        }
    }
}