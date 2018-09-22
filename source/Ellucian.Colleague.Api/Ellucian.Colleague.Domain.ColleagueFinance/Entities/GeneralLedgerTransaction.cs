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
    public class GeneralLedgerTransaction
    {
        /// <summary>
        /// Unique identifier (GUID) for transaciton.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Either Update or Verify for creation of the transaction in Colleague
        /// </summary>
        public string ProcessMode { get; set; }

        /// <summary>
        /// ID of the person submitting the GL Transaction
        /// </summary>
        public string SubmittedBy { get; set; }

        /// <summary>
        /// The comment associated with the transaction.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// A list of associated general ledger transactions.
        /// </summary>
        public List<GenLedgrTransaction> GeneralLedgerTransactions { get; set; }

        /// <summary>
        /// Constructor initializes the General Ledger transaction object.
        /// </summary>
        
        public GeneralLedgerTransaction()
        {
            GeneralLedgerTransactions = new List<GenLedgrTransaction>();
        }
    }
}
