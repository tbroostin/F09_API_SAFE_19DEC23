// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
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
    /// This class implements the IInitiatorService interface
    /// </summary>
    [RegisterType]
    public class InitiatorService : BaseCoordinationService, IInitiatorService
    {
        private IInitiatorRepository initiatorRepository;
        private readonly IConfigurationRepository configurationRepository;
        private IStaffRepository staffRepository;

        public InitiatorService(IInitiatorRepository initiatorRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IStaffRepository staffRepository,
            ILogger logger) : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.initiatorRepository = initiatorRepository;
            this.configurationRepository = configurationRepository;
            this.staffRepository = staffRepository;
        }


        /// <summary>
        /// Get the list of initiator based on keyword search.
        /// </summary>
        /// <param name="queryKeyword"> The search criteria containing keyword for initiator search.</param>
        /// <returns> The staff search results</returns> 
        public async Task<IEnumerable<Initiator>> QueryInitiatorByKeywordAsync(string queryKeyword)
        {
            List<Initiator> initiatorDtos = new List<Initiator>();

            if (string.IsNullOrEmpty(queryKeyword))
            {
                throw new ArgumentNullException("queryKeyword", "query keyword is required to query.");
            }

            // Check the permission code to view initiator information.
            CheckViewInitiatorPermissions();

            // Get the list of initiator search result domain entity from the repository
            var initiatorDomainEntities = await initiatorRepository.QueryInitiatorByKeywordAsync(queryKeyword.Trim());

            if (initiatorDomainEntities == null || !initiatorDomainEntities.Any())
            {
                return initiatorDtos;
            }

            //sorting
            initiatorDomainEntities = initiatorDomainEntities.OrderBy(item => item.Code).ThenBy(x => x.Name);

            // Convert the vendor search result into DTOs
            var dtoAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.Initiator, Initiator>();
            foreach (var initiatorDomainEntity in initiatorDomainEntities)
            {
                initiatorDtos.Add(dtoAdapter.MapToType(initiatorDomainEntity));
            }

            return initiatorDtos;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view initiator information.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewInitiatorPermissions()
        {
                var hasPermission = (HasPermission(BasePermissionCodes.ViewAnyPerson) ||
                                        HasPermission(ColleagueFinancePermissionCodes.CreateUpdateRequisition) ||
                                        HasPermission(ColleagueFinancePermissionCodes.CreateUpdatePurchaseOrder));
                
            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view intiator information.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }
    }
}
