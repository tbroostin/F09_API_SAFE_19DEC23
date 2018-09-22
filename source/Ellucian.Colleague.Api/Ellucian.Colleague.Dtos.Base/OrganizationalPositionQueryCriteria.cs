// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Organizational Position Query Criteria
    /// </summary>
    public class OrganizationalPositionQueryCriteria
    {
        /// <summary>
        /// Organizational Positions IDs to retrieve
        /// </summary>
        public IEnumerable<string> Ids { get; set; }
        /// <summary>
        /// Search string to find positions by title or ID
        /// </summary>
        public string SearchString { get; set; }
    }
}
