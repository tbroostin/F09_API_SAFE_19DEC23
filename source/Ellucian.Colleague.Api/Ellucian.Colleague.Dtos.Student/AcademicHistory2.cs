// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// All of a student's term-based and non-term academic credits
    /// </summary>
    public class AcademicHistory2
    {
        /// <summary>
        /// List of terms in which student earned credit <see cref="AcademicTerm"/>
        /// </summary>
        public List<AcademicTerm2> AcademicTerms { get; set; }
        /// <summary>
        /// List of credit items not applicable to a particular term <see cref="AcademicCredit"/>
        /// </summary>
        public List<AcademicCredit2> NonTermAcademicCredits { get; set; }
        /// <summary>
        /// Grade restriction <see cref="GradeRestriction"/>
        /// </summary>
        public GradeRestriction GradeRestriction { get; set; }        
        /// <summary>
        /// Total Credits Completed from all term and non-term credit
        /// </summary>
        public decimal TotalCreditsCompleted { get; set; }
        /// <summary>
        /// Overall GPA based on all term and non-term credit
        /// </summary>
        public decimal OverallGradePointAverage { get; set; }
        /// <summary>
        /// Reference back to the student record necessary when processing several students
        /// </summary>
        public string StudentId { get; set; }


        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public AcademicHistory2()
        {
            AcademicTerms = new List<AcademicTerm2>();
            NonTermAcademicCredits = new List<AcademicCredit2>();
        }
    }
}
