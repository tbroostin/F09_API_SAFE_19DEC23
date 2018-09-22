// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Used to query addresses for specific keys or persons.
    /// </summary>
    public class OrganizationalPersonPositionQueryCriteria
    {
        /// <summary>
        /// Specific OrganizationalPersonPosition Keys to retrieve
        /// </summary>
        public IEnumerable<string> Ids { get; set; }

        /// <summary>
        /// A search string to find person positions based on name or ID.
        /// <remarks>The following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// </summary>
        public string SearchString { get; set; }
    }
}
