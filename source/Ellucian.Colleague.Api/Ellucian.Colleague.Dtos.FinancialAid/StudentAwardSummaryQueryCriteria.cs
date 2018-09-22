//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Used to pass criteria to the StudentAwardSummary query api.
    /// </summary>
    public class StudentAwardSummaryQueryCriteria
    {
        /// <summary>
        /// Student Ids to use to select Student Awards Data
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
        /// <summary>
        /// Award Year used in Selection
        /// </summary>
        public string AwardYear { get; set; }
        /// <summary>
        /// Term used in Selection
        /// </summary>
        public string Term { get; set; }
    }
}
