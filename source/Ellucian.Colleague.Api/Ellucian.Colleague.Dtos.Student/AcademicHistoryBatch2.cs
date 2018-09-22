// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// All of a student's term-based and non-term academic credits
    /// </summary>
    public class AcademicHistoryBatch2
    {
        /// <summary>
        /// List of terms in which student earned credit <see cref="AcademicTerm"/>
        /// </summary>
        public List<AcademicTerm3> AcademicTerms { get; set; }
        /// <summary>
        /// List of credit items not applicable to a particular term <see cref="AcademicCredit"/>
        /// </summary>
        public List<AcademicCredit2> NonTermAcademicCredits { get; set; }
        /// <summary>
        /// Total Credits Completed from all term and non-term credit
        /// </summary>
        public decimal TotalCreditsCompleted { get; set; }
        /// <summary>
        /// Overall GPA based on all term and non-term credit, can be null if no credits completed.
        /// </summary>
        public decimal? OverallGradePointAverage { get; set; }
        /// <summary>
        /// Total Transfer Credits Completed from all term and non-term credit
        /// </summary>
        public decimal TotalTransferCreditsCompleted { get; set; }
        /// <summary>
        /// Overall Transfer GPA based on all term and non-term credit, can be null if no credits completed.
        /// </summary>
        public decimal? OverallTransferGradePointAverage { get; set; }
        /// <summary>
        /// Reference back to the student record necessary when processing several students
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// First Term Enrolled at this academic level
        /// </summary>
        public string FirstTermEnrolled { get; set; }

        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public AcademicHistoryBatch2()
        {
            AcademicTerms = new List<AcademicTerm3>();
            NonTermAcademicCredits = new List<AcademicCredit2>();
        }
    }
}
