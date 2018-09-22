// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Section Registration Interval DTO property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SectionRegistrationIntervalDtoProperty
    {
        /// <summary>
        /// Registration interval unit
        /// </summary>
        [JsonProperty("unit", DefaultValueHandling = DefaultValueHandling.Include)]
        public SectionWaitlistRegistrationIntervalUnit? Unit { get; set; }

        /// <summary>
        /// Registration interval value
        /// </summary>
        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? Value { get; set; }
    }
}
