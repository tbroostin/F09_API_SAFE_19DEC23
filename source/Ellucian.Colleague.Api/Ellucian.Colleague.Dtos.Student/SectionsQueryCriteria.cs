// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Selection Criteria for Sections
    /// </summary>
    public class SectionsQueryCriteria
    {
        /// <summary>
        /// List of Student keys to retrieve sections for.
        /// </summary>
        public IEnumerable<string> SectionIds { get; set; }
        /// <summary>
        /// If set to true then find the best term to associate to non-term sections.
        /// </summary>
        public bool BestFit { get; set; }
    }
}
