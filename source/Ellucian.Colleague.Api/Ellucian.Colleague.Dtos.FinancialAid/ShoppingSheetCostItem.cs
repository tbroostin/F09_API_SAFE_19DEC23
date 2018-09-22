using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A Cost line item on the shopping sheet.
    /// </summary>
    public class ShoppingSheetCostItem
    {
        /// <summary>
        /// The budget group category this cost item belongs to on the shopping sheet
        /// </summary>
        public ShoppingSheetBudgetGroup BudgetGroup { get; set; }

        /// <summary>
        /// The cost for this item
        /// </summary>
        public int Cost { get; set; }
    }
}
