using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// A Cost line item on the shopping sheet.
    /// </summary>
    [Serializable]
    public class ShoppingSheetCostItem
    {
        /// <summary>
        /// The budget group category this cost item belongs to on the shopping sheet
        /// </summary>
        public ShoppingSheetBudgetGroup BudgetGroup { get { return budgetGroup; } }
        private readonly ShoppingSheetBudgetGroup budgetGroup;

        /// <summary>
        /// The cost for this item
        /// </summary>
        public int Cost { get { return cost; } }
        private readonly int cost;

        public ShoppingSheetCostItem(ShoppingSheetBudgetGroup budgetGroup, int cost)
        {
            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException("cost cannot be less than zero");
            }

            this.cost = cost;
            this.budgetGroup = budgetGroup;
        }
    }
}
