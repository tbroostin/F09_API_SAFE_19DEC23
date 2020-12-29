// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Contains a subtotal line item information for a working budget.
    /// </summary>
    public class SubtotalLineItem
    {
        /// <summary>
        /// Contains BO for budget officer or a GL for an account component/subcomponent.
        /// </summary>
        public string SubtotalType { get; set; }

        /// <summary>
        /// The order of the subtotal.
        /// </summary>
        public int SubtotalOrder { get; set; }

        /// <summary>
        /// If a GL type subtotal, the name of the GL component or,
        /// if a BO type "Budget Officer".
        /// </summary>
        public string SubtotalName { get; set; }

        /// <summary>
        /// The value of the GL component/subcomponent or the budget officer ID.
        /// </summary>
        public string SubtotalValue { get; set; }

        /// <summary>
        /// If the subtotal type is BO, the name of the budget officer.
        /// If the subtotal type is GL, the component or subcomponent description.
        /// </summary>
        public string SubtotalDescription { get; set; }

        /// <summary>
        /// Base budget amount for the subtotal line item.
        /// </summary>
        public long SubtotalBaseBudgetAmount { get; set; }

        /// <summary>
        /// Working amount for the subtotal line item.
        /// </summary>
        public long SubtotalWorkingAmount { get; set; }

        /// <summary>
        /// List of subtotal budget comparables amounts for the subtotal line item.
        /// </summary>
        public List<BudgetComparable> SubtotalBudgetComparables { get; set; }
    }
}