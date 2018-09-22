using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// All the AcademicCredit for a student for a given term. Included in student's AcademicHistory
    /// </summary>
    public class AcademicTerm
    {
        /// <summary>
        /// Unique term Id
        /// </summary>
        public string TermId { get; set; }
        /// <summary>
        /// Decimal GPA for this term
        /// </summary>
        public decimal GradePointAverage { get; set; }
        /// <summary>
        /// Total credits earned for the term
        /// </summary>
        public decimal Credits { get; set; }
        /// <summary>
        /// Total continuing education units
        /// </summary>
        public decimal ContinuingEducationUnits { get; set; }
        /// <summary>
        /// List of <see cref="AcademicCredit">credit items </see>
        /// </summary>
        public List<AcademicCredit> AcademicCredits { get; set; }

        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public AcademicTerm()
        {
            AcademicCredits = new List<AcademicCredit>();
        }
    }
}
