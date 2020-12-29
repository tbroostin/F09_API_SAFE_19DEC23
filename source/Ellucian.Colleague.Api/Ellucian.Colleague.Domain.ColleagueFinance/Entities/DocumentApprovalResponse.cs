// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// A document approval response entity.
    /// </summary>
    [Serializable]
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
