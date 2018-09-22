// Copyright 2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Service for accessing banking information configuration
    /// </summary>
    [RegisterType]
    public class BankingInformationConfigurationService : BaseCoordinationService, IBankingInformationConfigurationService
    {
        private IAdapterRegistry adapterRegistry;
        private IBankingInformationConfigurationRepository bankingInformationConfigurationRepository;
        public BankingInformationConfigurationService(
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IBankingInformationConfigurationRepository bankingInformationConfigurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.bankingInformationConfigurationRepository = bankingInformationConfigurationRepository;
        }

        /// <summary>
        /// Get the Configuration object for Colleague Self Service Banking Information
        /// </summary>
        /// <returns>Returns a single banking information configuration object</returns>
        public async Task<Dtos.Base.BankingInformationConfiguration> GetBankingInformationConfigurationAsync()
        {
            var domainConfiguration = await bankingInformationConfigurationRepository.GetBankingInformationConfigurationAsync();
            var domainToDtoConfigurationAdapter = adapterRegistry.GetAdapter<Domain.Base.Entities.BankingInformationConfiguration, Dtos.Base.BankingInformationConfiguration>();
            var dtoConfiguration = domainToDtoConfigurationAdapter.MapToType(domainConfiguration);
            return dtoConfiguration;
        }
    }
}
