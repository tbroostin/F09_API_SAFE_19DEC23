﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Communicates information about the availability of funds for a specified accounting string.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountFundsAvailable
    {
        /// <summary>
        /// An object containing metadata.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MetaDataDtoProperty MetadataObject { get; set; }

        /// <summary>
        /// The full representation of the accounting string in the format specified by the authoritative system.
        /// </summary>
        [JsonProperty("accountingString")]
        public string AccountingStringValue { get; set; }

        /// <summary>
        /// Status of the availability of funds.
        /// </summary>
        [JsonProperty("fundsAvailable")]
        public FundsAvailable FundsAvailable { get; set; }

        /// <summary>
        /// The date for which the availability of funds was evaluated.
        /// </summary>
        [JsonProperty("balanceOn", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? BalanceOn { get; set; }
    }
}
