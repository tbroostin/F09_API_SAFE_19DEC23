// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Grades for a section
    /// </summary>
    public class SectionGrades2
    {
        /// <summary>
        /// Section Id
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Collection of student grades for the section.
        /// </summary>
        public List<StudentGrade2> StudentGrades { get; set; }
    }
}
