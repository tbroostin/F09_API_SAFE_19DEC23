// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// The Finance Query Gl transaction DTO.
    /// </summary>
    public class FinanceQueryGlTransaction
    {
        /// <summary>
        /// The reference number for this transaction.
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// The requisition, purchase order or blanket purchase order ID.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// The source for this transaction.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Transaction type, either Actuals, Budget, Encumbrances, or Requisitions
        /// </summary>
        public string TransactionType { get; set;}

        /// <summary>
        /// The date for this transaction.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// The description for this transaction.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The amount for this transaction.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
