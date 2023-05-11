// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student.AnonymousGrading
{
    /// <summary>
    /// Preliminary anonymous grade
    /// </summary>
    public class PreliminaryAnonymousGrade
    {
        /// <summary>
        /// Anonymous grading ID for the preliminary anonymous grade
        /// </summary>
        public string AnonymousGradingId { get; set; }
        /// <summary>
        /// Anonymous grading ID for MidTerm grades
        /// </summary>
        public string AnonymousMidTermGradingId { get; set; }

        /// <summary>
        /// ID for the final grade for the preliminary anonymous grade
        /// </summary>
        public string FinalGradeId { get; set; }

        /// <summary>
        /// ID for the course section to which the preliminary anonymous grade applies
        /// </summary>
        public string CourseSectionId { get; set; }

        /// <summary>
        /// ID of the associated student course section data
        /// </summary>
        public string StudentCourseSectionId { get; set; }

        /// <summary>
        /// Optional date on which the associated final grade expires
        /// </summary>
        public DateTime? FinalGradeExpirationDate { get; set; }
    }
}
