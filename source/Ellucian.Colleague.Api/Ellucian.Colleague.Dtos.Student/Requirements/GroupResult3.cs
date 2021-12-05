// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Result of a Group evaluation
    /// </summary>
    
    public class GroupResult3 : BaseResult2
    {
        /// <summary>
        /// Id of the group evaluated
        /// </summary>
        public string GroupId {get; set;}
        /// <summary>
        /// collection of student  AcademicCredits applied to this group
        /// </summary>
        public List<CreditResult> AppliedAcademicCredits { get; set; }
        /// <summary>
        /// collection of planned courses applied to this group
        /// </summary>
         public List<CourseResult> AppliedPlannedCourses { get; set; }
        /// <summary>
        /// Ids of student AcademicCredits manually applied to this group
        /// </summary>
        public List<string> ForceAppliedAcademicCreditIds { get; set; }
        /// <summary>
        /// Ids of student AcademicCredits manually excluded from application to this group
        /// </summary>
        public List<string> ForceDeniedAcademicCreditIds { get; set; }
        /// <summary>
        /// List of planned courses applied to this group
        /// </summary>
        public List<PlannedCredit> AppliedPlannedCredits { get; set; }
        /// <summary>
        /// Ids of additional academic credits that are included in the GPA calculation
        /// </summary>
        public List<string> AcademicCreditIdsIncludedInGPA { get; set; }
        /// <summary>
        /// Collection of student's academic credits that are not applied but are related to this group.
        /// Related Academic Credits are populated only when AEDF related policy is Together and a flag to honor related policy is Yes. 
        /// Related Academic Credits will be Null if above conditions are not met.
        /// </summary>
        public List<CreditResult> RelatedAcademicCredits { get; set; }
        /// <summary>
        /// Collection of student's planned credits that are not applied but are related to this group.
        /// </summary>
        public List<PlannedCredit> RelatedPlannedCredits { get; set; }
        /// <summary>
        /// Further explanations related to the group result
        /// </summary>
        public List<GroupExplanation> Explanations {get; set;}
        /// <summary>
        /// status of group result for min groups
        /// </summary>
        public GroupResultMinGroupStatus MinGroupStatus { get; set; }
    }

    /// <summary>
    /// enum to define explananation for Acad credit or course. This implies whether acad credit/course that is applied is 'Extra' to requirement completion.
    /// Default Value is 'None'.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GroupResultMinGroupStatus
    {
        /// <summary>
        /// none
        /// </summary>
        None,
        /// <summary>
        /// ignore
        /// </summary>
        Ignore,
        /// <summary>
        /// extra
        /// </summary>
        Extra

    }
}
