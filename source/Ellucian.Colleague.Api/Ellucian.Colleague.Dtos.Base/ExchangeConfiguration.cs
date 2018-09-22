// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Extended Exchange Configuration Information
    /// </summary>
    [DataContract]
    public class ExchangeConfiguration : BaseExchangeConfiguration
    {
        /// <summary>
        /// Name of the event queue to which messages are directed
        /// </summary>
        [DataMember(Name = "queueName")]
        public string QueueName { get; set; }

        /// <summary>
        /// Collection of keys used to direct/filter messages to the event queue
        /// </summary>
        [DataMember(Name = "routingKeys")]
        public List<string> RoutingKeys { get; set; }

        /// <summary>
        /// Constructor for BaseExchangeConfiguration
        /// </summary>
        /// <param name="name">Exchange Name</param>
        public ExchangeConfiguration(string name) : base(name)
        {
            ExchangeName = name;
            RoutingKeys = new List<string>();
        }
    }
}
