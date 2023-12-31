﻿//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
    public class StudentUnverifiedGradesDetails
    {
        /// <summary>
        /// The grade details.
        /// </summary>

        [JsonProperty("grades", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<StudentUnverifiedGradesGrades> Grades { get; set; }

        /// <summary>
        /// The details regarding the student's last known attendance.
        /// </summary>

        [JsonProperty("lastAttendance", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StudentUnverifiedGradesLastAttendance LastAttendance { get; set; }

    }
}

