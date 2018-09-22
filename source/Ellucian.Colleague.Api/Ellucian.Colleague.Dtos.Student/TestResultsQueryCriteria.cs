// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Class to define TestResults API query criteria
    /// </summary>
    public class TestResultsQueryCriteria
    {
        /// <summary>
        /// List of Student Ids to select.
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
        /// <summary>
        /// Specific Test Type to return, admissions, placement, or other.
        /// </summary>
        public string Type { get; set; }
    }
}
