//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The last date or status of attendance associated with the unverified grade
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentUnverifiedGradesLastAttendanceDtoProperty
    {
        /// <summary>
        /// The last date of attendance associated with the unverified grade
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// The status of attendance
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StudentUnverifiedGradesStatus Status { get; set; }
    }
}