/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.HumanResources.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Service class for Employee Current Benefits.
    /// </summary>
    [RegisterType]
    public class CurrentBenefitsService : BaseCoordinationService, ICurrentBenefitsService
    {
        private readonly ICurrentBenefitsRepository currentBenefitsRepository;
        public CurrentBenefitsService(ICurrentBenefitsRepository currentBenefitsRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStaffRepository staffRepository = null, IConfigurationRepository configurationRepository = null) : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            this.currentBenefitsRepository = currentBenefitsRepository;
        }

        /// <summary>
        /// Returns Employee Current Benefits Details
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user</param>
        /// <returns>EmployeeBenefits DTO object</returns>
        public async Task<EmployeeBenefits> GetEmployeesCurrentBenefitsAsync(string effectivePersonId)
        {
            if (string.IsNullOrEmpty(effectivePersonId))
                effectivePersonId = CurrentUser.PersonId;
            else if (!CurrentUser.IsPerson(effectivePersonId))
                throw new PermissionsException(string.Format("User {0} does not have permission to view employee current benefits information for person {1}", CurrentUser.PersonId, effectivePersonId));
            
            var employeeCurrentBenefitsEntitiy = await currentBenefitsRepository.GetEmployeeCurrentBenefitsAsync(effectivePersonId);

            var employeeCurrentBenefitsEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EmployeeBenefits, Dtos.HumanResources.EmployeeBenefits>();
            var employeeCurrentBenefitsDto = employeeCurrentBenefitsEntityToDtoAdapter.MapToType(employeeCurrentBenefitsEntitiy);

            return employeeCurrentBenefitsDto;
        }
    }
}
