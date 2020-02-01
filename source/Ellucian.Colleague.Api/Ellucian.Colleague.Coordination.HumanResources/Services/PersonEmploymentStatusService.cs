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
    public class PersonEmploymentStatusService : BaseCoordinationService, IPersonEmploymentStatusService
    {
        private readonly IPersonEmploymentStatusRepository personEmploymentStatusRepository;
        private readonly ISupervisorsRepository supervisorsRepository;

        public PersonEmploymentStatusService(
            IPersonEmploymentStatusRepository personEmploymentStatusRepository,
            ISupervisorsRepository supervisorsRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.personEmploymentStatusRepository = personEmploymentStatusRepository;
            this.supervisorsRepository = supervisorsRepository;
        }


        /// <summary>
        /// Get the PersonStatuses based on the permissions of the current user/user who has proxy or time history admin 
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective personId</param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonEmploymentStatus>> GetPersonEmploymentStatusesAsync(string effectivePersonId = null)
        {
            if (effectivePersonId == null)
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            //To view other's info, logged in user must be a proxy or admin
            else if (!CurrentUser.IsPerson(effectivePersonId) && !(HasProxyAccessForPerson(effectivePersonId, Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval)
                       || HasPermission(HumanResourcesPermissionCodes.ViewAllTimeHistory)))
            {
                throw new PermissionsException("User does not have permission to view person employment status information");
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

            var personEmploymentStatusEntities = await personEmploymentStatusRepository.GetPersonEmploymentStatusesAsync(userAndSubordinateIds);

            if (personEmploymentStatusEntities == null)
            {
                var message = "Unexpected null personStatusEntities returned from repository";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var personEmploymentStatusEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PersonEmploymentStatus, PersonEmploymentStatus>();

            return personEmploymentStatusEntities.Select(perstat => personEmploymentStatusEntityToDtoAdapter.MapToType(perstat));
        }
    }
}
