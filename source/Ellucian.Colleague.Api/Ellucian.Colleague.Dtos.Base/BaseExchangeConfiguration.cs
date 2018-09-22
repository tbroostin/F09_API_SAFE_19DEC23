// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Base Exchange Configuration Information
    /// </summary>
    [DataContract]
    public class BaseExchangeConfiguration
    {
        /// <summary>
        /// Name of the exchange
        /// </summary>
        [DataMember(Name = "exchangeName")]
        public string ExchangeName { get; set; }

        /// <summary>
        /// Constructor for BaseExchangeConfiguration
        /// </summary>
        /// <param name="name">Exchange Name</param>
        public BaseExchangeConfiguration(string name)
        {
            ExchangeName = name;
        }
    }
}
