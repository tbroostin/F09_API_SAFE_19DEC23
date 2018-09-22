// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The amount and currency values for a general ledger transaction
    /// </summary>
    [DataContract]
    public class ChargedAmountDtoProperty
    {
        /// <summary>
        /// The monetary value for the specified currency.
        /// </summary>
        [JsonProperty("amount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AmountDtoProperty  Amount { get; set; }

        /// <summary>
        /// The ISO 4217 currency code
        /// </summary>
        [JsonProperty("unitCost", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ChargedAmountUnitCostDtoProperty UnitCost { get; set; }
    }
}