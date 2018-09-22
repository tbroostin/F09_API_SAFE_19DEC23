//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to pass criteria to the Grades query api.
    /// </summary>
    public class GradeQueryCriteria
    {
        /// <summary>
        /// Ids of students to return from query
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
        /// <summary>
        /// Set the term value to return only this specific term data.
        /// </summary>
        public string Term { get; set; }
    }
}
