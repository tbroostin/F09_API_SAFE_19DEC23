// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes a General Ledger account number.
    /// </summary>
    [Serializable]
    public class CostCenterGlAccount : GlAccount
    {
        /// <summary>
        /// The GL account budget amount
        /// </summary>
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// Returns the total encumbrance amount for this GL account.
        /// </summary>
        public decimal EncumbranceAmount { get; set; }
    
        /// <summary>
        /// Returns the total actual amount for this GL account.
        /// </summary>
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// Justification Notes for this GL Account.
        /// </summary>
        public string JustificationNotes { get; set; }

        /// <summary>
        /// Returns the list of transactions associated to this line item.
        /// </summary>
        public ReadOnlyCollection<GlTransaction> Transactions { get; private set; }
        private readonly List<GlTransaction> transactions = new List<GlTransaction>();

        /// <summary>
        /// Is the GL account part of a GL Budget pool and in what capacity.
        /// </summary>
        public GlBudgetPoolType PoolType { get { return this.poolType; } }
        private readonly GlBudgetPoolType poolType;

        /// <summary>
        /// The constructor that initializes a GL account number.
        /// </summary>
        /// <param name="glAccount">The GL account</param>
        /// <param name="poolType">Whether the GL account is part of a budget pool and in what capacity.</param>
        public CostCenterGlAccount(string glAccount, GlBudgetPoolType poolType)
            : base(glAccount)
        {
            this.poolType = poolType;
            this.Transactions = transactions.AsReadOnly();
            JustificationNotes = string.Empty;
        }

        /// <summary>
        /// Add a project transaction to the list of transactions for this general ledger account.
        /// The transactions are sorted by descendant date. 
        /// Actuals transactions are not changed.
        /// Encumbrance and requisitions are summarized for each reference number.
        /// If the sum of the amounts is $0.00, they are excluded.
        /// </summary>
        /// <param name="newTransaction">GL transaction</param>
        /// <exception cref="ArgumentNullException">Thrown if the supplied transaction is null.</exception>
        public void AddGlTransaction(GlTransaction newTransaction)
        {
            if (newTransaction == null)
            {
                throw new ArgumentNullException("newTransaction", "GL transaction cannot be null.");
            }

            // First, differentiate between Actuals and Encumbrances.
            // Encumbrance and Requisition transactions are added up into the transaction
            // with the most recent date and later removed if the amount adds up to $0.00.
            if (newTransaction.GlTransactionType == GlTransactionType.Encumbrance || newTransaction.GlTransactionType == GlTransactionType.Requisition)
            {
                var matchingTransaction = transactions.FirstOrDefault(x => !string.IsNullOrEmpty(x.ReferenceNumber) && x.ReferenceNumber == newTransaction.ReferenceNumber
                    && (x.GlTransactionType == GlTransactionType.Encumbrance || x.GlTransactionType == GlTransactionType.Requisition));
                if (matchingTransaction != null)
                {
                    matchingTransaction.Amount += newTransaction.Amount;

                    // Use the latest date for the transaction.
                    if (DateTime.Compare(matchingTransaction.TransactionDate.Date, newTransaction.TransactionDate.Date) < 0)
                        matchingTransaction.TransactionDate = newTransaction.TransactionDate;
                }
                else
                    transactions.Add(newTransaction);
            }
            else
            {
                // For actual transactions just add it to the list. However, we want to make sure it's not a duplicate.
                if (!transactions.Where(x => x.Id == newTransaction.Id).Any())
                    transactions.Add(newTransaction);
            }
        }

        /// <summary>
        /// Remove those encumbrance or requisition transactions whose amount is $0.00
        /// </summary>
        /// <param name="transactions"></param>
        public void RemoveZeroDollarEncumbranceTransactions()
        {
            transactions.RemoveAll(x => (x.GlTransactionType == GlTransactionType.Encumbrance || x.GlTransactionType == GlTransactionType.Requisition) && (x.Amount == 0));
        }
    }
}