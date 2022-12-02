// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes a financial "finance query" which is a group of GL Accounts,
    /// each of one contains a group of GL accounts.
    /// </summary>
    [Serializable]
    public class FinanceQuery
    {
        /// <summary>
        /// Returns the total budget amount for all the GL accounts included in this finance query object.
        /// </summary>
        public decimal TotalBudget
        {
            get
            {
                return FinanceQuerySubtotals.Where(x => x != null).SelectMany(s => s.FinanceQueryGlAccountLineItems.Where(x => x.IsUmbrellaVisible).Select(x=>x.GlAccount)).Sum(x => x.BudgetAmount);
            }
        }

        /// <summary>
        /// Returns the total encumbrance amount for all the GL accounts included in this finance query object.
        /// </summary>
        public decimal TotalEncumbrances
        {
            get
            {
                return FinanceQuerySubtotals.Where(x => x != null).SelectMany(s => s.FinanceQueryGlAccountLineItems.Where(x => x.IsUmbrellaVisible).Select(x => x.GlAccount)).Sum(x => x.EncumbranceAmount);
            }
        }

        /// <summary>
        /// Returns the total actual amount for all the GL accounts included in this finance query object.
        /// </summary>
        public decimal TotalActuals
        {
            get
            {
                return FinanceQuerySubtotals.Where(x => x != null).SelectMany(s => s.FinanceQueryGlAccountLineItems.Where(x => x.IsUmbrellaVisible).Select(x => x.GlAccount)).Sum(x => x.ActualAmount);
            }
        }

        /// <summary>
        /// Returns the total requisitions amount for all the GL accounts included in this finance query object.
        /// </summary>
        public decimal TotalRequisitions
        {
            get
            {
                return FinanceQuerySubtotals.Where(x => x != null).SelectMany(s => s.FinanceQueryGlAccountLineItems.Where(x => x.IsUmbrellaVisible).Select(x => x.GlAccount)).Sum(x => x.RequisitionAmount);
            }
        }



        /// <summary>
        /// List of finance query subtotals that make up the finance query.
        /// </summary>
        private List<FinanceQuerySubtotal> financeQuerySubtotals = new List<FinanceQuerySubtotal>();
        public List<FinanceQuerySubtotal> FinanceQuerySubtotals { get { return financeQuerySubtotals; } set { financeQuerySubtotals = value; } }
    }
}
