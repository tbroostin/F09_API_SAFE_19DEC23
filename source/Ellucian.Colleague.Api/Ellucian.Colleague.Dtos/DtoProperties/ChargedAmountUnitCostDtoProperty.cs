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
    public class ChargedAmountUnitCostDtoProperty
    {
        /// <summary>
        /// Quantity times value is total cost
        /// </summary>
        [JsonProperty("quantity")]
        public long? Quantity { get; set; }

        /// <summary>
        /// The monetary value for the specified currency.
        /// </summary>
        [JsonProperty("cost", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AmountDtoProperty Cost { get; set; }
    }
}