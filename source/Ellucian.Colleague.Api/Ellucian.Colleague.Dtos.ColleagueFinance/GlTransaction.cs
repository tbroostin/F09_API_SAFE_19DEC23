// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is the transaction posted to the General Ledger
    /// </summary>
    public class GlTransaction
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
