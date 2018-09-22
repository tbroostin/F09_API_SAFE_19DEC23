//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to pass criteria to the Academic History query api.
    /// </summary>
    public class AcademicHistoryQueryCriteria
    {
        /// <summary>
        /// Ids of students to return from query
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
        /// <summary>
        /// If set to true, then non-term courses will be grouped into the best term.
        /// </summary>
        public bool BestFit { get; set; }
        /// <summary>
        /// If set to true then only active course work will be returned.
        /// </summary>
        public bool Filter { get; set; }
        /// <summary>
        /// Set the term value to return only this specific term data.
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// If set to true, the academic credits should be returned as student sections.
        /// </summary>
        public bool IncludeStudentSections { get; set; }
    }
}
