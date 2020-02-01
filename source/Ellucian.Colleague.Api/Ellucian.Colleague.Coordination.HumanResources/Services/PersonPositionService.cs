/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
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
    [RegisterType]
    public class PersonPositionService : BaseCoordinationService, IPersonPositionService
    {
        private readonly IPersonPositionRepository personPositionRepository;
        private readonly ISupervisorsRepository supervisorsRepository;
        private readonly IPositionRepository positionRepository;

        public PersonPositionService(
            IPersonPositionRepository personPositionRepository,
            ISupervisorsRepository supervisorsRepository,
            IPositionRepository positionRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.personPositionRepository = personPositionRepository;
            this.supervisorsRepository = supervisorsRepository;
            this.positionRepository = positionRepository;
        }

        /// <summary>
        /// Get the PersonPositions based on the permissions of the current user/user who has proxy or time history admin
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective personId</param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonPosition>> GetPersonPositionsAsync(string effectivePersonId = null)
        {
            if (effectivePersonId == null)
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            //To view other's info, logged in user must be a proxy or admin
            else if (!CurrentUser.IsPerson(effectivePersonId) && !(HasProxyAccessForPerson(effectivePersonId, Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval)
                       || HasPermission(HumanResourcesPermissionCodes.ViewAllTimeHistory)))
            {
                throw new PermissionsException("User does not have permission to view person position information");
            }

            var userAndSubordinateIds = new List<string>() { effectivePersonId };

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

            var personPositionEntities = await personPositionRepository.GetPersonPositionsAsync(userAndSubordinateIds);
            if (personPositionEntities == null)
            {
                var message = "Unexpected null personPositionEntities returned from repository";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var allPositions = (await positionRepository.GetPositionsAsync()).ToDictionary(p => p.Id);


            var filteredPersonPositions = personPositionEntities
                .Where(p => string.IsNullOrWhiteSpace(p.SupervisorId) &&
                            !string.IsNullOrWhiteSpace(p.PositionId) &&
                            allPositions.ContainsKey(p.PositionId));


            var supervisorPositionIds = filteredPersonPositions
                .Select(pp => allPositions[pp.PositionId].SupervisorPositionId)
                .Where(pid => !string.IsNullOrWhiteSpace(pid));

            if (supervisorPositionIds.Any())
            {
                var supervisors = await supervisorsRepository.GetSupervisorIdsForPositionsAsync(supervisorPositionIds);


                // check if the employee has a PERPOS level supervisor. if not, obtain a list of supervisor IDs from the supervisor position assigned to the employee's position.
                foreach (var personPositionEntity in filteredPersonPositions)
                {
                    var supervisorPositionId = allPositions[personPositionEntity.PositionId].SupervisorPositionId;
                    if (supervisors.ContainsKey(supervisorPositionId))
                    {
                        personPositionEntity.PositionLevelSupervisorIds = supervisors[supervisorPositionId];
                    }
                }
            }

            var personPositionEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PersonPosition, PersonPosition>();

            return personPositionEntities.Select(pp => personPositionEntityToDtoAdapter.MapToType(pp));
        }
    }
}
