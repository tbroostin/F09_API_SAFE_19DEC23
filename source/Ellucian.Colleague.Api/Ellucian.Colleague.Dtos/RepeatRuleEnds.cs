// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// When to end the daily repetitions
    /// </summary>
    [DataContract(Name = "ends")]
    public class RepeatRuleEnds
    {
        /// <summary>
        /// When to end the daily repetitions
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Number of times to repeat, then stop
        /// </summary>
        [JsonProperty("repetitions", NullValueHandling = NullValueHandling.Ignore)]
        public int? Repetitions { get; set; }

        /// <summary>
        /// Constructor for RepeatRuleEnds
        /// </summary>
        [JsonConstructor]
        public RepeatRuleEnds()
        {
        }
    }
}