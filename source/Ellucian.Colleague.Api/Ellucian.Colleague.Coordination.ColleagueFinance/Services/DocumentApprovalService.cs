// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Service for document approvals.
    /// </summary>
    [RegisterType]
    public class DocumentApprovalService : BaseCoordinationService, IDocumentApprovalService
    {

        private readonly IDocumentApprovalRepository documentApprovalRepository;
        private readonly IStaffRepository staffRepository;
        public DocumentApproval adjustmentOutputDto;

        /// <summary>
        /// Initialize the service.
        /// </summary>
        /// <param name="documentApprovalRepository">Document approval repository</param>
        /// <param name="staffRepository">Staff repository</param>
        /// <param name="adapterRegistry">Adapter registry</param>
        /// <param name="currentUserFactory">User factory</param>
        /// <param name="roleRepository">Role repository</param>
        /// <param name="logger">Logger object</param>
        public DocumentApprovalService(IDocumentApprovalRepository documentApprovalRepository,
            IStaffRepository staffRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.documentApprovalRepository = documentApprovalRepository;
            this.staffRepository = staffRepository;
        }

        /// <summary>
        /// Get the document approval for the user.
        /// </summary>
        /// <returns>Document Approval DTO</returns>
        public async Task<DocumentApproval> GetAsync()
        {
            // Check the permission code to view the document approval.
            CheckViewDocumentApprovalPermission();


            // Get the approver/login id for the current user.
            var currentUserStaffLoginId = string.Empty;
            try
            {
                currentUserStaffLoginId = await staffRepository.GetStaffLoginIdForPersonAsync(CurrentUser.PersonId);
            }
            catch (Exception e)
            {
                logger.Error(e, "Could not locate Staff Login ID for person ID: " + CurrentUser.PersonId);
                throw new PermissionsException("Could not find Staff information for the user.");
            }

            var documentApprovalEntity = await documentApprovalRepository.GetAsync(currentUserStaffLoginId);

            // Initialize the document approval dto.
            var documentApprovalDto = new DocumentApproval();

            // Update the dto with the information from the domain entity.
            if (documentApprovalEntity != null)
            {
                // Convert the domain entity into a DTO.
                var adapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.DocumentApproval, DocumentApproval>();
                documentApprovalDto = adapter.MapToType(documentApprovalEntity);
            }

            return documentApprovalDto;
        }

        /// <summary>
        /// Update document approvals.
        /// </summary>
        /// <param name="documentApprovalUpdateRequest">The document approval update request DTO.</param>        
        /// <returns>The requisition create update response DTO.</returns>
        public async Task<DocumentApprovalResponse> UpdateDocumentApprovalRequestAsync(DocumentApprovalRequest documentApprovalUpdateRequest)
        {
            // Instantiate the return object.
            DocumentApprovalResponse response = new DocumentApprovalResponse();

            // The document approval request cannot be null.
            if (documentApprovalUpdateRequest == null)
            {
                throw new ArgumentNullException("documentApprovalUpdateRequest", "The documentApprovalUpdateRequest cannot be null.");
            }

            // The document approval request must contains approval document requests.
            if (documentApprovalUpdateRequest.ApprovalDocumentRequests == null || !(documentApprovalUpdateRequest.ApprovalDocumentRequests.Any()))
            {
                throw new ArgumentNullException("ApprovalDocumentRequests", "There must be documents pending approval.");
            }

            // Check the permission code to view/update the document approval.         
            CheckViewDocumentApprovalPermission();

            // Get the approver/login id for the current user.
            var currentUserStaffLoginId = string.Empty;
            try
            {
                currentUserStaffLoginId = await staffRepository.GetStaffLoginIdForPersonAsync(CurrentUser.PersonId);
            }
            catch (Exception e)
            {
                logger.Error(e, "Could not locate Staff Login ID for person ID: " + CurrentUser.PersonId);
                throw new PermissionsException("Could not find Staff information for the user.");
            }

            //Convert DTO to domain entity            
            var documentApprovalUpdateRequestEntity = ConvertDocumentApprovalUpdateRequestDtoToEntity(documentApprovalUpdateRequest);

            // call the repository method that calls the CTX to process the approval documents.
            var documentApprovalResponseEntity = await documentApprovalRepository.UpdateDocumentApprovalAsync(currentUserStaffLoginId, documentApprovalUpdateRequestEntity.ApprovalDocumentRequests);

            // Define the adapter and if there is an response entity returned from the repository, convert the entity to a DTO.
            var updateResponseAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.DocumentApprovalResponse, DocumentApprovalResponse>();
            if (documentApprovalResponseEntity != null)
            {
                response = updateResponseAdapter.MapToType(documentApprovalResponseEntity);
            }

            return response;
        }

        #region Private methods
        /// <summary>
        /// Permission code that allows a READ/UPDATE operation on a document approval.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewDocumentApprovalPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewDocumentApproval);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view document approvals.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Convert a document approval update request DTO to an entity.
        /// </summary>
        /// <param name="documentApprovalUpdateRequest">A document approval request DTO.</param>
        /// <returns>A document approval response entity.</returns>
        private Domain.ColleagueFinance.Entities.DocumentApprovalRequest ConvertDocumentApprovalUpdateRequestDtoToEntity(DocumentApprovalRequest documentApprovalUpdateRequest)
        {
            Domain.ColleagueFinance.Entities.DocumentApprovalRequest documentApprovalRequestEntity = new Domain.ColleagueFinance.Entities.DocumentApprovalRequest();


            // Define the adapter and if there is an response entity returned from the repository, convert the entity to a DTO.
            var updateEntityAdapter = _adapterRegistry.GetAdapter<DocumentApprovalRequest, Domain.ColleagueFinance.Entities.DocumentApprovalRequest>();
            if (documentApprovalUpdateRequest != null)
            {
                documentApprovalRequestEntity = updateEntityAdapter.MapToType(documentApprovalUpdateRequest);
            }
            return documentApprovalRequestEntity;
        }
        #endregion
    }
}