// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// DTO for incoming JSON formatted student planning query criteria.
    /// </summary>
    public class PlanningStudentCriteria
    {
        /// <summary>
        /// List of Student Ids for retrieval of multiple records.
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
    }
}
