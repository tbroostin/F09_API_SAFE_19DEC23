// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Used to determine the status of the course in preview degree plan.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DegreePlanPreviewCourseEvaluationStatusType
    {
        /// <summary>
        /// This status is set when the student has taken the course listed in preview degree plan and the course is applied in program's evaluation.
        /// </summary>
        Applied,

        /// <summary>
        /// This status is set when the student has not taken the course listed in preview degree plan or student took the course but is not applied in program's evaluation.
        /// </summary>
        NonApplied,

        /// <summary>
        /// This status is set when a course was taken but did not meet Minimum grade in program evaluation. 
        /// </summary>
        MinGrade


    }
}
