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
    /// The grade and/or last attendance details. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentUnverifiedGradesLastAttendance
    {
        /// <summary>
        /// The last date the student attended the course.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// The details of the student's last known attendance
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StudentUnverifiedGradesStatus Status { get; set; }
    }
}

