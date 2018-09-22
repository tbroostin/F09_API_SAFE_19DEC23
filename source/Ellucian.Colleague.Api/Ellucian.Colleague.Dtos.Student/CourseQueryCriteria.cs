// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Course query input criteria.
    /// </summary>
    public class CourseQueryCriteria
    {
        /// <summary>
        /// List of course Ids to use as query criteria
        /// </summary>
        public IEnumerable<string> CourseIds { get; set; }
    }
}
