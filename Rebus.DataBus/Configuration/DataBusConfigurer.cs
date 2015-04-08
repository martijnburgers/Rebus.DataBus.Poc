using System;
using System.Collections.Generic;
using System.Transactions;
using Microsoft.Practices.ServiceLocation;
using Rebus.Configuration;

namespace Rebus.DataBus.Configuration
{
    public class DataBusConfigurer : BaseConfigurer
    {
        private readonly Queue<Action<IDataBusSettings>> _settingsRecorderQueue = new Queue<Action<IDataBusSettings>>();
        private IDataBus _dataBus;
        private IDataBusPropertyLoader _dataBusPropertyLoader;
        private Func<DataBusResolverContext, IDataBusPropertyLoader> _dataBusPropertyLoaderResolver = c => null;
        private IDataBusPropertyOffloader _dataBusPropertyOffloader;
        private Func<DataBusResolverContext, IDataBusPropertyOffloader> _dataBusPropertyOffloaderResolver = c => null;
        private Func<DataBusResolverContext, IDataBus> _dataBusResolver = c => null;
        private IDataBusSerializer _dataBusSerializer;

        private Func<DataBusResolverContext, IDataBusSerializer> _dataBusSerializerResolver =
            c => new BinaryDataBusSerializer();

        private IDataBusSettings _dataBusSettings;
        private Func<DataBusResolverContext, IDataBusSettings> _dataBusSettingsResolver = c => new DataBusSettings();
        private IServiceLocator _serviceLocator;
        private bool _settingsApplied;

        /// <summary>
        ///     CTOR
        /// </summary>
        /// <param name="backbone">The backbone of Rebus</param>
        public DataBusConfigurer(ConfigurationBackbone backbone) : base(backbone)
        {
            TryConfigureServiceLocator();
            ConfigureEvents();
        }

        /// <summary>
        ///     CTOR
        /// </summary>
        /// <param name="backbone">The backbone of Rebus</param>
        /// <param name="dataBusResolver">Factory method for resolving the databus.</param>
        public DataBusConfigurer(ConfigurationBackbone backbone, Func<DataBusResolverContext, IDataBus> dataBusResolver)
            : base(backbone)
        {
            if (dataBusResolver == null) throw new ArgumentNullException("dataBusResolver");
            _dataBusResolver = dataBusResolver;

            TryConfigureServiceLocator();
            ConfigureEvents();
        }

        protected internal IDataBus DataBus
        {
            get
            {
                return
                    GuardForResolvingNulls(_dataBus ?? (_dataBus = _dataBusResolver(new DataBusResolverContext(this))));
            }
        }

        protected internal IDataBusSettings DataBusSettings
        {
            get
            {
                return
                    GuardForResolvingNulls(
                        _dataBusSettings ??
                        (_dataBusSettings = _dataBusSettingsResolver(new DataBusResolverContext(this))));
            }
        }

        protected internal IDataBusSerializer DataBusSerializer
        {
            get
            {
                return
                    GuardForResolvingNulls(
                        _dataBusSerializer ??
                        (_dataBusSerializer = _dataBusSerializerResolver(new DataBusResolverContext(this))));
            }
        }

        protected internal IDataBusPropertyOffloader DataBusPropertyOffloader
        {
            get
            {
                return
                    GuardForResolvingNulls(
                        _dataBusPropertyOffloader ??
                        (_dataBusPropertyOffloader = _dataBusPropertyOffloaderResolver(new DataBusResolverContext(this))));
            }
        }

        protected internal IDataBusPropertyLoader DataBusPropertyLoader
        {
            get
            {
                return
                    GuardForResolvingNulls(
                        _dataBusPropertyLoader ??
                        (_dataBusPropertyLoader = _dataBusPropertyLoaderResolver(new DataBusResolverContext(this))));
            }
        }

        private static T GuardForResolvingNulls<T>(T objectToCheck) where T : class
        {
            if (objectToCheck == null)
                throw new DataBusConfigurationException(
                    String.Format(
                        "Could not resolve a type implementing '{0}'. You are probably missing some configuration",
                        typeof (T).FullName));
            ;

            return objectToCheck;
        }

