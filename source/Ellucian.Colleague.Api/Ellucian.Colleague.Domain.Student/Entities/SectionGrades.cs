// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Represents a student's grades for a particular section
    /// </summary>
    [Serializable]
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
