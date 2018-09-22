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
    public class StudentTermsQueryCriteria
    {
        /// <summary>
        /// List of Student keys to retrieve student term data for.
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
        /// <summary>
        /// Restrict student terms data to a specific term.
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// Restrict student terms data to a specific academic level.
        /// </summary>
        public string AcademicLevel { get; set; }
    }
}
