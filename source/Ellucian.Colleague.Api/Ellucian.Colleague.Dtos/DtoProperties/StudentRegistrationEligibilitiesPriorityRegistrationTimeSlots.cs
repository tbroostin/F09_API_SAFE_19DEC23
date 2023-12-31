//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Time periods when the student is assigned priority registration. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentRegistrationEligibilitiesPriorityRegistrationTimeSlots
    {
        /// <summary>
        /// The date and time when the student's priority registration starts.
        /// </summary>
        [JsonProperty("startOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset StartOn { get; set; }

        /// <summary>
        /// The date and time when the student's priority registration ends.
        /// </summary>
        [JsonProperty("endOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset EndOn { get; set; }
    }
}

