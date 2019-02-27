//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The grade submission details to be applied.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentUnverifiedGradesGradeDtoProperty
    {
        /// <summary>
        /// The type of grade.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Type { get; set; }
        
        /// <summary>
        /// The grade that is being submitted.
        /// </summary>
        [JsonProperty("grade", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Grade { get; set; }

        /// <summary>
        /// The incomplete grade details that are being submitted
        /// </summary>
        [JsonProperty("incompleteGrade", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StudentUnverifiedGradesIncompleteGradeDtoProperty IncompleteGrade { get; set; }
    }
}