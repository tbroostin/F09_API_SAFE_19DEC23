/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Used to create a query for Person names
    /// </summary>
    public class PersonNameQueryCriteria
    {
        /// <summary>
        /// Person Ids to retrieve
        /// </summary>
        public IEnumerable<string> Ids { get; set; }

        /// <summary>
        /// Search term of an id or partial name.
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// </summary>
        public string QueryKeyword { get; set; }

    }
}
