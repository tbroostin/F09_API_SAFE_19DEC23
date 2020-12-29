// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.BudgetManagement;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Services
{
    /// <summary>
    /// Provider for Budget Development configuration services.
    /// </summary>
    [RegisterType]
    public class BudgetDevelopmentConfigurationService : BaseCoordinationService, IBudgetDevelopmentConfigurationService
    {
        private IBudgetDevelopmentConfigurationRepository budgetDevelopmentConfigurationRepository;

        // Constructor for the Budget Development Configuration coordination service.
        public BudgetDevelopmentConfigurationService(IBudgetDevelopmentConfigurationRepository budgetDevelopmentConfigurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.budgetDevelopmentConfigurationRepository = budgetDevelopmentConfigurationRepository;
        }

        /// <summary>
        /// Get the budget configuration information.
        /// </summary>
        /// <returns>BudgetDevelopmentConfiguration DTO</returns>
        public async Task<BudgetConfiguration> GetBudgetDevelopmentConfigurationAsync()
        {
            var buDevConfigurationEntity = await budgetDevelopmentConfigurationRepository.GetBudgetDevelopmentConfigurationAsync();
            var buDevConfigurationDto = new BudgetConfiguration();

            // Check that the returned entity is not null and populate the DTO from it. 
            if (buDevConfigurationEntity != null)
            { 
                var adapter = _adapterRegistry.GetAdapter<Domain.BudgetManagement.Entities.BudgetConfiguration, Dtos.BudgetManagement.BudgetConfiguration>();

                buDevConfigurationDto = adapter.MapToType(buDevConfigurationEntity);
            }

            return buDevConfigurationDto;
        }
    }
}


