// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
//using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Coordination Service for CorrespondenceRequests
    /// </summary>
    [RegisterType]
    public class CorrespondenceRequestsService : BaseCoordinationService, ICorrespondenceRequestsService
    {
        private readonly ICorrespondenceRequestsRepository correspondenceRequestsRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="correspondenceRequestRepository">CorrespondenceRequestsRepository object</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public CorrespondenceRequestsService(IAdapterRegistry adapterRegistry,
            ICorrespondenceRequestsRepository correspondenceRequestsRepository,
            IConfigurationRepository configurationRepository,
            IStaffRepository staffRepo,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepo, configurationRepository)
        {
            this.correspondenceRequestsRepository = correspondenceRequestsRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get all of a person's correspondence requests.
        /// </summary>
        /// <param name="personId">The Id of the person for whom to get correspondence requests</param>
        /// <returns>A list of Correspondence Request DTO objects</returns>
        /// <exception cref="ArgumentNullException">Thrown if studentId is null or empty</exception>
        /// <exception cref="PermissionsException">Thrown if Current user is requesting data for someone whom they do not have access</exception>        
        public async Task<IEnumerable<Dtos.Base.CorrespondenceRequest>> GetCorrespondenceRequestsAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!CurrentUser.IsPerson(personId) && !HasProxyAccessForPerson(personId, Domain.Base.Entities.ProxyWorkflowConstants.CoreRequiredDocuments))
            {
                throw new PermissionsException(String.Format("Authenticated user (person ID {0}) does not have permission to view documents for given person ID {1}.", CurrentUser.PersonId, personId));
            }

            var correspondenceRequestEntityList = await correspondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

            var correspondenceRequestDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.CorrespondenceRequest, Dtos.Base.CorrespondenceRequest>();

            var correspondenceRequestDtoList = new List<Dtos.Base.CorrespondenceRequest>();
            if (correspondenceRequestEntityList == null)
            {
                logger.Debug("CorrespondenceRequestsRepository returned null from GetCorrespondenceRequestsAsync(string personId).");
                return correspondenceRequestDtoList;
            }

            foreach (var correspondenceRequestEntity in correspondenceRequestEntityList)
            {
                correspondenceRequestDtoList.Add(correspondenceRequestDtoAdapter.MapToType(correspondenceRequestEntity));
            }

            return correspondenceRequestDtoList;
        }

        /// <summary>
        /// Used to notify back office users when a self-service user has uploaded a new attachment associated with one of their correspondence requests.
        /// If a status code has been specified, the status of the correspondence request will also be changed.
        /// </summary>
        /// <accessComments>
        /// Users may submit attachment notifications for their own correspondence requests.
        /// </accessComments>
        /// <param name="attachmentNotification">Object that contains the person Id, Communication code and optionally the assign date of the correspondence request.</param>
        /// <returns>A CorrespondenceAttachmentNotification</returns>
        public async Task<Dtos.Base.CorrespondenceRequest> AttachmentNotificationAsync(Dtos.Base.CorrespondenceAttachmentNotification attachmentNotification)
        {
            if (attachmentNotification == null)
            {
                throw new ArgumentNullException("attachmentNotification");
            }

            if (string.IsNullOrEmpty(attachmentNotification.PersonId) || string.IsNullOrEmpty(attachmentNotification.CommunicationCode))
            {
                throw new ArgumentException("PersonID and CommunicationCode are required to do an attachment Notification.");
            }

            if (!CurrentUser.IsPerson(attachmentNotification.PersonId))
            {
                throw new PermissionsException(String.Format("Authenticated user (person ID {0}) does not have permission to perform an attachment notification given person ID {1}.", CurrentUser.PersonId, attachmentNotification.PersonId));
            }

            var correspondenceRequestEntity = await correspondenceRequestsRepository.AttachmentNotificationAsync(attachmentNotification.PersonId, attachmentNotification.CommunicationCode, attachmentNotification.AssignDate, attachmentNotification.Instance);


            if (correspondenceRequestEntity == null)
            {
                throw new ApplicationException("Attachment Notification failure.");
            }
            var correspondenceRequestDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.CorrespondenceRequest, Dtos.Base.CorrespondenceRequest>();
            return correspondenceRequestDtoAdapter.MapToType(correspondenceRequestEntity);

        }
    }
}
