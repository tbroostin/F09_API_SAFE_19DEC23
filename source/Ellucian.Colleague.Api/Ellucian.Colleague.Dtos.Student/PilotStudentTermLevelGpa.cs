// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student GPA by term by academic level
    /// </summary>
    public class PilotStudentTermLevelGpa
    {
        /// <summary>
        /// Reference back to the student record necessary when processing several students
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Academic Level code
        /// </summary>
        public string AcademicLevelCode { get; set; }

        /// <summary>
        /// Academic term
        /// </summary>
        public string AcademicTermCode { get; set; }

        /// <summary>
        /// Term GPA
        /// </summary>
        public decimal? TermGpa { get; set; }
    }
}
