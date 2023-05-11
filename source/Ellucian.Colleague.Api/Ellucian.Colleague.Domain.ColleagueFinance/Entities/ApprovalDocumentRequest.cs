// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// An approval document request entity.
    /// </summary>
    [Serializable]
    public class ApprovalDocumentRequest
    {
        /// <summary>
        /// Boolean flag to indicate that the user is approving the document.
        /// </summary>
        public bool Approve { get; set; }

        /// <summary>
        /// Boolean flag to indicate that the user is overriding an over budget condition.
        /// </summary>
        public bool OverrideBudget { get; set; }

        /// <summary>
        /// The type of document that is being approved.
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// The ID of the document that is being approved.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// The number of the document that is being approved.
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// The over budget amount of the document.
        /// </summary>
        public decimal? OverBudgetAmount { get; set; }

        /// <summary>
        /// The ID of the next approver of the document.
        /// </summary>
        public string NextApprover { get; set; }

        /// <summary>
        /// The change date of the document.
        /// </summary>
        public string ChangeDate { get; set; }

        /// <summary>
        /// The change time of the document.
        /// </summary>
        public string ChangeTime { get; set; }

        /// <summary>
        /// Boolean flag to indicate that the user is returning the document.
        /// </summary>
        public bool Return { get; set; }

        /// <summary>
        /// The return comments for the document.
        /// </summary>
        public string ReturnComments { get; set; }

        /// <summary>
        /// The list of line items associated with the document.
        /// </summary>
        public List<ApprovalItem> DocumentItems { get; set; }
    }
}
