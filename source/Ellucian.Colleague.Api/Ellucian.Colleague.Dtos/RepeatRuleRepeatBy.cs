// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// How to calculate the day of the month on which to start repeating
    /// </summary>
   [DataContract(Name = "repeatBy")]
    public class RepeatRuleRepeatBy
    {
        /// <summary>
        /// The cardinal day of the week (e.g., the first Tuesday, the last Friday, etc.) in the month 
        /// on which to start repeating.
        /// </summary>
       [JsonProperty("dayOfWeek", NullValueHandling = NullValueHandling.Ignore)]
        public RepeatRuleDayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// The number of the day in the month on which to start repeating.
        /// </summary>
       [JsonProperty("dayOfMonth", NullValueHandling = NullValueHandling.Ignore)]
        public int? DayOfMonth { get; set; }

        /// <summary>
        /// Constructor for RepeatRuleRepeatBy
        /// </summary>
        [JsonConstructor]
        public RepeatRuleRepeatBy()
        {
        }
    }
}


    