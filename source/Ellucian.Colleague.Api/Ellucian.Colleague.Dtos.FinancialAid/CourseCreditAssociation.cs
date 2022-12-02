/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// CourseCreditAssociation entity contains each course credit for a specific student/award year/award period
    /// </summary>
    public class CourseCreditAssociation
    {
        /// <summary>
        /// Colleague PERSON id of the student to whom this credit belongs
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The Award Year this credit belongs to
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// The Award Period that this credit belongs to
        /// </summary>
        public string AwardPeriod { get; set; }

        /// <summary>
        /// The Course Name for this credit
        /// </summary>
        public string CourseName { get; set; }

        /// <summary>
        /// The Course Title for this credit
        /// </summary>
        public string CourseTitle { get; set; }

        /// <summary>
        /// The Course Section number for this course
        /// </summary>
        public string CourseSection { get; set; }

        /// <summary>
        /// The Term that this course belongs to (since there can be many terms to one award period)
        /// </summary>
        public string CourseTerm { get; set; }

        /// <summary>
        /// The number of credits that this course is for (non-FA)
        /// </summary>
        public string CourseCred { get; set; }

        /// <summary>
        /// The number of Inst credits that this course is for
        /// </summary>
        public string CourseInstCred { get; set; }

        /// <summary>
        /// The number of Title IV credits that this course is for
        /// </summary>
        public string CourseTivCred { get; set; }

        /// <summary>
        /// The number of Pell credits that this course is for
        /// </summary>
        public string CoursePellCred { get; set; }

        /// <summary>
        /// The number of Direct Loan credits that this course is for
        /// </summary>
        public string CourseDlCred { get; set; }
        /// <summary>
        /// Flag indicating if the course is included in the student's program
        /// </summary>
        public bool CourseProgFlag { get; set; }
        /// <summary>
        /// Flag indicating if Degree Audit is active for this course's award period
        /// </summary>
        public bool DegreeAuditActiveFlag { get; set; }
    }
}
