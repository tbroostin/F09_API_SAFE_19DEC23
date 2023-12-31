//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The information about grade point averages for a student. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentGradePointAverages: BaseModel2
    {
        /// <summary>
        /// The student associated with the grade point averages.
        /// </summary>

        [JsonProperty("student", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 Student { get; set; }

        /// <summary>
        /// The period based grade point averages for the student.
        /// </summary>

        [JsonProperty("periodBased", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<StudentGradePointAveragesPeriodBasedDtoProperty> PeriodBased { get; set; }

        /// <summary>
        /// The cumulative grade point averages for the student.
        /// </summary>

        [JsonProperty("cumulative", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<StudentGradePointAveragesCumulativeDtoProperty> Cumulative { get; set; }

        /// <summary>
        /// The grade point average of the student for earned degrees.
        /// </summary>

        [JsonProperty("earnedDegrees", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<StudentGradePointAveragesEarnedDegreesDtoProperty> EarnedDegrees { get; set; }

    }
}