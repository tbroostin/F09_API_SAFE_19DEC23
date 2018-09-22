// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Linq;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// A service for processing approvals
    /// </summary>
    [RegisterType]
    public class ApprovalService : BaseCoordinationService, IApprovalService
    {
        private IApprovalRepository _approvalRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApprovalService"/> class.
        /// </summary>
        /// <param name="approvalRepository">The approvals repository.</param>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="currentUserFactory">The current user factory.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="logger">The logger.</param>
        public ApprovalService(IApprovalRepository approvalRepository, 
            IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _approvalRepository = approvalRepository;
        }

        /// <summary>
        /// Get an approval document
        /// </summary>
        /// <param name="documentId">Approval document ID</param>
        /// <returns>ApprovalDocument DTO</returns>
        public ApprovalDocument GetApprovalDocument(string documentId)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId");
            }
           
            var documentEntity = _approvalRepository.GetApprovalDocument(documentId);
            if(documentEntity != null && !string.IsNullOrEmpty(documentEntity.PersonId))
            {
                CheckPermissions(documentEntity.PersonId);
            }
            var documentAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.ApprovalDocument, ApprovalDocument>();
            var documentDto = documentAdapter.MapToType(documentEntity);

            return documentDto;
        }

        /// <summary>
        /// Get an approval response
        /// </summary>
        /// <param name="responseId">Approval response ID</param>
        /// <returns>ApprovalResponse DTO</returns>
        public ApprovalResponse GetApprovalResponse(string responseId)
        {
            if (string.IsNullOrEmpty(responseId))
            {
                throw new ArgumentNullException("responseId");
            }

            var responseEntity = _approvalRepository.GetApprovalResponse(responseId);
            if(responseEntity != null && !string.IsNullOrEmpty(responseEntity.PersonId))
            {
                CheckPermissions(responseEntity.PersonId);
            }
            var responseAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.ApprovalResponse, ApprovalResponse>();
            var responseDto = responseAdapter.MapToType(responseEntity);

            return responseDto;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view account holder's information.
        /// </summary>
        /// <param name="studentId">student id for whom we are trying to retrieve the data</param>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckPermissions(string studentId)
        {
            bool hasAdminPermission = HasPermission(BasePermissionCodes.ViewStudentAccountActivity);

            if (!CurrentUser.IsPerson(studentId) && !HasProxyAccessForPerson(studentId) && !hasAdminPermission)
            {
                logger.Info(CurrentUser + " does not have permission code " + BasePermissionCodes.ViewStudentAccountActivity);
                throw new PermissionsException();
            }
        }
    }
}
