// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
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
    public class SectionGrades
    {
        /// <summary>
        /// Section Id
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Collection of student grades for the section.
        /// </summary>
        public List<StudentGrade> StudentGrades { get; set; }
    }
}
