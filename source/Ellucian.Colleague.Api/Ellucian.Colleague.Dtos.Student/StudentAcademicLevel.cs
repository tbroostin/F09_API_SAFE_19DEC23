// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Academic Level information
    /// </summary>
    public class StudentAcademicLevel
    {
        /// <summary>
        /// Academic Level code
        /// </summary>
        public String AcademicLevel { get; set; }
        /// <summary>
        /// Admit Status
        /// </summary>
        public String AdmitStatus { get; set; }
        /// <summary>
        /// Class level code
        /// </summary>
        public String ClassLevel { get; set; }
        /// <summary>
        /// Starting term
        /// </summary>
        public String StartTerm { get; set; }
        /// <summary>
        /// List of academic credits
        /// </summary>
        public List<String> AcademicCredits { get; set; }
        /// <summary>
        /// IsActive boolean, true if Term provided on input and student acad level
        /// dates intersect with that term.  Set to false if no Term on input
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Student's academic level start date
        /// </summary>
        public DateTime? StudentAcademicLevelStartDate { get;  set; }
        /// <summary>
        /// Student's academic level end date
        /// </summary>
        public DateTime? StudentAcademicLevelEndDate { get;  set; }
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public StudentAcademicLevel()
        {

        }
    }
}
