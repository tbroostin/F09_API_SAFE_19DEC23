//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The grades eligible to be displayed on the student's transcript. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentTranscriptGradesOptions : BaseModel2
    {
        /// <summary>
        /// The grading options associated with the student transcript grade.
        /// </summary>
        [JsonProperty("grades", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<StudentTranscriptGradesOptionsGradesDtoProperty> Grades { get; set; }

        /// <summary>
        /// The grading scheme used to award the student their grade.
        /// </summary>
        [JsonProperty("gradeScheme", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StudentTranscriptGradesOptionsGradeSchemeDtoProperty GradeScheme { get; set; }        
    }
}