// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// The subtotal entity includes list of gl accounts for .
    /// </summary>
    public class FinanceQuerySubtotal
    {
        /// <summary>
        /// List of subtotal components with subtotal values and subtotal component name, value
        /// </summary>
        public List<FinanceQuerySubtotalComponent> SubtotalComponents { get; set; }

        /// <summary>
        /// List of finance query gl account line items, contains gl accounts and poolees
        /// </summary>
        public List<FinanceQueryGlAccountLineItem> FinanceQueryGlAccountLineItems { get; set; }

    }
}
