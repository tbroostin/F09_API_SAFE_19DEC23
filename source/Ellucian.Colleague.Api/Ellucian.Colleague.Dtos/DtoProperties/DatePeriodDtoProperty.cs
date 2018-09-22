//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;
using System;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The date period
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DatePeriodDtoProperty
    {

        /// <summary>
        /// The date started.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("startOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The date ended.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("endOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? EndDate { get; set; }

    }
}