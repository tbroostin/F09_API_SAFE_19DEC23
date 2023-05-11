// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes Subotal component entity, which includes subtotal amounts & subtotal component name with value.
    /// </summary>
    [Serializable]
    public class FinanceQuerySubtotalComponent
    {
        /// <summary>
        /// Returns the name of gl component used for subtotal
        /// </summary>
        public string SubtotalComponentName { get; set; }
        /// <summary>
        /// Returns the value of gl component used for subtotal
        /// </summary>
        public string SubtotalComponentValue { get; set; }

        /// <summary>
        /// Returns the description of gl component value used for subtotal
        /// </summary>
        public string SubtotalComponentDescription { get; set; }

        /// <summary>
        /// Returns the total budget amount for all the GL accounts included in this finance query object.
        /// </summary>
        public decimal SubTotalBudget { get { return FinanceQueryGlAccountLineItems.Where(x => x.IsUmbrellaVisible).Select(x => x.GlAccount).Sum(x => x.BudgetAmount); } }

        /// <summary>
        /// Returns the total encumbrance amount for all the GL accounts included in this finance query object.
        /// </summary>
        public decimal SubTotalEncumbrances { get { return FinanceQueryGlAccountLineItems.Where(x => x.IsUmbrellaVisible).Select(x => x.GlAccount).Sum(x => x.EncumbranceAmount); } }

        /// <summary>
        /// Returns the total actual amount for all the GL accounts included in this finance query object.
        /// </summary>
        public decimal SubTotalActuals { get { return FinanceQueryGlAccountLineItems.Where(x => x.IsUmbrellaVisible).Select(x => x.GlAccount).Sum(x => x.ActualAmount); } }

        /// <summary>
        /// Returns the total requisitions amount for all the GL accounts included in this finance query object.
        /// </summary>
        public decimal SubTotalRequisitions { get { return FinanceQueryGlAccountLineItems.Where(x => x.IsUmbrellaVisible).Select(x => x.GlAccount).Sum(x => x.RequisitionAmount); } }

        public List<FinanceQueryGlAccountLineItem> FinanceQueryGlAccountLineItems { get; set; }

        public FinanceQuerySubtotalComponent(string subtotalComponentName, string subtotalComponentValue, List<FinanceQueryGlAccountLineItem> glAccountLineItems)
        {
            this.SubtotalComponentName = subtotalComponentName;
            this.SubtotalComponentValue = subtotalComponentValue;
            this.FinanceQueryGlAccountLineItems = glAccountLineItems;
        }
    }
}
