// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class IntegrationConfigurationEntityAdapterTests
    {
        private Ellucian.Colleague.Domain.Base.Entities.IntegrationConfiguration integrationConfigurationEntity;
        private Ellucian.Colleague.Dtos.Base.IntegrationConfiguration integrationConfigurationDto;
        private IntegrationConfigurationEntityAdapter integrationConfigurationEntityAdapter;

        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ILogger> loggerMock;

        string id;
        string description;
        string amqpMessageServerBaseUrl;
        bool amqpConnectionIsSecure;
        int amqpMessageServerPortNumber;
        string amqpUsername;
        string amqpPassword;
        string businessEventExchangeName;
        string businessEventQueueName;
        string outboundExchangeName;
        string inboundExchangeName;
        string inboundExchangeQueueName;
        string apiUsername;
        string apiPassword;
        string apiErpName;
        AdapterDebugLevel debugLevel;
        int guidLifespan;
        bool useIntegrationHub;
        string apiKey;
        string tokenUrl;
        string publishUrl;
        string subscribeUrl;
        string errorUrl;
        string mediaType;
        List<Domain.Base.Entities.ResourceBusinessEventMapping> mappings;


        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            id = "CONFIG";
            description = "Integration Configuration";
            amqpMessageServerBaseUrl = "http://www.messageserver.com";
            amqpConnectionIsSecure = false;
            amqpMessageServerPortNumber = 5267;
            amqpUsername = "amqpAdmin";
            amqpPassword = "amqpPassword";
            businessEventExchangeName = "Event Exchange";
            businessEventQueueName = "Event Queue";
            outboundExchangeName = "Outbound Exchange";
            inboundExchangeName = "Inbound Exchange";
            inboundExchangeQueueName = "Inbound Queue";
            apiUsername = "apiAdmin";
            apiPassword = "apiPassword";
            apiErpName = "Colleague";
            debugLevel = AdapterDebugLevel.Information;
            guidLifespan = 10;
            useIntegrationHub = true;
            apiKey = "APIKEY1";
            tokenUrl = "http://www.toeknmessageserver.com";
            publishUrl = "http://www.publishmessageserver.com";
            subscribeUrl = "http://www.subscribemessageserver.com";
            errorUrl = "http://www.errormessageserver.com";
            mediaType = "application/vnd.hedtech.change-notifications.v2+json";

            mappings = new List<Domain.Base.Entities.ResourceBusinessEventMapping>();
            Domain.Base.Entities.ResourceBusinessEventMapping mapping1 = new Domain.Base.Entities.ResourceBusinessEventMapping("course", "1", "/courses/", "event1");
            Domain.Base.Entities.ResourceBusinessEventMapping mapping2 = new Domain.Base.Entities.ResourceBusinessEventMapping("section", "1", "/sections/", "event2");
            Domain.Base.Entities.ResourceBusinessEventMapping mapping3 = new Domain.Base.Entities.ResourceBusinessEventMapping("person", "1", "/persons/", "event3");
            mappings.Add(mapping1);
            mappings.Add(mapping2);

            integrationConfigurationEntity = new Domain.Base.Entities.IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, 
                    debugLevel, guidLifespan, mappings)
                    {
                        UseIntegrationHub = useIntegrationHub,
                        ApiKey = apiKey,
                        TokenUrl = tokenUrl,
                        PublishUrl = publishUrl,
                        SubscribeUrl = subscribeUrl,
                        ErrorUrl = errorUrl,
                        HubMediaType = mediaType
                    };

            integrationConfigurationEntityAdapter = new IntegrationConfigurationEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            integrationConfigurationDto = integrationConfigurationEntityAdapter.MapToType(integrationConfigurationEntity);
        }

        [TestMethod]
        public void IntegrationConfiguration_Id()
        {
            Assert.AreEqual(integrationConfigurationEntity.Id, integrationConfigurationDto.Id);
        }

        [TestMethod]
        public void IntegrationConfiguration_AmqpServerConfiguration()
        {
            Assert.AreEqual(integrationConfigurationEntity.AutomaticallyRecoverAmqpMessages, integrationConfigurationDto.AmqpServerConfiguration.AutomaticallyRecoverMessages);
            Assert.AreEqual(integrationConfigurationEntity.AmqpMessageServerBaseUrl, integrationConfigurationDto.AmqpServerConfiguration.BaseUrl);
            Assert.AreEqual(integrationConfigurationEntity.AmqpConnectionIsSecure, integrationConfigurationDto.AmqpServerConfiguration.ConnectionIsSecure);
            Assert.AreEqual(integrationConfigurationEntity.AmqpMessageServerConnectionTimeout, integrationConfigurationDto.AmqpServerConfiguration.ConnectionTimeout);
            Assert.AreEqual(integrationConfigurationEntity.AmqpMessageServerHeartbeat, integrationConfigurationDto.AmqpServerConfiguration.Heartbeat);
            //Assert.AreEqual(integrationConfigurationEntity.AmqpPassword, integrationConfigurationDto.AmqpServerConfiguration.Password);
            Assert.AreEqual(integrationConfigurationEntity.AmqpMessageServerPortNumber, integrationConfigurationDto.AmqpServerConfiguration.PortNumber);
            //Assert.AreEqual(integrationConfigurationEntity.AmqpUsername, integrationConfigurationDto.AmqpServerConfiguration.Username);
            Assert.AreEqual(integrationConfigurationEntity.AmqpMessageServerVirtualHost, integrationConfigurationDto.AmqpServerConfiguration.VirtualHost);

        }

        [TestMethod]
        public void IntegrationConfiguration_ApiConfiguration()
        {
            //Assert.AreEqual(integrationConfigurationEntity.ApiPassword, integrationConfigurationDto.ApiConfiguration.ApiPassword);
            Assert.AreEqual(integrationConfigurationEntity.ApiUsername, integrationConfigurationDto.ApiConfiguration.ApiUsername);
            Assert.AreEqual(integrationConfigurationEntity.ApiErpName, integrationConfigurationDto.ApiConfiguration.ErpName);
            for (int i = 0; i < integrationConfigurationDto.ApiConfiguration.ResourceMappings.Count; i++)
            {
                Assert.AreEqual(integrationConfigurationEntity.ResourceBusinessEventMappings[i].PathSegment, integrationConfigurationDto.ApiConfiguration.ResourceMappings[i].PathSegment);
                Assert.AreEqual(integrationConfigurationEntity.ResourceBusinessEventMappings[i].ResourceName, integrationConfigurationDto.ApiConfiguration.ResourceMappings[i].ResourceName);
                Assert.AreEqual(integrationConfigurationEntity.ResourceBusinessEventMappings[i].ResourceVersion, integrationConfigurationDto.ApiConfiguration.ResourceMappings[i].ResourceVersion);
                for (int j = 0; j < integrationConfigurationDto.ApiConfiguration.ResourceMappings[i].BusinessEvents.Count; j++)
                {
                    Assert.AreEqual(integrationConfigurationDto.ApiConfiguration.ResourceMappings[i].BusinessEvents[j], integrationConfigurationDto.ApiConfiguration.ResourceMappings[i].BusinessEvents[j]);
                }
            }
        }

        [TestMethod]
        public void IntegrationConfiguration_IntegartionHubConfiguration()
        {
            Assert.AreEqual(integrationConfigurationEntity.UseIntegrationHub, integrationConfigurationDto.UseIntegrationHub);
            //Assert.AreEqual(integrationConfigurationEntity.ApiKey, integrationConfigurationDto.IntegrationHubConfiguration.ApiKey);
            Assert.AreEqual(integrationConfigurationEntity.TokenUrl, integrationConfigurationDto.IntegrationHubConfiguration.TokenUrl);
            Assert.AreEqual(integrationConfigurationEntity.PublishUrl, integrationConfigurationDto.IntegrationHubConfiguration.PublishUrl);
            Assert.AreEqual(integrationConfigurationEntity.SubscribeUrl, integrationConfigurationDto.IntegrationHubConfiguration.SubscribeUrl);
            Assert.AreEqual(integrationConfigurationEntity.ErrorUrl, integrationConfigurationDto.IntegrationHubConfiguration.ErrorUrl);
            Assert.AreEqual(integrationConfigurationEntity.HubMediaType, integrationConfigurationDto.IntegrationHubConfiguration.HubMediaType);
        }

        [TestMethod]
        public void IntegrationConfiguration_BusinessEventConfiguration()
        {
            Assert.AreEqual(integrationConfigurationEntity.BusinessEventExchangeName, integrationConfigurationDto.BusinessEventConfiguration.ExchangeName);
            Assert.AreEqual(integrationConfigurationEntity.BusinessEventQueueName, integrationConfigurationDto.BusinessEventConfiguration.QueueName);
            for (int i = 0; i < integrationConfigurationDto.BusinessEventConfiguration.RoutingKeys.Count; i++)
            {
                Assert.AreEqual(integrationConfigurationEntity.BusinessEventRoutingKeys[i], integrationConfigurationDto.BusinessEventConfiguration.RoutingKeys[i]);
            }
        }


        [TestMethod]
        public void IntegrationConfiguration_InboundConfiguration()
        {
            Assert.AreEqual(integrationConfigurationEntity.InboundExchangeName, integrationConfigurationDto.InboundConfiguration.ExchangeName);
            Assert.AreEqual(integrationConfigurationEntity.InboundExchangeQueueName, integrationConfigurationDto.InboundConfiguration.QueueName);
            for (int i = 0; i < integrationConfigurationDto.InboundConfiguration.RoutingKeys.Count; i++)
            {
                Assert.AreEqual(integrationConfigurationEntity.InboundExchangeRoutingKeys[i], integrationConfigurationDto.InboundConfiguration.RoutingKeys[i]);
            }
        }

        [TestMethod]
        public void IntegrationConfiguration_OutboundConfiguration()
        {
            Assert.AreEqual(integrationConfigurationEntity.OutboundExchangeName, integrationConfigurationDto.OutboundConfiguration.ExchangeName);
        }

        [TestMethod]
        public void IntegrationConfiguration_DebugLevel_Info()
        {
            Assert.AreEqual("info", integrationConfigurationDto.DebugLevel);
        }

        [TestMethod]
        public void IntegrationConfiguration_DebugLevel_Debug()
        {
            integrationConfigurationEntity = new Domain.Base.Entities.IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, AdapterDebugLevel.Debug, guidLifespan, mappings);

            integrationConfigurationEntityAdapter = new IntegrationConfigurationEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            integrationConfigurationDto = integrationConfigurationEntityAdapter.MapToType(integrationConfigurationEntity);

            Assert.AreEqual("debug", integrationConfigurationDto.DebugLevel);
        }

        [TestMethod]
        public void IntegrationConfiguration_DebugLevel_Error()
        {
            integrationConfigurationEntity = new Domain.Base.Entities.IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, AdapterDebugLevel.Error, guidLifespan, mappings);

            integrationConfigurationEntityAdapter = new IntegrationConfigurationEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            integrationConfigurationDto = integrationConfigurationEntityAdapter.MapToType(integrationConfigurationEntity);

            Assert.AreEqual("error", integrationConfigurationDto.DebugLevel);
        }

        [TestMethod]
        public void IntegrationConfiguration_DebugLevel_Fatal()
        {
            integrationConfigurationEntity = new Domain.Base.Entities.IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, AdapterDebugLevel.Fatal, guidLifespan, mappings);

            integrationConfigurationEntityAdapter = new IntegrationConfigurationEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            integrationConfigurationDto = integrationConfigurationEntityAdapter.MapToType(integrationConfigurationEntity);

            Assert.AreEqual("fatal", integrationConfigurationDto.DebugLevel);
        }

        [TestMethod]
        public void IntegrationConfiguration_DebugLevel_Trace()
        {
            integrationConfigurationEntity = new Domain.Base.Entities.IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, AdapterDebugLevel.Trace, guidLifespan, mappings);

            integrationConfigurationEntityAdapter = new IntegrationConfigurationEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            integrationConfigurationDto = integrationConfigurationEntityAdapter.MapToType(integrationConfigurationEntity);

            Assert.AreEqual("trace", integrationConfigurationDto.DebugLevel);
        }

        [TestMethod]
        public void IntegrationConfiguration_DebugLevel_Warning()
        {
            integrationConfigurationEntity = new Domain.Base.Entities.IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, AdapterDebugLevel.Warning, guidLifespan, mappings);

            integrationConfigurationEntityAdapter = new IntegrationConfigurationEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            integrationConfigurationDto = integrationConfigurationEntityAdapter.MapToType(integrationConfigurationEntity);

            Assert.AreEqual("warn", integrationConfigurationDto.DebugLevel);
        }

        [TestMethod]
        public void IntegrationConfiguration_GuidLifespan()
        {
            Assert.AreEqual(10, integrationConfigurationDto.GuidLifespan);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntegrationConfiguration_NullSource_ThowsException()
        {
            var prDto = integrationConfigurationEntityAdapter.MapToType(null);
        }
    }
}
