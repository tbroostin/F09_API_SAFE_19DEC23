// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Microsoft.Practices.Unity;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Dependency
{
    /// <summary>
    /// Registers Application Layer Services and Data Transfer Adapters. The default load order is 200, which allows other items to be injected earlier.
    /// </summary>
    public class CoordinationRegistration : ICoordinationRegistration
    {
        /// <summary>
        /// Registers items within the unity container.
        /// </summary>
        /// <param name="container">unity container</param>
        public void Register(IUnityContainer container)
        {
            RegisterAdapters(container);
            RegisterServices(container);
        }

        private void RegisterServices(IUnityContainer container)
        {
            var assembly = GetType().Assembly;

            DependencyRegistration.RegisterTypes(assembly, container);
        }

        private void RegisterAdapters(IUnityContainer container)
        {
            var baseAdapterInterface = typeof(ITypeAdapter);

            // Select adapter types from this assembly that inherits from ITypeAdapter; the adapter interfaces are excluded
            // since concrete types are required
            var adapterTypes = GetType().Assembly.GetTypes().Where(x => baseAdapterInterface.IsAssignableFrom(x) && !x.IsInterface);

            ISet<ITypeAdapter> adapterCollection = new HashSet<ITypeAdapter>();

            ILogger logger = container.Resolve<ILogger>();

            AdapterRegistry registry = new AdapterRegistry(adapterCollection, logger);

            foreach (var adapterType in adapterTypes)
            {
                // Instantiate 
                var adapterObject = adapterType.GetConstructor(new Type[] { typeof(IAdapterRegistry), typeof(ILogger) }).Invoke(new object[] { registry, logger }) as ITypeAdapter;

                registry.AddAdapter(adapterObject);
            }

            // Register the adapter registry as a singleton instance
            container.RegisterInstance<IAdapterRegistry>(registry, new ContainerControlledLifetimeManager());
        }

        /// <summary>
        /// Determines the load order of the items registered.
        /// </summary>
        /// <returns>
        /// load order integer
        /// </returns>
        public int GetLoadOrder()
        {
            return 200;
        }
    }
}
