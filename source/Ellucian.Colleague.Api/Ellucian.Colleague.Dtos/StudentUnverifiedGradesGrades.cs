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
    /// The grade details. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentUnverifiedGradesGrades
    {
        /// <summary>
        /// The actual grade.
        /// </summary>

        [JsonProperty("grade", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Grade { get; set; }

        /// <summary>
        /// The type of grade (e.g. midterm, final).
        /// </summary>

        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Type { get; set; }

        /// <summary>
        /// The date the unverified grade was submitted.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("submittedOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? SubmittedOn { get; set; }


    }
}

