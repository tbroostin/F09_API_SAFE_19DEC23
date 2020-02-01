// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes subtotal entity, which is group of gl accounts and list of subtotal components with subtotal amounts
    /// </summary>
    [Serializable]
    public class FinanceQuerySubtotal
    {
        /// <summary>
        /// List of subtotal component, includes totals, subcomponent name and value
        /// </summary>
        public List<FinanceQuerySubtotalComponent> FinanceQuerySubtotalComponents { get; set; }


        /// <summary>
        /// List of gl accounts
        /// </summary>
        public List<FinanceQueryGlAccountLineItem> FinanceQueryGlAccountLineItems { get; set; }
    }
}
