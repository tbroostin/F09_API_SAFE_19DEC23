// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A generic receivables transaction
    /// </summary>
    public abstract class ReceivableTransaction
    {
        /// <summary>
        /// ID of the receivables transaction
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the person for whom the transaction was made
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The receivable type of this transaction
        /// </summary>
        public string ReceivableType { get; set; }

        /// <summary>
        /// ID of the term for which the transaction was made
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// Reference number of the transaction
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Date the transaction was made
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Indicates whether the transaction has been archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Location of the transaction
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// External system generating the transaction
        /// </summary>
        public string ExternalSystem { get; set; }

        /// <summary>
        /// The identifier of the transaction in the external system
        /// </summary>
        public string ExternalIdentifier { get; set; }
    }
}
