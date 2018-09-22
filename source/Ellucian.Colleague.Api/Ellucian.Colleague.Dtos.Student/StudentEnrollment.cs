//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to pass criteria to and from Student Enrollment.
    /// </summary>
    public class StudentEnrollment
    {
        /// <summary>
        /// Id of student to verify
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Term of Enrollment Record used as part of the key to
        /// find the specific enrollment record.
        /// </summary>
        public string TermId { get; set; }
        /// <summary>
        /// Section Id for Enrollment verification.
        /// </summary>
        public string SectionId { get; set; }
    }
}
