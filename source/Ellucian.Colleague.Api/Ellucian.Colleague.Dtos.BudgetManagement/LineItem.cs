// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Contains either a Budget line item or a subtotal but not both.
    /// </summary>
    public class LineItem
    {
        /// <summary>
        /// The sequence number for this line item within the working budget.
        /// It goes from 1 to the number of returned items which will be the 
        /// number of requested budget line items for the page plus, if the user
        /// requested any subtotals, any number of subtotals to be displayed on the page. 
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// A budget line item for the working budget.
        /// </summary>
        public BudgetLineItem BudgetLineItem { get; set; }

        /// <summary>
        /// A subtotal line item for the working budget.
        /// </summary>
        public SubtotalLineItem SubtotalLineItem { get; set; }
    }
}