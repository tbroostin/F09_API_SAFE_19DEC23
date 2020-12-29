﻿/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Coordination service for PersonPositionWage endpoints
    /// </summary>
    [RegisterType]
    public class PersonPositionWageService : BaseCoordinationService, IPersonPositionWageService
    {
        private readonly IPersonPositionWageRepository personPositionWageRepository;
        private readonly ISupervisorsRepository supervisorsRepository;
        private readonly IHumanResourcesReferenceDataRepository referenceDataRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="personPositionWageRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public PersonPositionWageService(
            IPersonPositionWageRepository personPositionWageRepository,
            ISupervisorsRepository supervisorsRepository,
            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.personPositionWageRepository = personPositionWageRepository;
            this.supervisorsRepository = supervisorsRepository;
            this.referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Gets the PersonPositionWages based on the permissions of the current user/proxy of the current user.
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective personId</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns>A collection of PersonPositionWage objects</returns>
        public async Task<IEnumerable<PersonPositionWage>> GetPersonPositionWagesAsync(string effectivePersonId = null, DateTime? lookupStartDate = null)
        {
            if (effectivePersonId == null)
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            // To view other's info, logged in user must be a proxy or time history admin or leave approver
            else if (!CurrentUser.IsPerson(effectivePersonId) && !(HasProxyAccessForPerson(effectivePersonId, Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval)
                       || HasPermission(HumanResourcesPermissionCodes.ViewAllTimeHistory) || HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest)))
            {
                throw new PermissionsException("User does not have permission to view person position wage information");
            }
            var userAndSubordinateIds = new List<string>() { effectivePersonId };

            // Supervisees for time card approver
            if (HasPermission(HumanResourcesPermissionCodes.ViewSuperviseeData))
            {
                var subordinateIds = (await supervisorsRepository.GetSuperviseesBySupervisorAsync(effectivePersonId)).ToList();

                if (subordinateIds == null)
                {
                    var message = "Unexpected null person id list returned from supervisors repository";
                    logger.Error(message);
                    throw new ApplicationException(message);
                }
                if (subordinateIds.Any())
                {
                    userAndSubordinateIds = userAndSubordinateIds.Concat(subordinateIds).ToList();
                }
            }
            // Supervisees for leave approver
            if (HasPermission(HumanResourcesPermissionCodes.ApproveRejectLeaveRequest))
            {
                var subordinateIds = (await supervisorsRepository.GetSuperviseesByPrimaryPositionForSupervisorAsync(effectivePersonId)).ToList();

                if (subordinateIds == null)
                {
                    var message = "Unexpected null person id list returned from supervisors repository";
                    logger.Error(message);
                    throw new ApplicationException(message);
                }
                if (subordinateIds.Any())
                {
                    userAndSubordinateIds = userAndSubordinateIds.Concat(subordinateIds).ToList();
                }
            }

            var personPositionWageEntities = await personPositionWageRepository.GetPersonPositionWagesAsync(userAndSubordinateIds.Distinct(), lookupStartDate);

            if (personPositionWageEntities == null)
            {
                var message = "Unexpected null personPositionWageEntities returned from repository";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var personPositionWageEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PersonPositionWage, PersonPositionWage>();

            var personPositionWageDtos = personPositionWageEntities.Select(ppw => personPositionWageEntityToDtoAdapter.MapToType(ppw));

            return personPositionWageDtos;
        }
    }
}
