// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Utility class to build finance query line items
    /// </summary>
    [Serializable]
    public class FinanceQueryGlAccountLineItemBuilder
    {
        /// <summary>
        /// List of gl accounts that makes to filtered result of finance query report.
        /// </summary>
        public ReadOnlyCollection<FinanceQueryGlAccountLineItem> FinanceQueryGlAccountLineItems { get; set; }

        private readonly List<FinanceQueryGlAccountLineItem> financeQueryGlAccountLineItems = new List<FinanceQueryGlAccountLineItem>();

        public FinanceQueryGlAccountLineItemBuilder()
        {
            this.FinanceQueryGlAccountLineItems = financeQueryGlAccountLineItems.AsReadOnly();
        }

        /// <summary>
        /// Method to add a poolee gl account to under respective umbrella gl account.
        /// </summary>
        /// <param name="glAccount">gl account number.</param>
        /// <param name="pooleeGlAccount">poolee gl account.</param>
        public void AddPoolee(string glAccount, FinanceQueryGlAccount pooleeGlAccount)
        {
            if (string.IsNullOrEmpty(glAccount))
            {
                throw new ArgumentNullException("glAccount", "The GL account number cannot be null or an empty string.");
            }

            if (pooleeGlAccount == null)
            {
                throw new ArgumentNullException("pooleeGlAccount", "poolee gl account cannot be null.");
            }

            var summary = financeQueryGlAccountLineItems.FirstOrDefault(x => x.GlAccountNumber == glAccount);

            if (summary != null && !string.IsNullOrEmpty(summary.GlAccountNumber))
            {
                summary.Poolees.Add(pooleeGlAccount);
            }
        }

        /// <summary>
        /// Method to add a GL account to the list of GL accounts that make up this GL object code.
        /// </summary>
        /// <param name="glAccount">gl account.</param>
        public void AddNonPooledGlAccount(FinanceQueryGlAccount glAccount)
        {
            if (glAccount == null)
            {
                throw new ArgumentNullException("glAccount", "The GL account cannot be null or an empty string.");
            }

            if (glAccount.PoolType != GlBudgetPoolType.None)
            {
                throw new ApplicationException("Only non-pooled account can be added.");
            }

            var summary = financeQueryGlAccountLineItems.FirstOrDefault(x => x.GlAccountNumber == glAccount.GlAccountNumber);

            if (summary != null && !string.IsNullOrEmpty(summary.GlAccountNumber)) return;
            summary = new FinanceQueryGlAccountLineItem(glAccount, false, true);
            this.financeQueryGlAccountLineItems.Add(summary);
        }

        /// <summary>
        /// Method to add a GL account to the list of GL accounts that make up this GL object code.
        /// </summary>
        /// <param name="glAccount">gl account.</param>
        /// <param name="isUmbrellaVisible">whether umbrella account has been given access to the user </param>
        public void AddBudgetPoolGlAccount(FinanceQueryGlAccount glAccount, bool isUmbrellaVisible)
        {
            if (glAccount == null)
            {
                throw new ArgumentNullException("glAccount", "The GL account cannot be null or an empty string.");
            }

            if (glAccount.PoolType != GlBudgetPoolType.Umbrella)
            {
                throw new ApplicationException("Only umbrella gl account can be added.");
            }

            var summary = financeQueryGlAccountLineItems.FirstOrDefault(x => x.GlAccountNumber == glAccount.GlAccountNumber);

            if (summary == null || string.IsNullOrEmpty(summary.GlAccountNumber))
            {
                summary = new FinanceQueryGlAccountLineItem(glAccount, true, isUmbrellaVisible);
                this.financeQueryGlAccountLineItems.Add(summary);
            }
        }
    }
}
