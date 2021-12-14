// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Defines methods necessary to read/write document approvals.
    /// </summary>
    public interface IDocumentApprovalRepository
    {
        /// <summary>
        /// Get the document approval for the user.
        /// </summary>
        /// <param name="staffLoginId">Staff login Id.</param>
        /// <returns>A document approval domain entity.</returns>
        Task<DocumentApproval> GetAsync(string staffLoginId);

        /// <summary>
        /// Process a document approval request.
        /// </summary>
        /// <param name="staffLoginId">Staff login Id.</param>
        /// <param name="approvalDocumentRequests">List of approval document requests.</param>
        /// <returns>A document approval response.</returns>
        Task<DocumentApprovalResponse> UpdateDocumentApprovalAsync(string staffLoginId, List<ApprovalDocumentRequest> approvalDocumentRequests);

        /// <summary>
        /// Retrieve documents approved by the user.
        /// </summary>
        /// <param name="staffLoginId">Staff login Id.</param>
        /// <param name="filterCriteria">Approved documents filter criteria.</param>
        /// <returns>List of document approved DTOs</returns>
        Task<IEnumerable<ApprovedDocument>> QueryApprovedDocumentsAsync(string staffLoginId, ApprovedDocumentFilterCriteria filterCriteria);
    }
}