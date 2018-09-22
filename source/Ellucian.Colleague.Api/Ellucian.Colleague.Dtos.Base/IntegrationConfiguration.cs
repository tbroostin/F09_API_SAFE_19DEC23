// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Integration Configuration Information
    /// </summary>
    [DataContract]
    public class IntegrationConfiguration
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Advanced Message Queuing Protocol Server Configuration Information
        /// </summary>
        [DataMember(Name = "amqpServerConfig")]
        public AmqpServerConfiguration AmqpServerConfiguration { get; set; }

        /// <summary>
        /// Business Event Exchange Queue Information
        /// </summary>
        [DataMember(Name = "erpEventConfig")]
        public ExchangeConfiguration BusinessEventConfiguration { get; set; }

        /// <summary>
        /// Inbound CDM Exchange Queue Information
        /// </summary>
        [DataMember(Name = "messageInConfig")]
        public ExchangeConfiguration InboundConfiguration { get; set; }

        /// <summary>
        /// Outbound CDM Exchange Queue Information
        /// </summary>
        [DataMember(Name = "messageOutConfig")]
        public BaseExchangeConfiguration OutboundConfiguration { get; set; }

        /// <summary>
        /// ERP API Mapping Configuration
        /// </summary>
        [DataMember(Name = "erpApiConfig")]
        public ErpApiConfiguration ApiConfiguration { get; set; }

        /// <summary>
        /// GUID Lifespan
        /// </summary>
        [DataMember(Name = "guidLifespan")]
        public int GuidLifespan { get; set; }

        /// <summary>
        /// Level of detail for debugging
        /// </summary>
        [DataMember(Name = "logLevel")]
        public string DebugLevel { get; set; }

        /// <summary>
        /// Flag indicating if integration hub is used
        /// </summary>
        [DataMember(Name = "useIntegrationHub")]
        public bool UseIntegrationHub { get; set; }

        /// <summary>
        /// Integration Hub Configuration Information
        /// </summary>
        [DataMember(Name = "integrationHubConfig")]
        public IntegrationHubConfiguration IntegrationHubConfiguration { get; set; }

    }
}