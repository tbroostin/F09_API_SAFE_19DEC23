// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
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
    /// Provider for GL account balances services.
    /// </summary>
    [RegisterType]
    public class GlAccountBalancesService : BaseCoordinationService, IGlAccountBalancesService
    {
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGlAccountBalancesRepository glAccountBalancesRepository;

        // Constructor for the GL account balances coordination service.
        public GlAccountBalancesService(IGlAccountBalancesRepository glAccountBalancesRepository,
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
            this.glAccountBalancesRepository = glAccountBalancesRepository;
        }

        /// <summary>
        /// Returns the GL account balances DTOs for the list of GL accounts and fiscal year.
        /// </summary>
        /// <param name="glAccounts">List of GL accounts.</param>
        /// <param name="fiscalYear">General Ledger fiscal year.</param>
        /// <returns>List of GL account balances DTO for the fiscal year.</returns>
        public async Task<IEnumerable<Dtos.ColleagueFinance.GlAccountBalances>> QueryGlAccountBalancesAsync(List<string> glAccounts, string fiscalYear)
        {
            List<Dtos.ColleagueFinance.GlAccountBalances> glAccountBalancesDto = new List<Dtos.ColleagueFinance.GlAccountBalances>();

            if (string.IsNullOrEmpty(fiscalYear))
            {
                throw new ArgumentNullException("fiscalYear", "A fiscal year must be specified.");
            }

            // These arguments should contain a value.
            if (glAccounts == null || !glAccounts.Any())
            {
                throw new ArgumentNullException("glAccounts", "GL accounts must be specified.");
            }

            // Check the permission code to view gl account balances for Procurement.
            CheckViewGlAccountBalancesPermission();

            // Get the account structure configuration which is needed for the GL user repository.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                throw new ApplicationException("GL account structure is not set up.");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            if (glClassConfiguration == null)
            {
                throw new ApplicationException("GL class configuration is not set up.");
            }

            // Get the ID for the person who is logged in, and use the ID to get the lists of assigned revenue and expense GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);
            
            if (generalLedgerUser == null)
            {
                throw new ApplicationException("No GL user definition available.");
            }

            if (generalLedgerUser.ExpenseAccounts == null || !generalLedgerUser.ExpenseAccounts.Any())
            {
                throw new PermissionsException("You do not have access to gl accounts");
            }

            List<string> glAccountsInternal = new List<string>();
            foreach (var glAcc in glAccounts)
            {
                string internalGlAccount = GlAccountUtility.ConvertGlAccountToInternalFormat(glAcc, glAccountStructure.MajorComponentStartPositions);
                if (!string.IsNullOrEmpty(internalGlAccount))
                {
                    glAccountsInternal.Add(internalGlAccount);
                }
            }

            // Obtain the GL account balances the fiscal year.
            var glAccountBalancesDomainEntities = await glAccountBalancesRepository.QueryGlAccountBalancesAsync(glAccountsInternal, fiscalYear, generalLedgerUser, glAccountStructure, glClassConfiguration);

            if (glAccountBalancesDomainEntities != null && glAccountBalancesDomainEntities.Any())
            {
                var glAccountBalancesDtoAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.GlAccountBalances, Dtos.ColleagueFinance.GlAccountBalances>();
                foreach (var glAccountBalance in glAccountBalancesDomainEntities)
                {
                    glAccountBalancesDto.Add(glAccountBalancesDtoAdapter.MapToType(glAccountBalance));
                }
            }

            return glAccountBalancesDto;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view GL account balances in Procurement.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewGlAccountBalancesPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.CreateUpdateRequisition)
                || HasPermission(ColleagueFinancePermissionCodes.CreateUpdatePurchaseOrder)
                || HasPermission(ColleagueFinancePermissionCodes.CreateUpdateVoucher);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view gl account balances.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }
    }


}
