// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Total amounts for list of gl accounts included for subtotal component.
    /// </summary>
    public class FinanceQuerySubtotalComponent
    {
        /// <summary>
        /// The total budget amount for subtotal component.
        /// </summary>
        public decimal SubtotalBudget { get; set; }

        /// <summary>
        /// The total actuals amount for subtotal component.
        /// </summary>
        public decimal SubtotalActuals { get; set; }

        /// <summary>
        /// The total encumbrances amount for subtotal component.
        /// </summary>
        public decimal SubtotalEncumbrances { get; set; }

        /// <summary>
        /// The total requisitions amount for subtotal component.
        /// </summary>
        public decimal SubtotalRequisitions { get; set; }

        /// <summary>
        /// Subtotal component name.
        /// </summary>
        public string SubtotalComponentName { get; set; }

        /// <summary>
        /// Subtotal component value.
        /// </summary>
        public string SubtotalComponentValue { get; set; }

        /// <summary>
        /// Subtotal component description.
        /// </summary>
        public string SubtotalComponentDescription { get; set; }

    }
}
