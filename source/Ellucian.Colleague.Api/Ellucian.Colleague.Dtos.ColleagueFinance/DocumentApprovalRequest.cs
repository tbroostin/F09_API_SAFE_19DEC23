// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Document approval Request DTO.
    /// </summary>
    public class DocumentApprovalRequest
    {
        /// <summary>
        /// The list of documents that the user is trying to approve.
        /// </summary>
        public List<ApprovalDocumentRequest> ApprovalDocumentRequests { get; set; }
    }
}
