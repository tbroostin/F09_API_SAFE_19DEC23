//Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class LeaveBalanceConfigurationService : BaseCoordinationService, ILeaveBalanceConfigurationService
    {
        private readonly ILeaveBalanceConfigurationRepository leaveBalanceConfiguratioRepository;

        public LeaveBalanceConfigurationService(
            ILeaveBalanceConfigurationRepository leaveBalanceConfiguratioRepository,
            IAdapterRegistry adapterRegistry, 
            ICurrentUserFactory currentUserFactory, 
            IRoleRepository roleRepository, 
            ILogger logger, 
            IStaffRepository staffRepository = null, 
            IConfigurationRepository configurationRepository = null) 
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            this.leaveBalanceConfiguratioRepository = leaveBalanceConfiguratioRepository;
        }

        /// <summary>
        /// Converts a LeaveBalanceConfiguration domain entity to its corresponding DTO
        /// </summary>
        /// <param name="source">LeaveBalanceConfiguration domain entity</param>
        /// <returns>LeaveBalanceConfiguration DTO</returns>
        private async Task<Dtos.HumanResources.LeaveBalanceConfiguration> ConvertLeaveBalanceConfigurationEntityToDto(Domain.HumanResources.Entities.LeaveBalanceConfiguration source)
        {
            if (source != null)
            {
                var leaveConfigurationDto = new Dtos.HumanResources.LeaveBalanceConfiguration();
                leaveConfigurationDto.ExcludedLeavePlanIds = source.ExcludedLeavePlanIds;
                return leaveConfigurationDto;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the configurations for leave balance
        /// </summary>
        /// <returns>LeaveBalanceConfiguration</returns>
        public async Task<LeaveBalanceConfiguration> GetLeaveBalanceConfigurationAsync()
        {
            try
            {
                var leaveManagementConfiguration = await leaveBalanceConfiguratioRepository.GetLeaveBalanceConfigurationAsync();
                return await ConvertLeaveBalanceConfigurationEntityToDto(leaveManagementConfiguration);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
    }
}
