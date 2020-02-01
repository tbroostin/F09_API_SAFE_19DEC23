// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Query request for Advisors
    /// </summary>
    public class AdvisorQueryCriteria
    {
        /// <summary>
        /// List of Advisor Ids to retrieve.
        /// </summary>
        public IEnumerable<string> AdvisorIds { get; set; }
        
        /// <summary>
        /// If true, only return the IDs of currently active advisees
        /// </summary>
        public bool OnlyActiveAdvisees { get; set; }
    }
}
