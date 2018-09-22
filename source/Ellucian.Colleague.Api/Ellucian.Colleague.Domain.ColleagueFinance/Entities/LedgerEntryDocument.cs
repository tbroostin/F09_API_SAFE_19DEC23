// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This class represents a generic ledger entry document. This class is intended to be extended
    /// by Journal Entries and Budget Entries.
    /// </summary>
    [Serializable]
    public abstract class LedgerEntryDocument : BaseFinanceDocument
    {
        /// <summary>
        /// Private date entered for public getter
        /// </summary>
        private readonly DateTime enteredDate;

        /// <summary>
        /// The date in which the document was created
        /// </summary>
        public DateTime EnteredDate { get { return enteredDate; } }

        /// <summary>
        /// Private entered by name for public getter
        /// </summary>
        private string enteredByName;

        /// <summary>
        /// The name of the person that added the document
        /// </summary>
        public string EnteredByName { get { return enteredByName; } }

        /// <summary>
        /// Create a new ledger entry document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="date">Document date</param>
        /// <param name="enteredDate">Entered date of document</param>
        /// <param name="enteredByName">Name of person who created the document</param>
        public LedgerEntryDocument(string documentId, DateTime date, DateTime enteredDate, string enteredByName)
            : base(documentId, date)
        {
            this.enteredDate = enteredDate;
            this.enteredByName = enteredByName;
        }
    }
}
