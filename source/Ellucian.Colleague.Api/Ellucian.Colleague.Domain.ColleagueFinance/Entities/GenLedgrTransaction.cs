// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a transaction posted to the General Ledger through the Data Model.
    /// </summary>
    [Serializable]
    public class GenLedgrTransaction
    {
        /// <summary>
        /// The type of the general ledger transaction (DN = Donation, PL = Pledge, etc.).
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// A source reference number/document number for the transaction.
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// A sequential number associated with the transaction.
        /// </summary>
        public string TransactionNumber { get; set; }

        /// <summary>
        /// The date the transaction is credited/debited to the account in the general ledger (i.e. posting date).
        /// </summary>
        public DateTimeOffset LedgerDate { get; private set; }

        /// <summary>
        /// A supplementary date assigned to the transaction based on the transaction type.
        /// </summary>
        public DateTimeOffset? TransactionTypeReferenceDate { get; set; }

        /// <summary>
        /// A string that identifies a person associated with the transaction.
        /// </summary>
        public string ReferencePersonId { get; set; }

        /// <summary>
        /// The detailed accounting lines associated with the transaction.
        /// </summary>
        public List<GenLedgrTransactionDetail> TransactionDetailLines { get; set; }

        /// <summary>
        /// The budget period associated with the transaction.
        /// </summary>
        public DateTime? BudgetPeriodDate { get; set; }

        /// <summary>
        /// Constructor initializes the General Ledger transaction object.
        /// </summary>

        public GenLedgrTransaction(string source, DateTimeOffset? ledgerDate)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException("source", "The transaction source cannot be null.");
            }
            if (ledgerDate == null)
            {
                throw new ArgumentNullException("ledgerDate", "The ledger date cannot be null.");
            }

            Source = source;
            LedgerDate = ledgerDate.GetValueOrDefault();
        }
    }
}
