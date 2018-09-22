// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Selection Criteria for Student Affiliations
    /// </summary>
    public class StudentAffiliationQueryCriteria
    {
        /// <summary>
        /// List of Student keys to retrieve student affiliation data for.
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
        /// <summary>
        /// Restrict student affiliation data to a specific term.
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// Restrict student affiliation data to a specific affiliation code.
        /// </summary>
        public string AffiliationCode { get; set; }
    }
}
