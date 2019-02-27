//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The student types associated with the student.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentCohortsDtoProperty
    {
        /// <summary>
        /// A cohort for a student
        /// </summary>
        [FilterProperty("criteria")]
        [JsonProperty("cohort", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Cohort { get; set; }

        /// <summary>
        /// The start date of the cohort for a student
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("startOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? StartOn { get; set; }

        /// <summary>
        /// The end date date of the cohort for a student
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("endOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? EndOn { get; set; }

        /// <summary>
        /// The starting administrative period for the cohort. (Banner only)
        /// </summary>
        [JsonProperty("startAdministrativePeriod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 StartAdministrativePeriod { get; set; }

        /// <summary>
        /// The ending administrative period for the cohort. (Banner only)
        /// </summary>
        [JsonProperty("endAdministrativePeriod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 EndAdministrativePeriod { get; set; }
    }
}