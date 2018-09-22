using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public string GroupId;
        /// <summary>
        /// Ids of student AcademicCredits applied to this group
        /// </summary>
        public List<string> AppliedAcademicCreditIds;
        /// <summary>
        /// Course Ids of planned courses applied to this group
        /// </summary>
        public List<string> AppliedPlannedCourseIds;
        /// <summary>
        /// Ids of student AcademicCredits manually applied to this group
        /// </summary>
        public List<string> ForceAppliedAcademicCreditIds;
        /// <summary>
        /// Ids of student AcademicCredits manually excluded from application to this group
        /// </summary>
        public List<string> ForceDeniedAcademicCreditIds;
        /// <summary>
        /// List of planned courses applied to this group
        /// </summary>
        public List<PlannedCredit> AppliedPlannedCredits;
        /// <summary>
        /// Ids of additional academic credits that are included in the GPA calculation
        /// </summary>
        public List<string> AcademicCreditIdsIncludedInGPA;

    }
}
