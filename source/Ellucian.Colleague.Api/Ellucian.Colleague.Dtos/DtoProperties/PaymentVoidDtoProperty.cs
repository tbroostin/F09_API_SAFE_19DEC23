//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The void type and data associated with the transaction
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PaymentVoidDtoProperty
    {

        /// <summary>
        /// The void type of the transaction
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PaymentTransactionsType Type { get; set; }

        /// <summary>
        /// The date the payment occurred.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime Date { get; set; }

    }
}