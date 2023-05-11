// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    [Serializable]
    public class FinanceQueryActivityDetail : GlAccount
    {
        /// <summary>
        /// Returns the list of activity detail transactions associated to GL account.
        /// </summary>
        public ReadOnlyCollection<GlTransaction> Transactions { get; private set; }
        private readonly List<GlTransaction> transactions = new List<GlTransaction>();

        /// <summary>
        /// Budget pool indicator; Umbrella, Poolee, or null
        /// </summary>
        public string BudgetPoolIndicator { get; set; }

        /// <summary>
        /// The constructor that initializes the GL account number activity.
        /// </summary>
        /// <param name="glAccount">The GL account</param>
        public FinanceQueryActivityDetail(string glAccount) : base(glAccount)
        {
            this.Transactions = transactions.AsReadOnly();
        }

        /// <summary>
        /// Add a transaction to the list of transactions for this general ledger account.
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
