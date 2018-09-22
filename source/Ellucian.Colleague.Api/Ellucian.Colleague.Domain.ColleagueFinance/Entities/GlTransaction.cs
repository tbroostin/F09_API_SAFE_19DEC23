// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a transaction posted to the General Ledger.
    /// </summary>
    [Serializable]
    public class GlTransaction
    {
        /// <summary>
        /// System-generated id that is read-only.
        /// </summary>
        private readonly string id;

        /// <summary>
        /// Public getter for the private id.
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Private transaction type for public getter.
        /// </summary>
        private readonly GlTransactionType glTransactionType;

        /// <summary>
        /// The type of transaction.
        /// </summary>
        public GlTransactionType GlTransactionType { get { return glTransactionType; } }

        /// <summary>
        /// Private member for the source of the transaction.
        /// </summary>
        private readonly string source;

        /// <summary>
        /// Public getter for the transaction source.
        /// </summary>
        public string Source { get { return source; } }

        /// <summary>
        /// Private member for the GL account number.
        /// </summary>
        private readonly string glAccount;

        /// <summary>
        /// Public getter for the GL account number.
        /// </summary>
        public string GlAccount { get { return glAccount; } }

        /// <summary>
        /// The resulting amount of this transaction.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Private reference number for public getter.
        /// </summary>
        private readonly string referenceNumber;

        /// <summary>
        /// The reference number for this transaction.
        /// </summary>
        public string ReferenceNumber { get { return referenceNumber; } }

        /// <summary>
        /// The requisition, purchase order or blanket purchase order ID.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// The date for this transaction.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Private description for public getter.
        /// </summary>
        private readonly string description;

        /// <summary>
        /// The description for this transaction.
        /// </summary>
        public string Description { get { return description; } }

        /// <summary>
        /// Constructor initializes the GL transaction object.
        /// </summary>
        /// <param name="id">ID of the transaction.</param>
        /// <param name="type">Budget, Actual, Encumbrance or Requisition.</param>
        /// <param name="source">Source for the transaction.</param>
        /// <param name="glAccount">General ledger account number for the transaction.</param>
        /// <param name="amount">Amount for the transaction.</param>
        /// <param name="referenceNumber">Reference Number for the transaction.</param>
        /// <param name="transactionDate">Date for the transaction.</param>
        /// <param name="description">Description for the transaction.</param>
        /// <exception cref="ArgumentNullException">Thrown if any applicable parameters are null.</exception>
        public GlTransaction(string id, GlTransactionType type, string source, string glAccount, decimal amount, string referenceNumber, DateTime transactionDate, string description)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id", "ID is a required field.");

            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source", "Source is a required field.");

            if (string.IsNullOrEmpty(glAccount))
                throw new ArgumentNullException("glAccount", "GL account number is a required field.");

            if (string.IsNullOrEmpty(referenceNumber))
                throw new ArgumentNullException("referenceNumber", "Reference number is a required field.");

            this.id = id;
            this.glTransactionType = type;
            this.source = source;
            this.glAccount = glAccount;
            this.Amount = amount;
            this.referenceNumber = referenceNumber;
            this.TransactionDate = transactionDate;
            this.description = description;
        }
    }
}
