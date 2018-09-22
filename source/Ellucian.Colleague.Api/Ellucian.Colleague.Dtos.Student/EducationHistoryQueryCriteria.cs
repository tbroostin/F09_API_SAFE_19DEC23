// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Class to define query selection criteria on Education History API
    /// </summary>
    public class EducationHistoryQueryCriteria
    {
        /// <summary>
        /// List of Students to select Education History for.
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
    }
}
