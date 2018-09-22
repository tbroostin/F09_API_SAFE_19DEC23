// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The number of hours worked per period.
    /// </summary>
    [DataContract]
    public class HoursPerPeriodDtoProperty
    {
        /// <summary>
        /// the time period for which working hours are considered.
        /// </summary>
        [JsonProperty("period")]
        public Ellucian.Colleague.Dtos.EnumProperties.PayPeriods? Period { get; set; }

        /// <summary>
        /// The number of hours worked per period. This property represents the number of hours worked per period.
        /// </summary>
        [JsonProperty("hours", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal? Hours { get; set; }
    }
}