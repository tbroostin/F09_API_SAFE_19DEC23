// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    public class IntegrationConfigurationEntityAdapter
    {
        private IAdapterRegistry AdapterRegistry;
        private ILogger Logger;

        public IntegrationConfigurationEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            Logger = logger;
        }

        public Dtos.Base.IntegrationConfiguration MapToType(Ellucian.Colleague.Domain.Base.Entities.IntegrationConfiguration source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "Must provide source to convert");
            }

            var config = new Dtos.Base.IntegrationConfiguration();

            config.Id = source.Id;
            config.AmqpServerConfiguration = new Dtos.Base.AmqpServerConfiguration()
            {
                BaseUrl = source.AmqpMessageServerBaseUrl,
                PortNumber = source.AmqpMessageServerPortNumber,
                VirtualHost = source.AmqpMessageServerVirtualHost,
                ConnectionTimeout = source.AmqpMessageServerConnectionTimeout,
                Heartbeat = source.AmqpMessageServerHeartbeat,
                AutomaticallyRecoverMessages = source.AutomaticallyRecoverAmqpMessages,
                ConnectionIsSecure = source.AmqpConnectionIsSecure,
                //Username = source.AmqpUsername,
                //Password = source.AmqpPassword
            };
            config.BusinessEventConfiguration = new Dtos.Base.ExchangeConfiguration(source.BusinessEventExchangeName)
            {
                QueueName = source.BusinessEventQueueName,
                RoutingKeys = source.BusinessEventRoutingKeys
            };
            config.OutboundConfiguration = new Dtos.Base.BaseExchangeConfiguration(source.OutboundExchangeName);
            config.InboundConfiguration = new Dtos.Base.ExchangeConfiguration(source.InboundExchangeName)
            {
                QueueName = source.InboundExchangeQueueName,
                RoutingKeys = source.InboundExchangeRoutingKeys
            };
            config.ApiConfiguration = new Dtos.Base.ErpApiConfiguration()
            {
                ErpName = source.ApiErpName,
                ApiUsername = source.ApiUsername,
                //ApiPassword = source.ApiPassword
            };
            if (source.ResourceBusinessEventMappings != null && source.ResourceBusinessEventMappings.Count > 0)
            {
                foreach (var mapping in source.ResourceBusinessEventMappings)
                {
                    config.ApiConfiguration.ResourceMappings.Add(new Dtos.Base.ResourceBusinessEventMapping()
                    {
                        ResourceName = mapping.ResourceName,
                        ResourceVersion = mapping.ResourceVersion,
                        PathSegment = mapping.PathSegment,
                        BusinessEvents = new List<string>() { mapping.BusinessEvent }
                    });
                }
            }
            config.GuidLifespan = source.GuidLifespan;
            config.DebugLevel = ConvertDebugLevelEnumValueToString(source.DebugLevel);
            
            config.UseIntegrationHub = source.UseIntegrationHub;
            config.IntegrationHubConfiguration = new Dtos.Base.IntegrationHubConfiguration
            {
               //ApiKey = source.ApiKey,
               TokenUrl = source.TokenUrl,
               PublishUrl = source.PublishUrl,
               SubscribeUrl = source.SubscribeUrl,
               ErrorUrl = source.ErrorUrl,
               HubMediaType = source.HubMediaType
            };
            return config;
        }

        /// <summary>
        /// Converts a Debug Level string to a corresponding AdapterDebugLevel enumeration value
        /// </summary>
        /// <param name="debugLevel">Debug Level</param>
        /// <returns>AdapterDebugLevel enumeration value</returns>
        private string ConvertDebugLevelEnumValueToString(AdapterDebugLevel debugLevel)
        {
            string level;
            switch (debugLevel)
            {
                case AdapterDebugLevel.Fatal:
                    level = "fatal";
                    break;
                case AdapterDebugLevel.Error:
                default:
                    level = "error";
                    break;
                case AdapterDebugLevel.Warning:
                    level = "warn";
                    break;
                case AdapterDebugLevel.Debug:
                    level = "debug";
                    break;
                case AdapterDebugLevel.Trace:
                    level = "trace";
                    break;
                case AdapterDebugLevel.Information:
                    level = "info";
                    break;
            }
            return level;
        }
    }
}
