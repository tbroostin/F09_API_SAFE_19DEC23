// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Instructional Event query parameters
    /// </summary>
    public class InstructionalEventFilter3
    {
        /// <summary>
        /// The section for which this event occurs
        /// </summary>
        [JsonProperty("section")]
        [FilterProperty("criteria")]
        public GuidObject2 Section { get; set; }

        /// <summary>
        /// instructor
        /// </summary>        
        [JsonProperty("academicPeriod")]
        [FilterProperty("criteria")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 AcademicPeriod { get; set; }

        /// <summary>
        /// The recurrence information for this instructional event
        /// </summary>
        [JsonProperty("recurrence")]
        [FilterProperty("criteria")]
        public RecurrenceFilter Recurrence { get; set; }
    }
}
