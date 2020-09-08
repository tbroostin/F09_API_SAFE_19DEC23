/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A Cost line item on the shopping sheet.
    /// </summary>
    public class ShoppingSheetCostItem2
    {
        /// <summary>
        /// The budget group category this cost item belongs to on the shopping sheet
        /// </summary>
        public ShoppingSheetBudgetGroup2 BudgetGroup { get; set; }

        /// <summary>
        /// The cost for this item
        /// </summary>
        public int Cost { get; set; }
    }
}
