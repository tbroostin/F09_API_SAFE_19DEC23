// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to pass criteria to the faculty query api.
    /// </summary>
    public class FacultyQueryCriteria
    {
        /// <summary>
        /// Ids of faculty to return from query
        /// </summary>
        public IEnumerable<string> FacultyIds { get; set; }
        /// <summary>
        /// Return only Faculty keys without any Advisor keys
        /// Optional when retrieving keys with SearchFacultyIds, defaults to false
        /// </summary>
        public bool IncludeFacultyOnly { get; set; }
        /// <summary>
        /// Return only Advisor keys without any Faculty keys
        /// Optional when retrieving keys with SearchFacultyIds, defaults to true
        /// </summary>
        public bool IncludeAdvisorOnly { get; set; }
    }
}
