// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Configuration information for Colleague integration with third party applications. 
    /// </summary>
    [Serializable]
    public class IntegrationConfiguration
    {
        private string _id;
        private string _description;
        private string _amqpMessageServerBaseUrl;
        private bool _amqpConnectionIsSecure;
        private int _amqpMessageServerPortNumber;
        private string _amqpUsername;
        private string _amqpPassword;
        private string _businessEventExchangeName;
        private string _businessEventQueueName;
        private string _outboundExchangeName;
        private string _inboundExchangeName;
        private string _inboundExchangeQueueName;
        private string _apiUsername;
        private string _apiPassword;
        private string _apiErpName;
        private AdapterDebugLevel _debugLevel;
        private int _guidLifespan;
        private readonly List<ResourceBusinessEventMapping> _resourceBusinessEventMappings = new List<ResourceBusinessEventMapping>();
       
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Description of the configuration info
        /// </summary>
        public string Description { get { return _description; } }

        /// <summary>
        /// Advanced Message Queuing Protocol Server Base URL
        /// </summary>
        public string AmqpMessageServerBaseUrl { get { return _amqpMessageServerBaseUrl; } }

        /// <summary>
        /// Flag indicating whether or not the Advanced Message Queuing Protocol Server connection is secured
        /// </summary>
        public bool AmqpConnectionIsSecure { get { return _amqpConnectionIsSecure; } }

        /// <summary>
        /// Advanced Message Queuing Protocol Server Port Number
        /// </summary>
        public int AmqpMessageServerPortNumber { get { return _amqpMessageServerPortNumber; } }

        /// <summary>
        /// Username for Advanced Message Queuing Protocol Server
        /// </summary>
        public string AmqpUsername { get { return _amqpUsername; } }

        /// <summary>
        /// Password for Advanced Message Queuing Protocol Server
        /// </summary>
        public string AmqpPassword { get { return _amqpPassword; } }

        /// <summary>
        /// Name of the Advanced Message Queuing Protocol exchange
        /// </summary>
        public string BusinessEventExchangeName { get { return _businessEventExchangeName; } }

        /// <summary>
        /// Name of the event queue to which Advanced Message Queuing Protocol messages are directed
        /// </summary>
        public string BusinessEventQueueName { get { return _businessEventQueueName; } }

        /// <summary>
        /// Name of Advanced Message Queuing Protocol exchange to which CDM objects are published for third party consumption
        /// </summary>
        public string OutboundExchangeName { get { return _outboundExchangeName; } }

        /// <summary>
        /// Name of Advanced Message Queuing Protocol exchange to which CDM objects are published by third parties for ERP consumption
        /// </summary>
        public string InboundExchangeName { get { return _inboundExchangeName; } }

        /// <summary>
        /// Name of the queue to which inbound exchange messages are directed
        /// </summary>
        public string InboundExchangeQueueName { get { return _inboundExchangeQueueName; } }

        /// <summary>
        /// Username for basic authentication to Colleague Web API 
        /// </summary>
        public string ApiUsername { get { return _apiUsername; } }

        /// <summary>
        /// Password for basic authentication to Colleague Web API 
        /// </summary>
        public string ApiPassword { get { return _apiPassword; } }

        /// <summary>
        /// Name of the ERP for the Colleague Web API
        /// </summary>
        public string ApiErpName { get { return _apiErpName; } }

        /// <summary>
        /// Name of the virtual host that specifies the namespace for entities (exchanges and queues) referred to by the protocol.
        /// </summary>
        public string AmqpMessageServerVirtualHost { get; set; }

        /// <summary>
        /// Time (in seconds) to wait while establishing a connection with the Advanced Message Queuing Protocol server
        /// </summary>
        public int? AmqpMessageServerConnectionTimeout { get; set; }

        /// <summary>
        /// Flag indicating whether or not to automatically recover Advanced Message Queuing Protocol messages
        /// </summary>
        public bool AutomaticallyRecoverAmqpMessages { get; set; }

        /// <summary>
        /// Heartbeat value (in seconds) to negotiate with the Advanced Message Queuing Protocol server
        /// </summary>
        public int? AmqpMessageServerHeartbeat { get; set; }

        /// <summary>
        /// Debugging Level for the ERP Adapter
        /// </summary>
        public AdapterDebugLevel DebugLevel { get { return _debugLevel; } }

        /// <summary>
        /// Time after which GUIDs expire
        /// </summary>
        public int GuidLifespan { get { return _guidLifespan; } }

        /// <summary>
        /// Collection of keys used to direct/filter messages to the business event queue
        /// </summary>
        public List<string> BusinessEventRoutingKeys { get; set; }

        /// <summary>
        /// Collection of keys used to direct/filter messages to the inbound queue
        /// </summary>
        public List<string> InboundExchangeRoutingKeys { get; set; }

        /// <summary>
        /// Collection of resource-to-business event mappings
        /// </summary>
        public ReadOnlyCollection<ResourceBusinessEventMapping> ResourceBusinessEventMappings { get; private set; }

        /// <summary>
        /// Flag indicating if integration hub is used
        /// </summary>
        public bool UseIntegrationHub { get; set; }

        /// <summary>
        /// API Key of the application from the hub admin UI
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// URL for the hub token service
        /// </summary>
        public string TokenUrl { get; set; }

        /// <summary>
        /// URL for the hub publish service
        /// </summary>
        public string PublishUrl { get; set; }

        /// <summary>
        /// URL for the hub subscribe service
        /// </summary>
        public string SubscribeUrl { get; set; }

        /// <summary>
        /// URL for the hub error reporting service
        /// </summary>
        public string ErrorUrl { get; set; }

        /// <summary>
        /// The media type for messages getting published and retrieved (i.e. application/vnd.hedtech.change-notifications.v2+json)
        /// </summary>
        public string HubMediaType { get; set; }

        /// <summary>
        /// Constructor for IntegrationConfiguration
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="description">Description</param>
        /// <param name="amqpMessageServerUrl">AMQP Server Base URL</param>
        /// <param name="amqpConnectionIsSecure">Secure connection flag</param>
        /// <param name="amqpMessageServerPortNumber">AMQP Server Port Number</param>
        /// <param name="amqpUsername">AMQP Server Username</param>
        /// <param name="amqpPassword">AMQP Server Password</param>
        /// <param name="businessEventExchangeName">Business Event Exchange Name</param>
        /// <param name="businessEventQueueName">Business Event Queue Name</param>
        /// <param name="outboundExchangeName">Outbound Exchange Name</param>
        /// <param name="inboundExchangeName">Inbound Exchange Name</param>
        /// <param name="inboundExchangeQueueName">Inbound Queue Name</param>
        /// <param name="apiUsername">Web API Username</param>
        /// <param name="apiPassword">Web API Password</param>
        /// <param name="apiErpName">ERP Name</param>
        /// <param name="resourceBusinessEventMappings">Resource-to-Business Event Mappings</param>
        public IntegrationConfiguration(string id, string description, string amqpMessageServerUrl, bool amqpConnectionIsSecure,
            int amqpMessageServerPortNumber, string amqpUsername, string amqpPassword, string businessEventExchangeName,
            string businessEventQueueName, string outboundExchangeName, string inboundExchangeName, string inboundExchangeQueueName,
            string apiUsername, string apiPassword, string apiErpName, AdapterDebugLevel debugLevel, int guidDuration,
            IEnumerable<ResourceBusinessEventMapping> mappings)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "An ID must be provided.");
            }
            if (string.IsNullOrEmpty(amqpMessageServerUrl))
            {
                throw new ArgumentNullException("amqpMessageServerUrl", "An AMQP Message Server Base URL must be provided.");
            }
            //if (string.IsNullOrEmpty(amqpUsername))
            //{
            //    throw new ArgumentNullException("amqpUsername", "An AMQP Message Server username must be provided.");
            //}
            //if (string.IsNullOrEmpty(amqpPassword))
            //{
            //    throw new ArgumentNullException("amqpPassword", "An AMQP Message Server password must be provided.");
            //}
            if (string.IsNullOrEmpty(businessEventExchangeName))
            {
                throw new ArgumentNullException("businessEventExchangeName", "A business event exchange name must be provided.");
            }
            if (string.IsNullOrEmpty(businessEventQueueName))
            {
                throw new ArgumentNullException("businessEventQueueName", "A business event queue name must be provided.");
            }
          
            if (string.IsNullOrEmpty(apiUsername))
            {
                throw new ArgumentNullException("apiUsername", "An API username must be provided.");
            }
            //if (string.IsNullOrEmpty(apiPassword))
            //{
            //    throw new ArgumentNullException("apiPassword", "An API password must be provided.");
            //}
            if (string.IsNullOrEmpty(apiErpName))
            {
                throw new ArgumentNullException("apiErpName", "An API password must be provided.");
            }
            if (guidDuration < 5 || guidDuration > 999)
            {
                throw new ArgumentOutOfRangeException("guidDuration", "GUID lifecycle duration must be between 5 and 999.");
            }

            _id = id;
            _description = description;
            _amqpMessageServerBaseUrl = amqpMessageServerUrl;
            _amqpConnectionIsSecure = amqpConnectionIsSecure;
            _amqpMessageServerPortNumber = amqpMessageServerPortNumber;
            _amqpUsername = amqpUsername;
            _amqpPassword = amqpPassword;
            _businessEventExchangeName = businessEventExchangeName;
            _businessEventQueueName = businessEventQueueName;
            _outboundExchangeName = outboundExchangeName;
            _inboundExchangeName = inboundExchangeName;
            _inboundExchangeQueueName = inboundExchangeQueueName;
            _apiUsername = apiUsername;
            _apiPassword = apiPassword;
            _apiErpName = apiErpName;
            _debugLevel = debugLevel;
            _guidLifespan = guidDuration;

            BusinessEventRoutingKeys = new List<string>();
            InboundExchangeRoutingKeys = new List<string>();

            if (mappings != null && mappings.Count() > 0)
            {
                _resourceBusinessEventMappings.AddRange(mappings);
            }
            ResourceBusinessEventMappings = _resourceBusinessEventMappings.AsReadOnly();
        }

        /// <summary>
        /// Add a resource business event mapping to the integration configuration
        /// </summary>
        /// <param name="mapping">Resource Business Event Mapping</param>
        public void AddResourceBusinessEventMapping (ResourceBusinessEventMapping mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping", "A mapping must be specified.");
            }

            // Prevent duplicates
            if (!ResourceBusinessEventMappings.Contains(mapping))
            {
                _resourceBusinessEventMappings.Add(mapping);
            }
        }
    }
}
