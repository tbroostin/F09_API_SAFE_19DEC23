// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Result of a Group evaluation
    /// </summary>
    
    public class GroupResult2 : BaseResult2
    {
        /// <summary>
        /// Id of the group evaluated
        /// </summary>
        public string GroupId {get; set;}
        /// <summary>
        /// Ids of student AcademicCredits applied to this group
        /// </summary>
        public List<string> AppliedAcademicCreditIds { get; set; }
        /// <summary>
        /// Course Ids of planned courses applied to this group
        /// </summary>
        public List<string> AppliedPlannedCourseIds { get; set; }
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
        /// Further explanations related to the group result
        /// </summary>
        public List<GroupExplanation> Explanations {get; set;}
    }
}
