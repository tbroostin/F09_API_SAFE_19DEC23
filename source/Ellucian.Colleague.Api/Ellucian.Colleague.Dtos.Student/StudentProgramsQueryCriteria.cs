// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Selection Criteria for Student Programs
    /// </summary>
    public class StudentProgramsQueryCriteria
    {
        /// <summary>
        /// List of Student keys to retrieve programs for.
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
        /// <summary>
        /// Restrict programs to a specific term.
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// Flag to determine if inactive programs should be returned.
        /// </summary>
        public bool IncludeInactivePrograms { get; set; }
        /// <summary>
        /// Flag to determine if all programs before the parameter term should be returned.
        /// If IncludeHistory is true, then IncludeInactivePrograms must be false.
        /// </summary>
        public bool IncludeHistory { get; set; }
    }
}
