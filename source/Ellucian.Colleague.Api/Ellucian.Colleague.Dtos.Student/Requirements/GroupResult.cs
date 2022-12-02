// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Result of a Group evaluation
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")] 
    public class GroupResult : BaseResult
    {
        /// <summary>
        /// Id of the group evaluated
        /// </summary>
        public string GroupId { get; set; }
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

    }
}
