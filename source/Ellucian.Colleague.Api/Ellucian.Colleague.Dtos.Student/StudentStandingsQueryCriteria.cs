// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Selection Criteria for Student Standings
    /// </summary>
    public class StudentStandingsQueryCriteria
    {
        /// <summary>
        /// List of Student Standings Keys to retrieve.
        /// </summary>
        public IEnumerable<string> StudentStandingIds { get; set; }
        /// <summary>
        /// List of Student keys to retrieve standings for.
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
        /// <summary>
        /// Restrict standings to a specific term.  (May be appended for standing based on CurrentTerm)
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// Use a current term to determine the most current standing found for a student up through that term.
        /// For example, if current term is 2015/FA, the most current standing found for the student may be in 2015/SP.
        /// </summary>
        public string CurrentTerm { get; set; }
    }
}
