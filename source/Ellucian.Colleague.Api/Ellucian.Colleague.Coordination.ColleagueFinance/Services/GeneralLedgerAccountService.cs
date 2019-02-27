// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Implements IGeneralLedgerAccountService.
    /// </summary>
    [RegisterType]
    public class GeneralLedgerAccountService : BaseCoordinationService, IGeneralLedgerAccountService
    {
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerAccountRepository generalLedgerAccountRepository;

        // This constructor initializes the private attributes.
        public GeneralLedgerAccountService(IGeneralLedgerUserRepository generalLedgerUserRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerAccountRepository generalLedgerAccountRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.generalLedgerUserRepository = generalLedgerUserRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerAccountRepository = generalLedgerAccountRepository;
        }

        /// <summary>
        /// Retrieves a General ledger account DTO.
        /// </summary>
        /// <param name="generalLedgerAccountId">General ledger account ID.</param>
        /// <returns>General ledger account DTO.</returns>
        public async Task<Dtos.ColleagueFinance.GeneralLedgerAccount> GetAsync(string generalLedgerAccountId)
        {
            #region Null checks
            if (string.IsNullOrEmpty(generalLedgerAccountId))
            {
                throw new ArgumentNullException("generalLedgerAccountId", "generalLedgerAccountId is required.");
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                throw new ConfigurationException("Account structure must be defined.");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ConfigurationException("GL class configuration must be defined.");
            }

            // Get the ID for the person who is logged in, and use the ID to get his list of assigned expense and revenue GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);
            if (generalLedgerUser == null)
            {
                throw new ApplicationException("GL user must be defined.");
            }

            var glAccountEntity = await generalLedgerAccountRepository.GetAsync(generalLedgerAccountId, glAccountStructure.MajorComponentStartPositions);
            if (glAccountEntity == null)
            {
                throw new ApplicationException("No general ledger account entity returned.");
            }
            #endregion

            var glAccountDto = new Dtos.ColleagueFinance.GeneralLedgerAccount();
            glAccountDto.Id = glAccountEntity.Id;
            glAccountDto.Description = "";
            glAccountDto.FormattedId = "";

            // Only send back the description if the user has access to the account.
            if (generalLedgerUser.AllAccounts != null && generalLedgerUser.AllAccounts.Contains(generalLedgerAccountId))
            {
                glAccountDto.FormattedId = glAccountEntity.FormattedGlAccount;
                glAccountDto.Description = glAccountEntity.Description;
            }

            return glAccountDto;
        }

        /// <summary>
        /// Validate a GL account. 
        /// If there is a fiscal year, it will also validate it for that year.
        /// </summary>
        /// <param name="generalLedgerAccountId">GL account ID.</param>
        /// <param name="fiscalYear">Optional; General Ledger fiscal year.</param>
        /// <returns>A GL account validation response DTO.</returns>
        public async Task<GlAccountValidationResponse> ValidateGlAccountAsync(string generalLedgerAccountId, string fiscalYear)
        {
            if (string.IsNullOrEmpty(generalLedgerAccountId))
            {
                throw new ArgumentNullException("generalLedgerAccountId", "generalLedgerAccountId is required.");
            }

            if (generalLedgerAccountId.Replace("-", "").Length > 15)
            {
                generalLedgerAccountId = generalLedgerAccountId.Replace("-", "_");
            }
            else
            {
                generalLedgerAccountId = generalLedgerAccountId.Replace("-", "");
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                throw new ConfigurationException("Account structure must be defined.");
            }

            // Get the excluded GL components (from BUWP).
            var budgetAdjustmentAccountExclusions = await generalLedgerConfigurationRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            if (budgetAdjustmentAccountExclusions == null)
            {
                budgetAdjustmentAccountExclusions = new Domain.ColleagueFinance.Entities.BudgetAdjustmentAccountExclusions();
            }
            var excludedAccountMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(new List<string>() { generalLedgerAccountId }, budgetAdjustmentAccountExclusions);

            // Get the GL class configuration.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ConfigurationException("GL class configuration must be defined.");
            }

            // Get the ID for the person who is logged in, and use the ID to get his list of assigned expense and revenue GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);
            if (generalLedgerUser == null)
            {
                throw new ApplicationException("GL user must be defined.");
            }

            // Initialize the DTO.
            var glAccountValidationResponseDto = new GlAccountValidationResponse();
            glAccountValidationResponseDto.Id = generalLedgerAccountId;

            // Check if the user has access to the GL account.
            if (generalLedgerUser.AllAccounts == null || !generalLedgerUser.AllAccounts.Any() || generalLedgerUser.ExpenseAccounts == null || !generalLedgerUser.ExpenseAccounts.Any())
            {
                // The user does not have access; return an error
                glAccountValidationResponseDto.Status = "failure";
                glAccountValidationResponseDto.ErrorMessage = "You do not have access to this GL Account.";
                return glAccountValidationResponseDto;
            }

            if (generalLedgerUser.AllAccounts != null && !generalLedgerUser.AllAccounts.Contains(generalLedgerAccountId))
            {
                // The user does not have access; return an error
                glAccountValidationResponseDto.Status = "failure";
                glAccountValidationResponseDto.ErrorMessage = "You do not have access to this GL Account.";
                return glAccountValidationResponseDto;
            }

            // Check that the GL Account is not an excluded account (as defined on BUWP).
            if (excludedAccountMessages != null && excludedAccountMessages.Any())
            {
                // The account is excluded; return an error.
                glAccountValidationResponseDto.Status = "failure";
                glAccountValidationResponseDto.ErrorMessage = "The account is excluded from online budget adjustments.";
                return glAccountValidationResponseDto;
            }

            // Check that is an expense GL account.
            if (generalLedgerUser.ExpenseAccounts != null && !generalLedgerUser.ExpenseAccounts.Contains(generalLedgerAccountId))
            {
                // The GL account is not an expense one; return an error
                glAccountValidationResponseDto.Status = "failure";
                glAccountValidationResponseDto.ErrorMessage = "The GL account is not an expense type.";
                return glAccountValidationResponseDto;
            }

            // Validate the GL account.
            var glAccountValidationResponseEntity = await generalLedgerAccountRepository.ValidateGlAccountAsync(generalLedgerAccountId, fiscalYear);

            // The user has access to the GL account. Proceed with validations.
            if (glAccountValidationResponseEntity == null)
            {
                throw new ApplicationException("No GL account validation response entity returned.");
            }

            // Check if validation returns any error.
            if (glAccountValidationResponseEntity.Status != "success")
            {
                // If validation returns errors, populate the error properties of the DTO.
                glAccountValidationResponseDto.Status = "failure";
                glAccountValidationResponseDto.ErrorMessage = glAccountValidationResponseEntity.ErrorMessage;
            }
            else
            {
                // The GL account was validated correctly.
                glAccountValidationResponseDto.Status = "success";
                glAccountValidationResponseDto.RemainingBalance = glAccountValidationResponseEntity.RemainingBalance;

                var glAccountEntity = await generalLedgerAccountRepository.GetAsync(generalLedgerAccountId, glAccountStructure.MajorComponentStartPositions);
                if (glAccountEntity == null)
                {
                    throw new ApplicationException("No GL account entity returned.");
                }

                glAccountValidationResponseDto.Id = glAccountEntity.Id;
                glAccountValidationResponseDto.Description = glAccountEntity.Description;
                glAccountValidationResponseDto.FormattedId = glAccountEntity.FormattedGlAccount;
            }

            return glAccountValidationResponseDto;
        }
    }
}