// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Invoice query input criteria.
    /// </summary>
    public class InvoiceQueryCriteria
    {
        /// <summary>
        /// List of invoice Ids to use as query criteria
        /// </summary>
        public IEnumerable<string> InvoiceIds { get; set; }
    }
}
