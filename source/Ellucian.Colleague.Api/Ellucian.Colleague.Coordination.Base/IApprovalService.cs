// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Base
{
    public interface IApprovalService
    {
        /// <summary>
        /// Get an approval document
        /// </summary>
        /// <param name="documentId">Approval document ID</param>
        /// <returns>ApprovalDocument DTO</returns>
        ApprovalDocument GetApprovalDocument(string documentId);

        /// <summary>
        /// Get an approval response
        /// </summary>
        /// <param name="responseId">Approval response ID</param>
        /// <returns>ApprovalResponse DTO</returns>
        ApprovalResponse GetApprovalResponse(string responseId);
    }
}
