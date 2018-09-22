// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student grades for a section
    /// </summary>
    public class StudentGrade
    {
        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Midterm grade 1
        /// </summary>
        public string MidtermGrade1 { get; set; }

        /// <summary>
        /// Midterm grade 2
        /// </summary>
        public string MidtermGrade2 { get; set; }

        /// <summary>
        /// Midterm grade 3
        /// </summary>
        public string MidtermGrade3 { get; set; }

        /// <summary>
        /// Midterm grade 4
        /// </summary>
        public string MidtermGrade4 { get; set; }

        /// <summary>
        /// Midterm grade 5
        /// </summary>
        public string MidtermGrade5 { get; set; }

        /// <summary>
        /// Midterm grade 6
        /// </summary>
        public string MidtermGrade6 { get; set; }

        /// <summary>
        /// Final grade
        /// </summary>
        public string FinalGrade { get; set; }

        /// <summary>
        /// Final grade expiration date
        /// </summary>
        public DateTime? FinalGradeExpirationDate { get; set; }

        /// <summary>
        /// Clear final grade expiration date flag
        /// </summary>
        public bool ClearFinalGradeExpirationDateFlag { get; set; }

        /// <summary>
        /// Date of last attendance
        /// </summary>
        public DateTime? LastAttendanceDate { get; set; }

        /// <summary>
        /// Clear date of last attendance flag
        /// </summary>
        public bool ClearLastAttendanceDateFlag { get; set; }

        /// <summary>
        /// Never attended class flag
        /// </summary>
        public bool? NeverAttended { get; set; }
    }
}
