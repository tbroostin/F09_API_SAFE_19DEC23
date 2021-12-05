// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Controls actions for budget adjustments
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class DocumentApprovalController : BaseCompressedApiController
    {
        private IDocumentApprovalService documentApprovalService;
        private readonly ILogger logger;

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="documentApprovalService">Document approval service</param>
        /// <param name="logger">Logger</param>
        public DocumentApprovalController(IDocumentApprovalService documentApprovalService, ILogger logger)
        {
            this.documentApprovalService = documentApprovalService;
            this.logger = logger;
        }

        /// <summary>
        /// Get the document approval for the user.
        /// </summary>
        /// <returns>A document approval DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.DOCUMENT.APPROVAL
        /// </accessComments>
        [HttpGet]
        public async Task<Dtos.ColleagueFinance.DocumentApproval> GetAsync()
        {
            try
            {
                return await documentApprovalService.GetAsync();
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, "Insufficient permissions to get the document approval.");
                throw CreateHttpResponseException("Insufficient permissions to get the document approval.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException argex)
            {
                logger.Error(argex, "Unable to get the document approval.");
                throw CreateHttpResponseException("Unable to get the document approval.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get the document approval.");
                throw CreateHttpResponseException("Unable to get the document approval.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update document approvals.
        /// </summary>
        /// <param name="documentApprovalUpdateRequest">The document approval update request DTO.</param>        
        /// <returns>The document approval update response DTO.</returns>
        /// <accessComments>
        /// Requires permission VIEW.DOCUMENT.APPROVAL.
        /// </accessComments>
        [HttpPost]
        public async Task<Dtos.ColleagueFinance.DocumentApprovalResponse> PostDocumentApprovalAsync([FromBody] Dtos.ColleagueFinance.DocumentApprovalRequest documentApprovalUpdateRequest)
        {
            // The document approval request cannot be null.
            if (documentApprovalUpdateRequest == null)
            {
                throw CreateHttpResponseException("Request body cannot be null.", HttpStatusCode.BadRequest);
            }

            // The list of approval document requests in the document approval request must have objects.
            if (documentApprovalUpdateRequest.ApprovalDocumentRequests == null || !(documentApprovalUpdateRequest.ApprovalDocumentRequests.Any()))
            {
                throw CreateHttpResponseException("Request body must have documents to approve.", HttpStatusCode.BadRequest);
            }

            try
            {
                return await documentApprovalService.UpdateDocumentApprovalRequestAsync(documentApprovalUpdateRequest);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to update document approvals.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument to update a document approval.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to update a document approval.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves documents approved by the user.
        /// </summary>
        /// <param name="filterCriteria">Approved documents filter criteria.</param>
        /// <returns>List of document approved DTOs.</returns>
        /// <accessComments>
        /// Requires permission VIEW.DOCUMENT.APPROVAL.
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<Dtos.ColleagueFinance.ApprovedDocument>> QueryApprovedDocumentsAsync([FromBody] Dtos.ColleagueFinance.ApprovedDocumentFilterCriteria filterCriteria)
        {
            try
            {
                return await documentApprovalService.QueryApprovedDocumentsAsync(filterCriteria);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex, "Insufficient permissions to get the approved documents.");
                throw CreateHttpResponseException("Insufficient permissions to get the approved documents.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException argex)
            {
                logger.Error(argex, "Unable to get approved documents.");
                throw CreateHttpResponseException("Unable to get approved documents.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get approved documents.");
                throw CreateHttpResponseException("Unable to get approved documents.", HttpStatusCode.BadRequest);
            }
        }
    }
}