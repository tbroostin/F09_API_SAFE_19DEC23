// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Contains the comparable information for a budget line item for a working budget.
    /// </summary>
    public class BudgetComparable
    {
        /// <summary>
        /// The budget comparable number.
        /// </summary>
        public string ComparableNumber { get; set; }

        /// <summary>
        /// The comparable amount.
        /// </summary>
        public long? ComparableAmount { get; set; }
    }
}