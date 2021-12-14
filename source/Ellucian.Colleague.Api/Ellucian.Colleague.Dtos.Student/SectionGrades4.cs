// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Grade data to be put for a section
    /// </summary>
    public class SectionGrades4
    {
        /// <summary>
        /// Section Id
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Force no verification, regardless of immediate verification settings
        /// </summary>
        public bool? ForceNoVerifyFlag { get; set; }

        /// <summary>
        /// Send grading complete e-mail to faculty when Colleague is configured to do so
        /// </summary>
        public bool? SendGradingCompleteEmail { get; set; }

        /// <summary>
        /// Collection of student grades for the section.
        /// </summary>
        public List<StudentGrade2> StudentGrades { get; set; }
    }
}
