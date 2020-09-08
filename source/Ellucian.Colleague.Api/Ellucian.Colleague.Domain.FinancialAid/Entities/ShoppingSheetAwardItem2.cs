/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// An Scholarship And Grant, WorkStudy or Loan line item on the shopping sheet
    /// </summary>
    [Serializable]
    public class ShoppingSheetAwardItem2
    {
        /// <summary>
        /// The award group category this item belongs to on the shopping sheet
        /// </summary>
        public ShoppingSheetAwardGroup2 AwardGroup { get { return awardGroup; } }
        private readonly ShoppingSheetAwardGroup2 awardGroup;

        /// <summary>
        /// The amount for this item
        /// </summary>
        public int Amount { get { return amount; } }
        private readonly int amount;

        public ShoppingSheetAwardItem2(ShoppingSheetAwardGroup2 awardGroup, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("amount cannot be less than zero");
            }

            this.awardGroup = awardGroup;
            this.amount = amount;
        }
    }
}
