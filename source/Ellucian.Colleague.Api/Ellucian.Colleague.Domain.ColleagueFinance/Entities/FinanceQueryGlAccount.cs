// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes a General Ledger account number for Finance query.
    /// </summary>
    [Serializable]
    public class FinanceQueryGlAccount : GlAccount
    {
        /// <summary>
        /// The GL account budget amount.
        /// </summary>
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// The GL account encumbrance amount. 
        /// </summary>
        public decimal EncumbranceAmount { get; set; }

        /// <summary>
        /// The GL account requisition amount. 
        /// </summary>
        public decimal RequisitionAmount { get; set; }

        /// <summary>
        /// The GL account actual amount.
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// Is the GL account part of a GL Budget pool and in what capacity.
        /// </summary>
        public GlBudgetPoolType PoolType { get { return this.poolType; } }
        private readonly GlBudgetPoolType poolType;

        /// <summary>
        /// The constructor that initializes a GL account number.
        /// </summary>
        /// <param name="glAccount">The GL account</param>        
        public FinanceQueryGlAccount(string glAccount)
            : base(glAccount)
        {
        }

        public FinanceQueryGlAccount(string glAccount, GlBudgetPoolType poolType)
            : base(glAccount)
        {
            this.poolType = poolType;
        }

    }
}