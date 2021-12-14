// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Converters;
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Client;
using Ellucian.Web.Adapters;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Bootstrapping;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Resource;
using Ellucian.Web.Resource.Repositories;
using Ellucian.Web.Security;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Newtonsoft.Json;
using Serilog.Core;
using Serilog.Events;
using slf4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using Unity.Mvc3;

namespace Ellucian.Colleague.Api
{
    /// <summary>
    /// Manages the initial configuration of the web API layer, including container registration.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// When set true in appSettings, the logic to convert rules into .NET expression trees will be disabled.
        /// </summary>
        private static string ExecuteAllRulesInColleague = "ExecuteAllRulesInColleague";

        private static string LogFile = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "Logs", "ColleagueWebApi.log");
        private static string LogCategory = "ColleagueAPIApplication";
        private static string LogComponentName = "ColleagueWebAPI";
        private static string colleagueTimeZone = "";
        private static string baseResourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_GlobalResources");
        private static string resourceCustomizationFilePath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + @"\ResourceCustomization.json";

        private const string HedtechIntegrationStudentUnverifiedGradesSubmissionsFormat = "application/vnd.hedtech.integration.student-unverified-grades-submissions.v{0}+json";
        private const string HedtechIntegrationStudentTranscriptGradesAdjustmentsFormat = "application/vnd.hedtech.integration.student-transcript-grades-adjustments.v{0}+json";

        /// <summary>
        /// This property set/get an instance of the Serilog LoggingLevelSwitch
        /// </summary>
        public static LoggingLevelSwitch LoggingLevelSwitch { get; private set; }

        /// <summary>
        /// Initializes the dependency injection container.
        /// </summary>
        public static void Initialize()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            // Register dependency resolver for WebAPI
            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);

            // Use custom converter to handle deserialization of DateTimeOffset objects using Colleague time zone.
            SetCustomJsonDateTimeConverter();

            //Add the supported Data Model MediaTypes to the SupportedMediaTypes collection so the json 
            //deserializer can handle when the content-type header is set to one of these types.
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v1+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v2+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v3+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v4+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v5+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v6+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v7+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v8+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v9+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v10+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v11+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v12+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.v13+json"));

            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue(string.Format(HedtechIntegrationStudentUnverifiedGradesSubmissionsFormat, "1.0.0")));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue(string.Format(HedtechIntegrationStudentTranscriptGradesAdjustmentsFormat, "1.0.0")));

            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.bulk-requests.v1.0.0+json"));
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.hedtech.integration.bulk-requests.v1+json"));


        }

        private static IUnityContainer BuildUnityContainer()
        {
            /*
             * NOTE: Order can be important when setting up the container, so be careful!
             */

            // [0] create container and configure items specified in web.config unity configuration section...
            var container = new UnityContainer();
            // Read the Unity configuration
            UnityConfigurationSection section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            section.Configure(container);

            // [1] setup connection settings
            container.RegisterType<ISettingsRepository, XmlSettingsRepository>();
            var settings = container.Resolve<ISettingsRepository>().Get();
            var collSettings = settings.ColleagueSettings;
            var dmiSettings = settings.ColleagueSettings.DmiSettings;
            container.RegisterInstance<ColleagueSettings>(collSettings);
            container.RegisterInstance<DmiSettings>(dmiSettings);
            Task.Run(async () => await DmiConnectionPool.SetSizeAsync(
                DmiConnectionPool.ConnectionPoolName(collSettings.DmiSettings.IpAddress, collSettings.DmiSettings.Port, collSettings.DmiSettings.Secure),
                collSettings.DmiSettings.ConnectionPoolSize)).GetAwaiter().GetResult();
            Task.Run(async () => await Ellucian.Dmi.Client.Das.DasSessionPool.SetSizeAsync(collSettings.DasSettings.ConnectionPoolSize)).GetAwaiter().GetResult();

            // [2] setup logging (depends on settings). Override default log template with one that has timestamp.
            LogEventLevel logEventLevel = settings.LogLevel;
            LoggingLevelSwitch = new LoggingLevelSwitch(logEventLevel);
            Ellucian.Logging.SerilogAdapter.Configure(LogFile, LoggingLevelSwitch, LogComponentName);
            ILogger logger = slf4net.LoggerFactory.GetLogger(LogCategory);
            container.RegisterInstance<ILogger>(logger);

            // [3] critical common components (depend on logging and settings)
            container.RegisterType<ISessionRepository, ColleagueSessionRepository>();
            container.RegisterType<IRoleRepository, RoleRepository>();
            container.RegisterType<IUserRepository, UserRepository>();
            container.RegisterType<IColleagueTransactionFactory, HttpContextTransactionFactory>();
            container.RegisterType<ICurrentUserFactory, ThreadCurrentUserFactory>();

            // [4] setup api settings (depends on settings, logging, and common components)
            container.RegisterType<IApiSettingsRepository, ApiSettingsRepository>();

            // [5] required repository for extendedRouteContraint used for extensibility
            container.RegisterType<IExtendRepository, ExtendRepository>();

            var apiSettings = new ApiSettings("null");
            try
            {
                apiSettings = container.Resolve<IApiSettingsRepository>().Get(settings.ProfileName);
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(e.Message))
                {
                    string m = e.Message.ToLower();
                    if (m.Contains("cannot access file") && m.Contains("web.api.config"))
                    {
                        logger.Error("WEB.API.CONFIG has not been configured for anonymous access on WSPD! Anything using the API settings may fail.");
                    }
                }
                logger.Error(e, "Unable to read API Settings from colleague. Profile Name: {0}", settings.ProfileName);
            }

            container.RegisterInstance<ApiSettings>(apiSettings);
            colleagueTimeZone = apiSettings.ColleagueTimeZone;

            // [5] Resource Repository 
            var localResourceRepository = new LocalResourceRepository(baseResourcePath, resourceCustomizationFilePath);
            container.RegisterInstance<IResourceRepository>(localResourceRepository);

            // Web API Cache Initialization
            //   By default, at this point, the HTTP runtime cache is already instantiated per Web.config (completed in the section.Configure(container)
            //   call earlier in this method)
            ICacheProvider apiCacheProvider = container.Resolve<ICacheProvider>();
            if (apiCacheProvider != null)
            {
                logger.Info("Cache initialized; using " + apiCacheProvider.GetType().ToString());
            }

            // rules "engine"
            container.RegisterInstance<RuleAdapterRegistry>(new RuleAdapterRegistry());
            var rulesFlag = ConfigurationManager.AppSettings[ExecuteAllRulesInColleague];
            var config = new RuleConfiguration();
            if (!string.IsNullOrEmpty(rulesFlag) && "TRUE".Equals(rulesFlag.ToUpper()))
            {
                config.ExecuteAllRulesInColleague = true;
            }
            container.RegisterInstance<RuleConfiguration>(config);

            // the following calls all depend on all assemblies being loaded...
            LoadAllAssembliesIntoAppDomain(container);

            RegisterTypes(container);
            RegisterAdapters(container);
            BootstrapModules(container);

            return container;
        }

        private static void LoadAllAssembliesIntoAppDomain(IUnityContainer container)
        {
            var binDirectory = System.Web.HttpRuntime.BinDirectory;
            var files = Directory.GetFiles(binDirectory, "*.dll", SearchOption.AllDirectories);
            AssemblyName a = null;
            foreach (var s in files)
            {
                a = AssemblyName.GetAssemblyName(s);
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(
                    assembly => AssemblyName.ReferenceMatchesDefinition(
                    assembly.GetName(), a)))
                {
                    Assembly.LoadFrom(s);
                }
            }

            // debug
            ILogger logger = container.Resolve<ILogger>();
            if (logger.IsDebugEnabled)
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                List<Assembly> sortedLoadedAssemblies = loadedAssemblies.OrderBy(x => x.FullName).ToList();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Loaded AppDomain Assemblies:");
                foreach (var asm in sortedLoadedAssemblies)
                {
                    sb.AppendLine(asm.FullName);
                }
                logger.Debug(sb.ToString());
            }
        }

        private static void RegisterTypes(IUnityContainer container)
        {
            List<Type> concreteTypeList = new List<Type>();
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Interfaces are excluded since concrete types are required
            try
            {
                var ellucianLoadedAssemblies = loadedAssemblies.Where(x => x.GetName().Name.StartsWith("ellucian", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var concreteTypes = ellucianLoadedAssemblies.SelectMany(assembly => assembly.GetTypes().Where(type => !type.IsInterface && type.GetCustomAttributes(typeof(RegisterTypeAttribute), true).FirstOrDefault() != null));
                concreteTypeList.AddRange(concreteTypes);
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    if (exSub is FileNotFoundException)
                    {
                        FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                throw new Exception(errorMessage);
            }

            foreach (var type in concreteTypeList)
            {
                var interfaces = type.GetInterfaces();
                Type implementedInterface = null;
                switch (interfaces.Count())
                {
                    case 1:
                        // If only implementing one interface, use it.
                        implementedInterface = interfaces.ElementAt(0);
                        break;
                    case 2:
                        // Assuming the type implements one other interface besides IDisposable
                        implementedInterface = interfaces.Where(x => x != typeof(IDisposable)).FirstOrDefault();
                        break;
                    default:
                        // If there are more than 2 implemented interfaces, try to match on the name (MyRepository probably implements IMyRepository)
                        // NOTE: This is far from ideal.  If MyRepositoryA implements IMyRepositoryA and MyRepositoryB implements IFooRepository and inherits MyRepositoryA,
                        // this will not pick the correct implemented interface.
                        // TODO: Look into interfacemap???
                        implementedInterface = interfaces.Where(x => x.Name.Contains(type.Name)).FirstOrDefault();

                        // If picking by name failed, take the first that isn't IDisposable
                        if (implementedInterface == null)
                        {
                            implementedInterface = interfaces.Where(x => x != typeof(IDisposable)).FirstOrDefault();
                        }
                        break;
                }

                var registerTypeAttribute = type.GetCustomAttributes(typeof(RegisterTypeAttribute), true).FirstOrDefault() as RegisterTypeAttribute;

                if (implementedInterface != null && registerTypeAttribute != null)
                {
                    if (registerTypeAttribute.Lifetime == RegistrationLifetime.Hierarchy)
                    {
                        container.RegisterType(implementedInterface, type, new HierarchicalLifetimeManager());
                    }
                    else
                    {
                        // Maps the interface to the concrete class
                        container.RegisterType(implementedInterface, type);
                    }
                }
            }
        }

        private static void RegisterAdapters(IUnityContainer container)
        {
            ILogger logger = container.Resolve<ILogger>();
            try
            {
                var baseAdapterInterface = typeof(ITypeAdapter);
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                // Select adapter types from this assembly that inherits from ITypeAdapter; the adapter interfaces are excluded
                // since concrete types are required
                var ellucianLoadedAssemblies = loadedAssemblies.Where(x => x.GetName().Name.StartsWith("ellucian", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var adapterTypes = ellucianLoadedAssemblies.SelectMany(assembly => assembly.GetTypes().Where(x => baseAdapterInterface.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && !x.ContainsGenericParameters));

                ISet<ITypeAdapter> adapterCollection = new HashSet<ITypeAdapter>();
                AdapterRegistry registry = new AdapterRegistry(adapterCollection, logger);

                StringBuilder debug = new StringBuilder();
                debug.AppendLine("RegisterAdapters:");

                foreach (var adapterType in adapterTypes)
                {
                    // Instantiate 
                    var adapterObject = adapterType.GetConstructor(new Type[] { typeof(IAdapterRegistry), typeof(ILogger) }).Invoke(new object[] { registry, logger }) as ITypeAdapter;
                    registry.AddAdapter(adapterObject);
                    debug.AppendLine("added: " + adapterObject.GetType().ToString());
                }

                logger.Debug(debug.ToString());

                // Register the adapter registry as a singleton instance
                container.RegisterInstance<IAdapterRegistry>(registry, new ContainerControlledLifetimeManager());
            }
            catch (ReflectionTypeLoadException e)
            {
                logger.Error("RegisterAdapters error(s)", e);
                if (e.LoaderExceptions != null)
                {
                    foreach (var le in e.LoaderExceptions)
                    {
                        logger.Error("Loader Exception: " + le.Message);
                    }
                }
                throw e;
            }
        }

        private static void BootstrapModules(IUnityContainer container)
        {
            ILogger logger = container.Resolve<ILogger>();
            try
            {
                var moduleBootstrapperInterface = typeof(IModuleBootstrapper);
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();


                var ellucianLoadedAssemblies = loadedAssemblies.Where(x => x.GetName().Name.StartsWith("ellucian", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                var interfaceImplementers = ellucianLoadedAssemblies.SelectMany(assembly => assembly.GetTypes().Where(x => moduleBootstrapperInterface.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && !x.ContainsGenericParameters));
                if (interfaceImplementers != null)
                {
                    StringBuilder debug = new StringBuilder();
                    debug.AppendLine("BootstrapModules:");

                    foreach (var module in interfaceImplementers)
                    {
                        try
                        {
                            var instance = Activator.CreateInstance(module) as Ellucian.Web.Http.Bootstrapping.IModuleBootstrapper;
                            instance.BootstrapModule(container);
                            instance = null;
                            debug.AppendLine("executed: " + module.Name);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Module bootstrapping failed");
                        }
                    }

                    logger.Debug(debug.ToString());
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                logger.Error("BootstrapModules error(s)", e);
                if (e.LoaderExceptions != null)
                {
                    foreach (var le in e.LoaderExceptions)
                    {
                        logger.Error("Loader Exception: " + le.Message);
                    }
                }
                throw e;
            }
        }

        /// <summary>
        /// Sets the custom json date time converter to override how json date/time strings
        /// are handled using the ColleagueDateTimeConverter.
        /// </summary>
        private static void SetCustomJsonDateTimeConverter()
        {
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            json.SerializerSettings.DateParseHandling = DateParseHandling.None;
            json.SerializerSettings.Converters.Add(new ColleagueDateTimeConverter(colleagueTimeZone));
        }
    }
}
