using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Microsoft.Practices.ServiceLocation;
using Rebus.Autofac;

namespace Rebus.DataBus.Autofac
{
    public class AutofacServiceLocatorContainerAdapter : AutofacContainerAdapter, IServiceLocator
    {
        private readonly AutofacServiceLocator _csl;

        public AutofacServiceLocatorContainerAdapter(IContainer container)
            : base(container)
        {
            if (container == null) throw new ArgumentNullException("container");

            _csl = new AutofacServiceLocator(container);
        }

        public object GetService(Type serviceType)
        {
            return _csl.GetService(serviceType);
        }

        public object GetInstance(Type serviceType)
        {
            return _csl.GetInstance(serviceType);
        }

        public object GetInstance(Type serviceType, string key)
        {
            return _csl.GetInstance(serviceType, key);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _csl.GetAllInstances(serviceType);
        }

        public TService GetInstance<TService>()
        {
            return _csl.GetInstance<TService>();
        }

        public TService GetInstance<TService>(string key)
        {
            return _csl.GetInstance<TService>(key);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return _csl.GetAllInstances<TService>();
        }
    }
}
