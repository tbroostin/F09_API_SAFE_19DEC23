//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The grading options associated with the student transcript grade.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentTranscriptGradesOptionsGradesDtoProperty
    {
        /// <summary>
        /// A grade that may be awarded to the student by the instructor.
        /// </summary>

        [JsonProperty("grade", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Grade { get; set; }

        /// <summary>
        /// The grade value.
        /// </summary>
        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Value { get; set; }
    }
}