// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// An Anonymous Grading Id for a student
    /// </summary>
    public class StudentAnonymousGrading
    {
        /// <summary>
        /// Grading Id for student and term or course section.
        /// </summary>
        public string AnonymousGradingId { get; set; }

        /// <summary>
        /// Grading Id for student and term or course section for Midterm.
        /// </summary>
        public string MidTermGradingId { get; set; }

        /// <summary>
        /// ID of the academic term to which the anonymous grading ID applies.
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// ID of the course section to which the anonymous grading ID applies.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Informational message about the blind grading ID, if necessary.
        /// </summary>
        public string Message { get; set; }
    }
}
