// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// A Document approval request entity.
    /// </summary>
    [Serializable]
    public class DocumentApprovalRequest
    {
        /// <summary>
        /// The list of documents that the user is trying to approve.
        /// </summary>
        public List<ApprovalDocumentRequest> ApprovalDocumentRequests { get; set; }
    }
}
