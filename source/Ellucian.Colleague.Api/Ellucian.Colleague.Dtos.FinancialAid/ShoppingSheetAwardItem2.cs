/*Copyright 2015-2020 Ellucian Company L.P. and its affiliates*/
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// An Scholarship And Grant, WorkStudy or Loan line item on the shopping sheet
    /// </summary>
    public class ShoppingSheetAwardItem2
    {
        /// <summary>
        /// The award group category this item belongs to on the shopping sheet
        /// </summary>
        public ShoppingSheetAwardGroup2 AwardGroup { get; set; }

        /// <summary>
        /// The amount for this item
        /// </summary>
        public int Amount { get; set; }
    }
}
