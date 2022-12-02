//Copyright 2018-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Dependency;
using Ellucian.Data.Colleague.Exceptions;

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
        /// Gets the configurations for leave balance
        /// </summary>
        /// <returns>LeaveBalanceConfiguration</returns>
        public async Task<LeaveBalanceConfiguration> GetLeaveBalanceConfigurationAsync()
        {
            var adapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.LeaveBalanceConfiguration, LeaveBalanceConfiguration>();
            try
            {
                var leaveManagementConfiguration = await leaveBalanceConfiguratioRepository.GetLeaveBalanceConfigurationAsync();
                return adapter.MapToType(leaveManagementConfiguration);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }

            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message);
            }
            
        }
    }
}
