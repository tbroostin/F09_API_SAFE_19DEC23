using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Coordination.Planning
{
    /// <summary>
    /// ArchivedCourse objects are used to build degree plan archive reports.  They aren't exposed directly via an API.
    /// </summary>
    public class ArchivedCourse
    {
        /// <summary>
        /// Course ID of the archived course
        /// </summary>
        public string CourseId { get; set; }
        /// <summary>
        /// Section Id of the archived course (optional in an archive course)
        /// </summary>
        public string SectionId { get; set; }
        /// <summary>
        /// Credits of the archived course
        /// </summary>
        public decimal? Credits { get; set; }
        /// <summary>
        /// Formatted display of credits for the archived course
        /// </summary>
        public string FormattedCredits { get; set; }
        /// <summary>
        /// Name of the archived courses, such as MATH-101. If there is a section the section component will also be included.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Title of the archived course. This will be the course title at the time it was archived. If there was a section with a different title that title will be used.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Approval status of the archived course. This can be approved, denied, planned or blank.
        /// </summary>
        public string ApprovalStatus { get; set; }
        /// <summary>
        /// Name of the advisor who approved or denied the archived course. (Last name, first initial).
        /// </summary>
        public string ApprovedBy { get; set; }
        /// <summary>
        /// Formatted date when the approval or denial was granted. This uses ToShortDateString.
        /// </summary>
        public string ApprovalDate { get; set; }
        /// <summary>
        /// Term code for the archived course. Can be blank for a non-term section
        /// </summary>
        public string TermCode { get; set; }
        /// <summary>
        /// Description of the term on the archived course
        /// </summary>
        public string TermDescription { get; set; }
        /// <summary>
        /// Reporting year of the term on the archived course. Used for sorting
        /// </summary>
        public int TermReportingYear { get; set; }
        /// <summary>
        /// Sequence of the term within a term reporting year. Used for sorting
        /// </summary>
        public int TermSequence { get; set; }
        /// <summary>
        /// If the planned course had continuing education units instead of credits this will be filled in
        /// </summary>
        public decimal? ContinuingEducationUnits { get; set; }
        /// <summary>
        /// Formatted display of the continuing education units
        /// </summary>
        public string FormattedCeus { get; set; }
        /// <summary>
        /// Name of the person who added the particular archived course to the plan
        /// </summary>
        public string AddedBy { get; set; }
        /// <summary>
        /// Date when the particular archived course was added to the plan
        /// </summary>
        public string AddedDate { get; set; }
        /// <summary>
        /// If the student has academic credit for the archived course, this is the registration status of that item.
        /// </summary>
        public string RegistrationStatus { get; set; }

        public ArchivedCourse()
        {

        }
    }

}
