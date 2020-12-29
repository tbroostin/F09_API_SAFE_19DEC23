// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for the Document Approval service.
    /// </summary>
    public interface IDocumentApprovalService
    {
        /// <summary>
        /// Get the document approval for the user.
        /// </summary>
        /// <returns>A document approval DTO.</returns>
        Task<DocumentApproval> GetAsync();

        /// <summary>
        /// Update document approvals.
        /// </summary>
        /// <param name="documentApprovalUpdateRequest">The document approval update request DTO.</param>        
        /// <returns>The document approval update response DTO.</returns>
        Task<DocumentApprovalResponse> UpdateDocumentApprovalRequestAsync(DocumentApprovalRequest documentApprovalUpdateRequest);

    }
}
