﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The amount and currency values for a general ledger transaction
    /// </summary>
    [DataContract]
    public class AmountDtoProperty
    {
        /// <summary>
        /// The monetary value for the specified currency.
        /// </summary>
        [JsonProperty("value")]
        public decimal? Value { get; set; }

        /// <summary>
        /// The ISO 4217 currency code
        /// </summary>
        [JsonProperty("currency")]
        public CurrencyCodes? Currency { get; set; }
    }
}