        private void TryConfigureServiceLocator()
        {
            //this is only way to get to the container adapter, needed resolving services.
            _serviceLocator = Backbone.ActivateHandlers as IServiceLocator;
        }

        private void ConfigureEvents()
        {
            Backbone.ConfigureEvents(
                e =>
                    e.MessageSent +=
                        (messageBus, destination, message) =>
                            OffloadDataBusProperties(DataBusPropertyOffloader, destination, message));

            Backbone.ConfigureEvents(
                e => e.BeforeMessage += (messageBus, message) => LoadDataBusProperties(DataBusPropertyLoader, message));

            Backbone.ConfigureEvents(e => e.BusStarted += messageBus => ApplyRecoredSettings());
        }

        private static void LoadDataBusProperties(IDataBusPropertyLoader dataBusPropertyLoader, object message)
        {
            dataBusPropertyLoader.Load(message);
        }

        private static void OffloadDataBusProperties(
            IDataBusPropertyOffloader dataBusPropertyOffloader,
            string destination,
            object message)
        {
            //todo different offloaders per destination?

            dataBusPropertyOffloader.Offload(message);
        }

        /// <summary>
        ///     If available use the service locator for resolving the types <see cref="IDataBus" />,
        ///     <see cref="IDataBusSerializer" />, <see cref="IDataBusPropertyLoader" />, <see cref="IDataBusPropertyOffloader" />
        /// </summary>
        protected internal DataBusConfigurer UseServiceLocator()
        {
            if (_serviceLocator == null)
            {
                throw new InvalidOperationException(
                    "No service locator known. Make sure your container adapter is a service locator, meaning it implements Microsoft.Practices.ServiceLocation.IServiceLocator");
            }

            _dataBusResolver = c => _serviceLocator.GetInstance<IDataBus>();
            _dataBusSettingsResolver = c => _serviceLocator.GetInstance<IDataBusSettings>();
            _dataBusSerializerResolver = c => _serviceLocator.GetInstance<IDataBusSerializer>();
            _dataBusPropertyOffloaderResolver = c => _serviceLocator.GetInstance<IDataBusPropertyOffloader>();
            _dataBusPropertyLoaderResolver = c => _serviceLocator.GetInstance<IDataBusPropertyLoader>();

            return this;
        }

        protected internal void UseDataBus(Func<DataBusResolverContext, IDataBus> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _dataBusResolver = factory;
        }

        protected internal void UseSerializer(Func<DataBusResolverContext, IDataBusSerializer> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _dataBusSerializerResolver = factory;
        }

        protected internal void UseDataBusPropertyOffloader(
            Func<DataBusResolverContext, IDataBusPropertyOffloader> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _dataBusPropertyOffloaderResolver = factory;
        }

        protected internal void UseDataBusPropertyLoader(Func<DataBusResolverContext, IDataBusPropertyLoader> factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _dataBusPropertyLoaderResolver = factory;
        }

        protected internal void EnableChecksums()
        {
            if (_settingsApplied)
                DataBusSettings.IsChecksummingEnabled = true;
            else
                _settingsRecorderQueue.Enqueue(settings => settings.IsChecksummingEnabled = true);
        }

        protected internal void DisableChecksums()
        {
            if (_settingsApplied)
                DataBusSettings.IsChecksummingEnabled = false;
            else
                _settingsRecorderQueue.Enqueue(settings => settings.IsChecksummingEnabled = false);
        }

        private void ApplyRecoredSettings()
        {
            while (_settingsRecorderQueue.Count != 0)
            {
                Action<IDataBusSettings> apply = _settingsRecorderQueue.Dequeue();

                apply(DataBusSettings);
            }

            if (_settingsRecorderQueue.Count != 0)
                ApplyRecoredSettings();

            _settingsApplied = true;
        }

        protected internal void SetTransactionScope(TransactionScopeOption transactionScope)
        {
            if (_settingsApplied)
                DataBusSettings.TransactionScope = transactionScope;
            else
                _settingsRecorderQueue.Enqueue(settings => settings.TransactionScope = transactionScope);
        }
    }
}