/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
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
    /// Coordination service for PersonStipend endpoint
    /// </summary>
    [RegisterType]
    public class PersonStipendService : BaseCoordinationService, IPersonStipendService
    {
        private readonly IPersonStipendRepository personStipendRepository;
        private readonly ISupervisorsRepository supervisorsRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="personStipendRepository"></param>
        /// <param name="supervisorsRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public PersonStipendService(
            IPersonStipendRepository personStipendRepository,
            ISupervisorsRepository supervisorsRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.personStipendRepository = personStipendRepository;
            this.supervisorsRepository = supervisorsRepository;
        }

        /// <summary>
        /// Get the PersonStipend records based on the permissions of the current user/user who has proxy or time history admin (with the permission VIEW.ALL.TIME.HISTORY)
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective personId</param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonStipend>> GetPersonStipendAsync(string effectivePersonId = null)
        {
            if (effectivePersonId == null)
            {
                effectivePersonId = CurrentUser.PersonId;
            }
            //To view other's info, logged in user must be a proxy or an admin
            else if (!CurrentUser.IsPerson(effectivePersonId) && !(HasProxyAccessForPerson(effectivePersonId, Domain.Base.Entities.ProxyWorkflowConstants.TimeManagementTimeApproval)
                       || HasPermission(HumanResourcesPermissionCodes.ViewAllTimeHistory)))
            {
                throw new PermissionsException("User does not have permission to view person stipend information");
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

            var personStipendEntities = await personStipendRepository.GetPersonStipendAsync(userAndSubordinateIds);

            if (personStipendEntities == null)
            {
                var message = "Unexpected null personStipends returned from the repository";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var personStipendEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PersonStipend, PersonStipend>();

            var personStipendDtos = personStipendEntities.Select(ppw => personStipendEntityToDtoAdapter.MapToType(ppw));

            return personStipendDtos;
        }
    }
}
