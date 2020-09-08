// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Approval Document Response DTO.
    /// </summary>
    public class ApprovalDocumentResponse
    {
        /// <summary>
        /// The type of the document that was approved or not updated.
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// The ID of the document that was approved or not updated.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// The number of the document that was approved or not updated.
        /// </summary>
        public string DocumentNumber { get; set; }

        /// <summary>
        /// The status of the document that was approved.
        /// </summary>
        public string DocumentStatus { get; set; }

        /// <summary>
        /// Messages associated with the document that was approved or not updated.
        /// </summary>
        public List<string> DocumentMessages { get; set; }
    }
}
