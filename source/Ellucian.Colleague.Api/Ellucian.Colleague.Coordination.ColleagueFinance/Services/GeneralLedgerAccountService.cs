// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
                logger.Debug("==> generalLedgerAccountId is null or empty <==");
                throw new ArgumentNullException("generalLedgerAccountId", "generalLedgerAccountId is required.");
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                logger.Debug("==> glAccountStructure is null <==");
                throw new ConfigurationException("Account structure must be defined.");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                logger.Debug("==> glClassConfiguration is null <==");
                throw new ConfigurationException("GL class configuration must be defined.");
            }

            // Get the ID for the person who is logged in, and use the ID to get his list of assigned expense and revenue GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);
            if (generalLedgerUser == null)
            {
                logger.Debug("==> generalLedgerUser is null <==");
                throw new ApplicationException("GL user must be defined.");
            }

            var glAccountEntity = await generalLedgerAccountRepository.GetAsync(generalLedgerAccountId, glAccountStructure.MajorComponentStartPositions);
            if (glAccountEntity == null)
            {
                logger.Debug("==> glAccountEntity is null <==");
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
        /// Retrieves the list of active expense GL account DTOs for which the user has access.
        /// </summary>
        /// <param name="glClass">Optional: null for all the user GL accounts, expense for only the expense type GL accounts.</param>
        /// <returns>A collection of active expense GL account DTOs for the user.</returns>
        public async Task<IEnumerable<Dtos.ColleagueFinance.GlAccount>> GetUserGeneralLedgerAccountsAsync(string glClass)
        {
            Stopwatch watch = null;

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                logger.Debug("==> glAccountStructure is null <==");
                throw new ConfigurationException("Account structure must be defined.");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                logger.Debug("==> glClassConfiguration is null <==");
                throw new ConfigurationException("GL class configuration must be defined.");
            }

            var glAccountDtos = new List<GlAccount>();

            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            // Get the ID for the person who is logged in, and use the ID to get his list of assigned expense GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);

            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("==> GL account LookUp SERVICE timing: GetGeneralLedgerUserAsync2 completed in " + watch.ElapsedMilliseconds.ToString() + " ms <==");
            }

            if (generalLedgerUser == null)
            {
                logger.Debug("==> generalLedgerUser is null <==");
                throw new ApplicationException("GL user must be defined.");
            }

            List<string> glAccounts = new List<string>();
            if (string.IsNullOrWhiteSpace(glClass))
            {
                if (generalLedgerUser.AllAccounts != null && generalLedgerUser.AllAccounts.Any())
                {
                    logger.Debug("==> generalLedgerUser.AllAccounts is used <==");
                    glAccounts = generalLedgerUser.AllAccounts.ToList();
                }
                else
                {
                    logger.Debug("==> generalLedgerUser.AllAccounts is null or empty <==");
                }
            }
            else if (glClass.ToUpperInvariant().Substring(0, 1) == "E")
            {
                if (generalLedgerUser.ExpenseAccounts != null && generalLedgerUser.ExpenseAccounts.Any())
                {
                    logger.Debug("==> generalLedgerUser.ExpenseAccounts is used <==");
                    glAccounts = generalLedgerUser.ExpenseAccounts.ToList();
                }
                else
                {
                    logger.Debug("==> generalLedgerUser.ExpenseAccounts is null or empty <==");
                }
            }
            else
            {
                logger.Debug("==> Incorrect gl class <==");
                throw new ArgumentNullException("glClass", "glClass must be null for all types or expense.");
            }

            logger.Info("number of GL accounts " + glAccounts.Count());

            // Limit the list of accounts to those that are active.
            List<string> activeGlAccounts = await generalLedgerAccountRepository.GetActiveGeneralLedgerAccounts(glAccounts);

            if (activeGlAccounts != null && activeGlAccounts.Any())
            {
                if (logger.IsInfoEnabled)
                {
                    logger.Info("==> number of active GL accounts " + activeGlAccounts.Count() + " <==");
                    watch.Restart();
                }

                var glAccountEntities = await generalLedgerAccountRepository.GetUserGeneralLedgerAccountsAsync(activeGlAccounts, glAccountStructure);

                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("number of GL accounts entities " + glAccountEntities.Count());
                    logger.Info("GL account LookUp SERVICE timing: GetUserGeneralLedgerAccountsAsync completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
                }

                if (glAccountEntities != null && glAccountEntities.Any())
                {
                    var glAccountAdapter = new GlAccountEntityToDtoAdapter2(_adapterRegistry, logger);

                    if (logger.IsInfoEnabled)
                    {
                        watch.Restart();
                    }

                    glAccountEntities = glAccountEntities.OrderBy(x => x.GlAccountNumber, StringComparer.Ordinal).ToList();

                    foreach (var glAccount in glAccountEntities)
                    {
                        if (glAccount != null && !string.IsNullOrWhiteSpace(glAccount.GlAccountNumber))
                        {
                            var glAccountDto = glAccountAdapter.MapToType(glAccount, glAccountStructure.MajorComponentStartPositions);
                            glAccountDto.GlClass = GetGlClass(glAccount.GlAccountNumber, glAccountStructure, glClassConfiguration);
                            glAccountDtos.Add(glAccountDto);
                        }
                    }

                    if (logger.IsInfoEnabled)
                    {
                        watch.Stop();
                        logger.Info("GL account LookUp SERVICE timing: converting entities into dtos completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
                    }
                }
            }

            return glAccountDtos;
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
                logger.Debug("==> generalLedgerAccountId is null or empty <==");
                throw new ArgumentNullException("generalLedgerAccountId", "generalLedgerAccountId is required.");
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                logger.Debug("==> glAccountStructure is null <==");
                throw new ConfigurationException("Account structure must be defined.");
            }

            // Convert the GL account number to internal format. We cannot be sure in which format it comes in.
            generalLedgerAccountId = GlAccountUtility.ConvertGlAccountToInternalFormat(generalLedgerAccountId, glAccountStructure.MajorComponentStartPositions);

            // Get the excluded GL components (from BUWP).
            var budgetAdjustmentAccountExclusions = await generalLedgerConfigurationRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            if (budgetAdjustmentAccountExclusions == null)
            {
                budgetAdjustmentAccountExclusions = new Domain.ColleagueFinance.Entities.BudgetAdjustmentAccountExclusions();
            }
            var excludedAccountMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(new List<string>() { generalLedgerAccountId }, budgetAdjustmentAccountExclusions, glAccountStructure.MajorComponentStartPositions);

            // Get the GL class configuration.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                logger.Debug("==> glClassConfiguration is null <==");
                throw new ConfigurationException("GL class configuration must be defined.");
            }

            // Get the ID for the person who is logged in, and use the ID to get his list of assigned expense and revenue GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);
            if (generalLedgerUser == null)
            {
                logger.Debug("==> generalLedgerUser is null <==");
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

                try
                {
                    var glAccountEntity = await generalLedgerAccountRepository.GetAsync(generalLedgerAccountId, glAccountStructure.MajorComponentStartPositions);

                    if (glAccountEntity == null)
                    {
                        logger.Debug("==> glAccountEntity not returned <==");
                        throw new ApplicationException("No GL account entity returned.");
                    }

                    glAccountValidationResponseDto.Id = glAccountEntity.Id;
                    glAccountValidationResponseDto.Description = glAccountEntity.Description;
                    glAccountValidationResponseDto.FormattedId = glAccountEntity.FormattedGlAccount;

                }
                catch (ColleagueSessionExpiredException csee)
                {
                    logger.Error(csee, "==> generalLedgerAccountRepository.GetAsync session expired <==");
                    throw;
                }
            }

            return glAccountValidationResponseDto;
        }

        private static Dtos.ColleagueFinance.GlClass GetGlClass(string glAccountNumber, Domain.ColleagueFinance.Entities.GeneralLedgerAccountStructure glAccountStructure, Domain.ColleagueFinance.Entities.GeneralLedgerClassConfiguration glClassConfiguration)
        {
            Domain.ColleagueFinance.Entities.GlClass glClassEnum = GlAccountUtility.GetGlAccountGlClass(glAccountNumber, glClassConfiguration, glAccountStructure.MajorComponentStartPositions);

            // Translate the domain GlClass into the DTO GlClass
            switch (glClassEnum)
            {
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Asset:
                    return Dtos.ColleagueFinance.GlClass.Asset;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Expense:
                    return Dtos.ColleagueFinance.GlClass.Expense;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.FundBalance:
                    return Dtos.ColleagueFinance.GlClass.FundBalance;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Liability:
                    return Dtos.ColleagueFinance.GlClass.Liability;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Revenue:
                    return Dtos.ColleagueFinance.GlClass.Revenue;
                default:
                    throw new ApplicationException("Invalid glClass for GL account: " + glAccountNumber);
            }
        }
    }
}