// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Document Approval Response DTO.
    /// </summary>
    public class DocumentApprovalResponse
    {
        /// <summary>
        /// The list of documents that were approved by the user.
        /// </summary>
        public List<ApprovalDocumentResponse> UpdatedApprovalDocumentResponses { get; set; }

        /// <summary>
        /// The list of documents that were not approved by the user.
        /// </summary>
        public List<ApprovalDocumentResponse> NotUpdatedApprovalDocumentResponses { get; set; }
    }
}
