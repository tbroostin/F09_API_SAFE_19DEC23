/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
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
    public class ShoppingSheetCostItem2
    {
        /// <summary>
        /// The budget group category this cost item belongs to on the shopping sheet
        /// </summary>
        public ShoppingSheetBudgetGroup2 BudgetGroup { get { return budgetGroup; } }
        private readonly ShoppingSheetBudgetGroup2 budgetGroup;

        /// <summary>
        /// The cost for this item
        /// </summary>
        public int Cost { get { return cost; } }
        private readonly int cost;

        public ShoppingSheetCostItem2(ShoppingSheetBudgetGroup2 budgetGroup, int cost)
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
