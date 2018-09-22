// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The cardinal day of the week (e.g., the first Tuesday, the last Friday, etc.) in the month 
    /// on which to start repeating.
    /// </summary>
    [DataContract(Name = "dayOfWeek")]
    public class RepeatRuleDayOfWeek
    {
        /// <summary>
        /// The day of the week on which the events fall
        /// </summary>
        [JsonProperty("day")]
        public HedmDayOfWeek? Day { get; set; }

        /// <summary>
        ///  The nth occurrence of the day of the week in the month. 
        ///  Use a positive number to count from the start of the month (e.g., 1 = first in the month, 2 = second in the month), 
        ///  and a negative number to count from the end of the month (e.g. -1 = last in the month, -2 = second from last, etc.).                                                         
        /// </summary>
        [JsonProperty("occurrence")]
        public int Occurrence { get; set; }

        /// <summary>
        /// Constructor for RepeatRuleEnds
        /// </summary>
        [JsonConstructor]
        public RepeatRuleDayOfWeek()
        {
        }
    }
}