// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for approvals
    /// </summary>
    public interface IApprovalRepository
    {
        /// <summary>
        /// Get an approval document
        /// </summary>
        /// <param name="id">Approval document ID</param>
        /// <returns>Approval document</returns>
        ApprovalDocument GetApprovalDocument(string id);

        /// <summary>
        /// Create an approval document
        /// </summary>
        /// <param name="approvalDoc">Approval document to create</param>
        /// <returns>Updated approval document</returns>
        ApprovalDocument CreateApprovalDocument(ApprovalDocument approvalDoc);

        /// <summary>
        /// Get an approval response
        /// </summary>
        /// <param name="id">Approval response ID</param>
        /// <returns>Approval response</returns>
        ApprovalResponse GetApprovalResponse(string id);

        /// <summary>
        /// Create an approval response
        /// </summary>
        /// <param name="approvalResp">Approval response to create</param>
        /// <returns>Updated approval response</returns>
        ApprovalResponse CreateApprovalResponse(ApprovalResponse approvalResp);
    }
}
