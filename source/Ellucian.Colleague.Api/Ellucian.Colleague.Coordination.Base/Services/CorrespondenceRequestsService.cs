// Copyright 2018 Ellucian Company L.P. and its affiliates.
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
        /// <exception cref="PermissionsException">Thrown if Current user is requesting data for a student other than self</exception>        
        public async Task<IEnumerable<Dtos.Base.CorrespondenceRequest>> GetCorrespondenceRequestsAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!CurrentUser.IsPerson(personId))
            {
                throw new PermissionsException(String.Format("Authenticated user (person ID {0}) does not match passed person ID {1}.", CurrentUser.PersonId, personId));
            }

            var correspondenceRequestEntityList = await correspondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);

            var correspondenceRequestDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.CorrespondenceRequest, Dtos.Base.CorrespondenceRequest>();

            var correspondenceRequestDtoList = new List<Dtos.Base.CorrespondenceRequest>();
            if (correspondenceRequestEntityList == null)
            {
                logger.Info("StudentDocumentRepository returned null from Get(string studentId).");
                return correspondenceRequestDtoList;
            }

            foreach (var correspondenceRequestEntity in correspondenceRequestEntityList)
            {
                correspondenceRequestDtoList.Add(correspondenceRequestDtoAdapter.MapToType(correspondenceRequestEntity));
            }

            return correspondenceRequestDtoList;
        }
    }
}
