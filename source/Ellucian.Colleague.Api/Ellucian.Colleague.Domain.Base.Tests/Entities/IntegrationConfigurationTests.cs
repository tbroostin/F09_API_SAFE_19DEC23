// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class IntegrationConfigurationTests
    {
        private string id;
        private string description;
        private string amqpMessageServerBaseUrl;
        private bool amqpConnectionIsSecure;
        private int amqpMessageServerPortNumber;
        private string amqpUsername;
        private string amqpPassword;
        private string businessEventExchangeName;
        private string businessEventQueueName;
        private string outboundExchangeName;
        private string inboundExchangeName;
        private string inboundExchangeQueueName;
        private string apiUsername;
        private string apiPassword;
        private string apiErpName;
        private AdapterDebugLevel debugLevel;
        private int guidDuration;
        private bool useIntegrationHub;
        private string apiKey;
        private string tokenUrl;
        private string publishUrl;
        private string subscribeUrl;
        private string errorUrl;
        private string mediaType;


        private IntegrationConfiguration configuration;

        private ResourceBusinessEventMapping mapping1;
        private ResourceBusinessEventMapping mapping2;
        private ResourceBusinessEventMapping mapping3;
        private List<ResourceBusinessEventMapping> mappings = new List<ResourceBusinessEventMapping>();

        [TestInitialize]
        public void Initialize()
        {
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
            guidDuration = 10;
            useIntegrationHub = true;
            apiKey = "APIKEY1";
            tokenUrl = "http://www.toeknmessageserver.com";
            publishUrl = "http://www.publishmessageserver.com";
            subscribeUrl = "http://www.subscribemessageserver.com";
            errorUrl = "http://www.errormessageserver.com";
            mediaType = "application/vnd.hedtech.change-notifications.v2+json";

            mapping1 = new ResourceBusinessEventMapping("course", "1", "/courses/", "event1");
            mapping2 = new ResourceBusinessEventMapping("section", "1", "/sections/", "event2");
            mapping3 = new ResourceBusinessEventMapping("person", "1", "/persons/", "event3");
            mappings.Add(mapping1);
            mappings.Add(mapping2);
        }

        [TestClass]
        public class IntegrationConfiguration_Constructor : IntegrationConfigurationTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_NullId()
            {
                configuration = new IntegrationConfiguration(null, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName,
                    debugLevel, guidDuration, mappings);
                Assert.AreEqual(id, configuration.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_EmptyId()
            {
                configuration = new IntegrationConfiguration(string.Empty, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName,
                    debugLevel, guidDuration, mappings);
                Assert.AreEqual(id, configuration.Id);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidId()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName,
                    debugLevel, guidDuration, mappings);
                Assert.AreEqual(id, configuration.Id);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidDescription()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName,
                    debugLevel, guidDuration, mappings);
                Assert.AreEqual(description, configuration.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_NullAmqpMessageServerBaseUrl()
            {
                configuration = new IntegrationConfiguration(id, description, null, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName,
                    debugLevel, guidDuration, mappings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_EmptyAmqpMessageServerBaseUrl()
            {
                configuration = new IntegrationConfiguration(id, description, string.Empty, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName,
                    debugLevel, guidDuration, mappings);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidAmqpMessageServerBaseUrl()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName,
                    debugLevel, guidDuration, mappings);
                Assert.AreEqual(amqpMessageServerBaseUrl, configuration.AmqpMessageServerBaseUrl);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidAmqpConnectionIsSecureFlag()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(amqpConnectionIsSecure, configuration.AmqpConnectionIsSecure);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidAmqpMessageServerPortNumber()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(amqpMessageServerPortNumber, configuration.AmqpMessageServerPortNumber);
            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public void IntegrationConfiguration_Constructor_NullAmqpUsername()
            //{
            //    configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
            //        amqpMessageServerPortNumber, null, amqpPassword, businessEventExchangeName, businessEventQueueName,
            //        outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public void IntegrationConfiguration_Constructor_EmptyAmqpUsername()
            //{
            //    configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
            //        amqpMessageServerPortNumber, string.Empty, amqpPassword, businessEventExchangeName, businessEventQueueName,
            //        outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
            //}

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidAmqpUsername()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(amqpUsername, configuration.AmqpUsername);
            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public void IntegrationConfiguration_Constructor_NullAmqpPassword()
            //{
            //    configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
            //        amqpMessageServerPortNumber, amqpUsername, null, businessEventExchangeName, businessEventQueueName,
            //        outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public void IntegrationConfiguration_Constructor_EmptyAmqpPassword()
            //{
            //    configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
            //        amqpMessageServerPortNumber, amqpUsername, string.Empty, businessEventExchangeName, businessEventQueueName,
            //        outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
            //}

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidAmqpPassword()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(amqpPassword, configuration.AmqpPassword);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_NullBusinessEventExchangeName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, null, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_EmptyBusinessEventExchangeName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, string.Empty, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidBusinessEventExchangeName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(businessEventExchangeName, configuration.BusinessEventExchangeName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_NullBusinessEventQueueName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, null,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_EmptyBusinessEventQueueName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, string.Empty,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidBusinessEventQueueName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(businessEventQueueName, configuration.BusinessEventQueueName);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_NullOutboundExchange()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    null, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(null, configuration.OutboundExchangeName);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_EmptyOutboundExchange()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    string.Empty, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(string.Empty, configuration.OutboundExchangeName);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidOutboundExchange()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(outboundExchangeName, configuration.OutboundExchangeName);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_NullInboundExchange()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, null, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(null, configuration.InboundExchangeName);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_EmptyInboundExchange()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, string.Empty, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(string.Empty, configuration.InboundExchangeName);

            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidInboundExchange()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(inboundExchangeName, configuration.InboundExchangeName);
            }

            [TestMethod]
           public void IntegrationConfiguration_Constructor_NullInboundExchangeQueueName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, null, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(null, configuration.InboundExchangeQueueName);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_EmptyInboundExchangeQueueName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, string.Empty, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(string.Empty, configuration.InboundExchangeQueueName);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidInboundExchangeQueueName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(inboundExchangeQueueName, configuration.InboundExchangeQueueName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_NullApiUsername()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, null, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_EmptyApiUsername()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, string.Empty, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidApiUsername()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(apiUsername, configuration.ApiUsername);
            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public void IntegrationConfiguration_Constructor_NullApiPassword()
            //{
            //    configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
            //        amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
            //        outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, null, apiErpName, debugLevel, guidDuration, mappings);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public void IntegrationConfiguration_Constructor_EmptyApiPassword()
            //{
            //    configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
            //        amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
            //        outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, string.Empty, apiErpName, debugLevel, guidDuration, mappings);
            //}

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidApiPassword()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(apiPassword, configuration.ApiPassword);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_NullApiErpName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, null, debugLevel, guidDuration, mappings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_Constructor_EmptyApiErpName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, string.Empty, debugLevel, guidDuration, mappings);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidApiErpName()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(apiErpName, configuration.ApiErpName);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidDebugLevel()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(debugLevel, configuration.DebugLevel);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_ValidGuidDuration()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(guidDuration, configuration.GuidLifespan);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void IntegrationConfiguration_Constructor_GuidDurationTooLow()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, 1, mappings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void IntegrationConfiguration_Constructor_GuidDurationTooHigh()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, 1000, mappings);
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_VerifyDefaultBusinessEventRoutingKeys()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(0, configuration.BusinessEventRoutingKeys.Count());
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_VerifyDefaultInboundExchangeRoutingKeys()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                Assert.AreEqual(0, configuration.InboundExchangeRoutingKeys.Count());
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_NullResourceMappings()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, null);
                Assert.AreEqual(0, configuration.ResourceBusinessEventMappings.Count());
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_EmptyResourceMappings()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, new List<ResourceBusinessEventMapping>());
                Assert.AreEqual(0, configuration.ResourceBusinessEventMappings.Count());
            }

            [TestMethod]
            public void IntegrationConfiguration_Constructor_VerifyResourceMappings()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                CollectionAssert.AreEqual(mappings, configuration.ResourceBusinessEventMappings);
            }

        }

        [TestClass]
        public class IntegrationConfiguration_AddResourceBusinessEventMapping : IntegrationConfigurationTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntegrationConfiguration_AddResourceBusinessEventMapping_NullMapping()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.AddResourceBusinessEventMapping(null);
            }

            [TestMethod]
            public void IntegrationConfiguration_AddResourceBusinessEventMapping_ValidMapping()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.AddResourceBusinessEventMapping(mapping3);
                Assert.AreEqual(mapping3, configuration.ResourceBusinessEventMappings[2]);
            }

            [TestMethod]
            public void IntegrationConfiguration_AddResourceBusinessEventMapping_DuplicateMapping()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.AddResourceBusinessEventMapping(mapping1);
                Assert.AreEqual(2, configuration.ResourceBusinessEventMappings.Count);
            }

            [TestMethod]
            public void IntegrationConfiguration_AddResourceBusinessEventMapping_MultipleMappings()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.AddResourceBusinessEventMapping(mapping3);
                Assert.AreEqual(3, configuration.ResourceBusinessEventMappings.Count);
                Assert.AreEqual(mapping1, configuration.ResourceBusinessEventMappings[0]);
                Assert.AreEqual(mapping2, configuration.ResourceBusinessEventMappings[1]);
                Assert.AreEqual(mapping3, configuration.ResourceBusinessEventMappings[2]);
            }
        }

        [TestClass]
        public class IntegrationConfiguration_IntegrationHub : IntegrationConfigurationTests
        {
            
            [TestMethod]
            public void IntegrationConfiguration_IntegrationHub_UseIntegrationHub()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.UseIntegrationHub = useIntegrationHub;
                Assert.AreEqual(useIntegrationHub, configuration.UseIntegrationHub);
            }

            [TestMethod]
            public void IntegrationConfiguration_IntegrationHub_ApiKey()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.ApiKey = apiKey;
                Assert.AreEqual(apiKey, configuration.ApiKey);
            }

            [TestMethod]
            public void IntegrationConfiguration_IntegrationHub_TokenUrl()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.TokenUrl = tokenUrl;
                Assert.AreEqual(tokenUrl, configuration.TokenUrl);
            }

            [TestMethod]
            public void IntegrationConfiguration_IntegrationHub_PublishUrl()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.PublishUrl = publishUrl;
                Assert.AreEqual(publishUrl, configuration.PublishUrl);
            }

            [TestMethod]
            public void IntegrationConfiguration_IntegrationHub_SubscribeUrl()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.SubscribeUrl = subscribeUrl;
                Assert.AreEqual(subscribeUrl, configuration.SubscribeUrl);
            }

            [TestMethod]
            public void IntegrationConfiguration_IntegrationHub_ErrorUrl()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.ErrorUrl = errorUrl;
                Assert.AreEqual(errorUrl, configuration.ErrorUrl);
            }

            [TestMethod]
            public void IntegrationConfiguration_IntegrationHub_MediaType()
            {
                configuration = new IntegrationConfiguration(id, description, amqpMessageServerBaseUrl, amqpConnectionIsSecure,
                    amqpMessageServerPortNumber, amqpUsername, amqpPassword, businessEventExchangeName, businessEventQueueName,
                    outboundExchangeName, inboundExchangeName, inboundExchangeQueueName, apiUsername, apiPassword, apiErpName, debugLevel, guidDuration, mappings);
                configuration.HubMediaType = mediaType;
                Assert.AreEqual(mediaType, configuration.HubMediaType);
            }
        }
    }
}