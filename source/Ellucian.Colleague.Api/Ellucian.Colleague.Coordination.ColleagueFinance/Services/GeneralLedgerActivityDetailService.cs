// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Linq;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Provider for General Ledger activity details services.
    /// </summary>
    [RegisterType]
    public class GeneralLedgerActivityDetailsService : BaseCoordinationService, IGeneralLedgerActivityDetailService
    {
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerActivityDetailRepository glActivityDetailRepository;

        // Constructor for the GL account detail coordination service.
        public GeneralLedgerActivityDetailsService(IGeneralLedgerActivityDetailRepository glActivityDetailRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.generalLedgerUserRepository = generalLedgerUserRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.glActivityDetailRepository = glActivityDetailRepository;
        }

        /// <summary>
        /// Returns the GL activity detail DTOs for the GL account and fiscal year the user selected.
        /// </summary>
        /// <param name="glAccount">The GL account.</param>
        /// <param name="fiscalYear">General Ledger fiscal year.</param>
        /// <returns>A GL account DTO with its activity detail for the fiscal year.</returns>
        public async Task<Dtos.ColleagueFinance.GlAccountActivityDetail> QueryGlAccountActivityDetailAsync(string glAccount, string fiscalYear)
        {
            var glAccountDto = new Dtos.ColleagueFinance.GlAccountActivityDetail();

            // These arguments should contain a value.
            if (string.IsNullOrEmpty(glAccount))
            {
                throw new ArgumentNullException("glAccount", "A GL account must be specified.");
            }

            if (string.IsNullOrEmpty(fiscalYear))
            {
                throw new ArgumentNullException("fiscalYear", "A fiscal year must be specified.");
            }

            // Get the account structure configuration which is needed for the GL user repository.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get cost center configuration so we know how to calculate the cost center.
            var costCenterStructure = await generalLedgerConfigurationRepository.GetCostCenterStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get the lists of assigned revenue and expense GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);

            // Initialize the adapter to convert the GL account.
            var glAccountDtoAdapter = new GlAccountActivityDetailEntityToDtoAdapter(_adapterRegistry, logger);

            // Also validate that the user still has access to the selected GL account.
            if ((!generalLedgerUser.AllAccounts.Contains(glAccount)))
            {
                throw new PermissionsException("You do not have access to the requested GL account " + glAccount);
            }

            // Obtain the GL account activity for this GL account for the fiscal year.
            var glAccountDomain = await glActivityDetailRepository.QueryGlActivityDetailAsync(glAccount, fiscalYear, costCenterStructure, glClassConfiguration);

            // Convert the domain entity and its list of activity detail into DTOs.
            if (glAccountDomain != null)
            {
                glAccountDto = glAccountDtoAdapter.MapToType(glAccountDomain, glAccountStructure.MajorComponentStartPositions);
            }

            return glAccountDto;
        }
    }
}
