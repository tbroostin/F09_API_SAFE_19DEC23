//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The list of valid statuses for courses. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CourseStatuses : CodeItem2
    {
        /// <summary>
        /// The category associated with the course status (active or ended).
        /// </summary>

        [JsonProperty("category", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CourseStatusesCategory? Category { get; set; }

    }
}
