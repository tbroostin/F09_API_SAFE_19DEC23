/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Service class for Employee Compensation.
    /// </summary>
    [RegisterType]
    public class EmployeeCompensationService : BaseCoordinationService, IEmployeeCompensationService
    {
        private readonly IEmployeeCompensationRepository employeeCompensationRepository;

        private readonly IHumanResourcesReferenceDataRepository humanResourcesReferenceDataRepository;

        public EmployeeCompensationService(IEmployeeCompensationRepository employeeCompensationRepository, IHumanResourcesReferenceDataRepository humanResourcesReferenceDataRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStaffRepository staffRepository = null, IConfigurationRepository configurationRepository = null) : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            this.employeeCompensationRepository = employeeCompensationRepository;
            this.humanResourcesReferenceDataRepository = humanResourcesReferenceDataRepository;
        }

        /// <summary>
        /// Returns Employee Compensation Details
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user</param>
        /// <param name="salaryAmount">Estimated Annual Salary amount used in compensation re-calculation(if provided)</param>
        /// <returns>Employee Compensation DTO containing Benefit-Deductions,Taxes and Stipends. </returns>
        public async Task<EmployeeCompensation> GetEmployeeCompensationAsync(string effectivePersonId=null, decimal? salaryAmount=null)
        {
            if (string.IsNullOrEmpty(effectivePersonId))
                effectivePersonId = CurrentUser.PersonId;
            else if (!CurrentUser.IsPerson(effectivePersonId) && !HasPermission(HumanResourcesPermissionCodes.ViewAllTotalCompensation))
                throw new PermissionsException(string.Format("User {0} does not have permission to view employee compensation information for person {1}", CurrentUser.PersonId, effectivePersonId));

            //if logged in userid is not same as effective person id, it means the API is called from Total Comp Admin View.
            bool isAdminView = CurrentUser.IsPerson(effectivePersonId) ? false:true;
            var employeeCompensationEntities = await employeeCompensationRepository.GetEmployeeCompensationAsync(effectivePersonId, salaryAmount, isAdminView);

            var employeeCompensationEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EmployeeCompensation, Dtos.HumanResources.EmployeeCompensation>();
            var employeeCompensationDTO = employeeCompensationEntityToDtoAdapter.MapToType(employeeCompensationEntities);

            return employeeCompensationDTO;
        }
    }
}
