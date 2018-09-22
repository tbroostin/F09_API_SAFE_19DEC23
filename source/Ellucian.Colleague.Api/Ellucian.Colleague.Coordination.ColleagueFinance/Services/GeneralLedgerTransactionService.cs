// Copyright 2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Implements the IGeneralLedgerTransactionService
    /// </summary>
    [RegisterType]
    public class GeneralLedgerTransactionService : BaseCoordinationService, IGeneralLedgerTransactionService
    {
        private IGeneralLedgerTransactionRepository generalLedgerTransactionRepository;
        private IPersonRepository personRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;
        IDictionary<string, string> _projectReferenceIds = null;


        // Constructor to initialize the private attributes
        public GeneralLedgerTransactionService(IGeneralLedgerTransactionRepository generalLedgerTransactionRepository,
            IPersonRepository personRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IColleagueFinanceReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.generalLedgerTransactionRepository = generalLedgerTransactionRepository;
            this.personRepository = personRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
            this._referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Returns the DTO for the specified general ledger transaction
        /// </summary>
        /// <param name="id">Guid to General Ledger Transaction</param>
        /// <returns>General Ledger Transaction DTO</returns>
        public async Task<Ellucian.Colleague.Dtos.GeneralLedgerTransaction> GetByIdAsync(string id)
        {
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            // Get the general ledger transaction domain entity from the repository
            var generalLedgerTransactionDomainEntity = await generalLedgerTransactionRepository.GetByIdAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel);

            if (generalLedgerTransactionDomainEntity == null)
            {
                throw new ArgumentNullException("GeneralLedgerTransactionDomainEntity", "GeneralLedgerTransactionDomainEntity cannot be null.");
            }

            var projectIds = generalLedgerTransactionDomainEntity.GeneralLedgerTransactions
                            .SelectMany(t => t.TransactionDetailLines)
                            .Where(i => !string.IsNullOrEmpty(i.ProjectId))
                            .Select(p => p.ProjectId)
                            .ToList()
                            .Distinct();
            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            // Convert the general ledger transaction and all its child objects into DTOs.
            return await BuildGeneralLedgerTransactionDtoAsync(generalLedgerTransactionDomainEntity, glConfiguration);
        }

        /// <summary>
        /// Returns the DTO for the specified general ledger transaction for Ethos version 8
        /// </summary>
        /// <param name="id">Guid to General Ledger Transaction</param>
        /// <returns>General Ledger Transaction DTO</returns>
        public async Task<Ellucian.Colleague.Dtos.GeneralLedgerTransaction2> GetById2Async(string id)
        {
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            // Get the general ledger transaction domain entity from the repository
            var generalLedgerTransactionDomainEntity = await generalLedgerTransactionRepository.GetByIdAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel);

            if (generalLedgerTransactionDomainEntity == null)
            {
                throw new ArgumentNullException("GeneralLedgerTransactionDomainEntity", "GeneralLedgerTransactionDomainEntity cannot be null.");
            }

            var projectIds = generalLedgerTransactionDomainEntity.GeneralLedgerTransactions
                            .SelectMany(t => t.TransactionDetailLines)
                            .Where(i => !string.IsNullOrEmpty(i.ProjectId))
                            .Select(p => p.ProjectId)
                            .ToList()
                            .Distinct();
            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            // Convert the general ledger transaction and all its child objects into DTOs.
            return await BuildGeneralLedgerTransactionDto2Async(generalLedgerTransactionDomainEntity, glConfiguration);
        }

        /// <summary>
        /// Returns all general ledger transactions for the data model version 6
        /// </summary>
        /// <returns>Collection of GeneralLedgerTransactions</returns>
        public async Task<IEnumerable<Dtos.GeneralLedgerTransaction>> GetAsync()
        {
            var generalLedgerTransactionDtos = new List<Dtos.GeneralLedgerTransaction>();
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            // Get the general ledger transaction domain entity from the repository
            var generalLedgerTransactionDomainEntities = await generalLedgerTransactionRepository.GetAsync(CurrentUser.PersonId, generalLedgerUser.GlAccessLevel);

            if (generalLedgerTransactionDomainEntities == null)
            {
                throw new ArgumentNullException("GeneralLedgerTransactionDomainEntity", "GeneralLedgerTransactionDomainEntity cannot be null.");
            }

            var projectIds = generalLedgerTransactionDomainEntities
                            .SelectMany(s => s.GeneralLedgerTransactions)
                            .SelectMany(t => t.TransactionDetailLines)
                            .Where(i => !string.IsNullOrEmpty(i.ProjectId))
                            .Select(p => p.ProjectId)
                            .ToList()
                            .Distinct();
            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            // Convert the general ledger transaction and all its child objects into DTOs.
            foreach (var entity in generalLedgerTransactionDomainEntities)
            {
                if (entity != null)
                {
                    var generalLedgerTransactionDto = await BuildGeneralLedgerTransactionDtoAsync(entity, glConfiguration);
                    generalLedgerTransactionDtos.Add(generalLedgerTransactionDto);
                }
            }
            return generalLedgerTransactionDtos;
        }

        /// <summary>
        /// Returns all general ledger transactions for the data model version 8
        /// </summary>
        /// <returns>Collection of GeneralLedgerTransactions</returns>
        public async Task<IEnumerable<Dtos.GeneralLedgerTransaction2>> Get2Async()
        {
            var generalLedgerTransactionDtos = new List<Dtos.GeneralLedgerTransaction2>();
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            // Get the general ledger transaction domain entity from the repository
            var generalLedgerTransactionDomainEntities = await generalLedgerTransactionRepository.GetAsync(CurrentUser.PersonId, generalLedgerUser.GlAccessLevel);

            if (generalLedgerTransactionDomainEntities == null)
            {
                throw new ArgumentNullException("GeneralLedgerTransactionDomainEntity", "GeneralLedgerTransactionDomainEntity cannot be null.");
            }
            var projectIds = generalLedgerTransactionDomainEntities
                            .SelectMany(s => s.GeneralLedgerTransactions)
                            .SelectMany(t => t.TransactionDetailLines)
                            .Where(i => !string.IsNullOrEmpty(i.ProjectId))
                            .Select(p => p.ProjectId)
                            .ToList()
                            .Distinct();
            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }
            // Convert the general ledger transaction and all its child objects into DTOs.
            foreach (var entity in generalLedgerTransactionDomainEntities)
            {
                if (entity != null)
                {
                    var generalLedgerTransactionDto = await BuildGeneralLedgerTransactionDto2Async(entity, glConfiguration);
                    generalLedgerTransactionDtos.Add(generalLedgerTransactionDto);
                }
            }
            return generalLedgerTransactionDtos;
        }        

        /// <summary>
        /// Update a single general ledger transaction for the data model version 6
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<Dtos.GeneralLedgerTransaction> UpdateAsync(string id, Dtos.GeneralLedgerTransaction generalLedgerDto)
        {
            ValidateGeneralLedgerDto(generalLedgerDto);

            generalLedgerTransactionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            if (!generalLedgerDto.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentOutOfRangeException("generalLedgerDto.Id", "The id in the body must match the id in the request.");
            }

            var generalLedgerTransactionDto = new Dtos.GeneralLedgerTransaction();

            var generalLedgerTransactionDtos = new List<Dtos.GeneralLedgerTransaction>();
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            var generalLedgerTransactionEntity = await BuildGeneralLedgerTransactionEntityAsync(generalLedgerDto, glConfiguration.MajorComponents.Count);
            var entity = await generalLedgerTransactionRepository.UpdateAsync(id, generalLedgerTransactionEntity, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, glConfiguration);

            var projectIds = entity.GeneralLedgerTransactions
                                   .Where(tr => tr.TransactionDetailLines.Any())
                                   .SelectMany(trd => trd.TransactionDetailLines.Select(x => x.ProjectId))
                                   .ToList()
                                   .Distinct();

            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            generalLedgerTransactionDto = await BuildGeneralLedgerTransactionDtoAsync(entity, glConfiguration);

            return generalLedgerTransactionDto;
        }

        /// <summary>
        /// Update a single general ledger transaction for the data model version 8
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<Dtos.GeneralLedgerTransaction2> Update2Async(string id, Dtos.GeneralLedgerTransaction2 generalLedgerDto)
        {
            ValidateGeneralLedgerDto2(generalLedgerDto);

            generalLedgerTransactionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            if (!generalLedgerDto.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentOutOfRangeException("generalLedgerDto.Id", "The id in the body must match the id in the request.");
            }

            var generalLedgerTransactionDto = new Dtos.GeneralLedgerTransaction2();

            var generalLedgerTransactionDtos = new List<Dtos.GeneralLedgerTransaction2>();
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            var generalLedgerTransactionEntity = await BuildGeneralLedgerTransactionEntity2Async(generalLedgerDto, glConfiguration.MajorComponents.Count);
            var entity = await generalLedgerTransactionRepository.UpdateAsync(id, generalLedgerTransactionEntity, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, glConfiguration);

            var projectIds = entity.GeneralLedgerTransactions
                                   .Where(tr => tr.TransactionDetailLines.Any())
                                   .SelectMany(trd => trd.TransactionDetailLines.Select(x => x.ProjectId))
                                   .ToList()
                                   .Distinct();

            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            generalLedgerTransactionDto = await BuildGeneralLedgerTransactionDto2Async(entity, glConfiguration);

            return generalLedgerTransactionDto;
        }

        

        /// <summary>
        /// Create a single general ledger transaction for the data model version 6
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<Dtos.GeneralLedgerTransaction> CreateAsync(Dtos.GeneralLedgerTransaction generalLedgerDto)
        {
            ValidateGeneralLedgerDto(generalLedgerDto);

            generalLedgerTransactionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var generalLedgerTransactionDto = new Dtos.GeneralLedgerTransaction();

            var generalLedgerTransactionDtos = new List<Dtos.GeneralLedgerTransaction>();
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            var generalLedgerTransactionEntity = await BuildGeneralLedgerTransactionEntityAsync(generalLedgerDto, glConfiguration.MajorComponents.Count);
            var entity = await generalLedgerTransactionRepository.CreateAsync(generalLedgerTransactionEntity, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, glConfiguration);

            var projectIds = entity.GeneralLedgerTransactions
                                  .Where(tr => tr.TransactionDetailLines.Any())
                                  .SelectMany(trd => trd.TransactionDetailLines.Select(x => x.ProjectId))
                                  .ToList()
                                  .Distinct();

            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            generalLedgerTransactionDto = await BuildGeneralLedgerTransactionDtoAsync(entity, glConfiguration);

            return generalLedgerTransactionDto;
        }

        /// <summary>
        /// Create a single general ledger transaction for the data model version 8
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<Dtos.GeneralLedgerTransaction2> Create2Async(Dtos.GeneralLedgerTransaction2 generalLedgerDto)
        {
            ValidateGeneralLedgerDto2(generalLedgerDto);

            generalLedgerTransactionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var generalLedgerTransactionDto = new Dtos.GeneralLedgerTransaction2();

            var generalLedgerTransactionDtos = new List<Dtos.GeneralLedgerTransaction2>();
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            var generalLedgerTransactionEntity = await BuildGeneralLedgerTransactionEntity2Async(generalLedgerDto, glConfiguration.MajorComponents.Count);
            var entity = await generalLedgerTransactionRepository.CreateAsync(generalLedgerTransactionEntity, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, glConfiguration);

            var projectIds = entity.GeneralLedgerTransactions
                                  .Where(tr => tr.TransactionDetailLines.Any())
                                  .SelectMany(trd => trd.TransactionDetailLines.Select(x => x.ProjectId))
                                  .ToList()
                                  .Distinct();

            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            generalLedgerTransactionDto = await BuildGeneralLedgerTransactionDto2Async(entity, glConfiguration);

            return generalLedgerTransactionDto;
        }

        

        /// <summary>
        /// validate general ledger transaction for the data model version 6
        /// </summary>
        /// <returns></returns>
        private void ValidateGeneralLedgerDto(Dtos.GeneralLedgerTransaction generalLedgerDto)
        {
            decimal? creditAmount = 0;
            decimal? debitAmount = 0;
            if (generalLedgerDto == null)
            {
                throw new ArgumentNullException("generalLedgerDto", "The body of the request is required for updates.");
            }
            if (generalLedgerDto.ProcessMode != Dtos.EnumProperties.ProcessMode.Update &&
                generalLedgerDto.ProcessMode != Dtos.EnumProperties.ProcessMode.Validate)
            {
                throw new ArgumentOutOfRangeException("generalLedgerDto.processMode", "Process Mode of update or validate is required.");
            }
            if (generalLedgerDto.Transactions == null)
            {
                throw new ArgumentNullException("generalLedgerDto.transactions", "Transactions are missing from the request.");
            }
            foreach (var trans in generalLedgerDto.Transactions)
            {
                if (trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.Donation &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.DonationEndowed &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.Pledge &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.PledgeEndowed)
                {
                    throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.type", "Transaction type only supports Donations and Pledges");
                }
                if (trans.LedgerDate == null)
                {
                    throw new ArgumentNullException("generalLedgerDto.transactions.ledgerDate", "Each transaction must have a ledgerDate assigned.");
                }
                if (trans.TransactionDetailLines == null)
                {
                    throw new ArgumentNullException("generalLedgerDto.transactions.transactionDetailLines", "Transaction Detail is required.");
                }
                foreach (var transDetail in trans.TransactionDetailLines)
                {
                    if (string.IsNullOrEmpty(transDetail.AccountingString))
                    {
                        throw new ArgumentNullException("generalLedgerDto.transactions.transactionDetailLines.accountingString", "Each transaction detail must have an accounting string.");
                    }
                    if (string.IsNullOrEmpty(transDetail.Description))
                    {
                        throw new ArgumentNullException("generalLedgerDto.transactions.transactionDetailLines.description", "Each transaction detail must have a description.");
                    }
                    if (transDetail.Amount == null || !transDetail.Amount.Value.HasValue || transDetail.Amount.Value == 0)
                    {
                        throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.amount.value", "Each transaction detail must have an amount and value.");
                    }
                    if (transDetail.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD &&
                        transDetail.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
                    {
                        throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.amount.currency", "Each transaction detail must have currency of either USD or CAD.");
                    }
                    if (transDetail.Type != Dtos.EnumProperties.CreditOrDebit.Credit &&
                        transDetail.Type != Dtos.EnumProperties.CreditOrDebit.Debit)
                    {
                        throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.type", "Each Detail item must be specified as either Debit or Credit.");
                    }
                    if (transDetail.Type == Dtos.EnumProperties.CreditOrDebit.Credit)
                    {
                        creditAmount += transDetail.Amount.Value;
                    }
                    else
                    {
                        debitAmount += transDetail.Amount.Value;
                    }
                }
            }
            if (debitAmount != creditAmount)
            {
                throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.amount.value", string.Format("Total Credits ({0}) and Debits ({1}) must be equal across all transaction detail entries.", creditAmount, debitAmount));
            }
        }

        /// <summary>
        /// validate general ledger transaction for the data model version 6
        /// </summary>
        /// <returns></returns>

        private void ValidateGeneralLedgerDto2(Dtos.GeneralLedgerTransaction2 generalLedgerDto)
        {
            decimal? creditAmount = 0;
            decimal? debitAmount = 0;
            if (generalLedgerDto == null)
            {
                throw new ArgumentNullException("generalLedgerDto", "The body of the request is required for updates.");
            }
            if (generalLedgerDto.ProcessMode != Dtos.EnumProperties.ProcessMode.Update &&
                generalLedgerDto.ProcessMode != Dtos.EnumProperties.ProcessMode.Validate)
            {
                throw new ArgumentOutOfRangeException("generalLedgerDto.processMode", "Process Mode of update or validate is required.");
            }
            if (generalLedgerDto.Transactions == null)
            {
                throw new ArgumentNullException("generalLedgerDto.transactions", "Transactions are missing from the request.");
            }
            foreach (var trans in generalLedgerDto.Transactions)
            {
                if (trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.Donation &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.DonationEndowed &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.Pledge &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.PledgeEndowed)
                {
                    throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.type", "Transaction type only supports Donations and Pledges");
                }
                if (trans.LedgerDate == null)
                {
                    throw new ArgumentNullException("generalLedgerDto.transactions.ledgerDate", "Each transaction must have a ledgerDate assigned.");
                }
                if (trans.TransactionDetailLines == null)
                {
                    throw new ArgumentNullException("generalLedgerDto.transactions.transactionDetailLines", "Transaction Detail is required.");
                }
                foreach (var transDetail in trans.TransactionDetailLines)
                {
                    if (string.IsNullOrEmpty(transDetail.AccountingString))
                    {
                        throw new ArgumentNullException("generalLedgerDto.transactions.transactionDetailLines.accountingString", "Each transaction detail must have an accounting string.");
                    }
                    if (string.IsNullOrEmpty(transDetail.Description))
                    {
                        throw new ArgumentNullException("generalLedgerDto.transactions.transactionDetailLines.description", "Each transaction detail must have a description.");
                    }
                    if (transDetail.Amount == null || !transDetail.Amount.Value.HasValue || transDetail.Amount.Value == 0)
                    {
                        throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.amount.value", "Each transaction detail must have an amount and value.");
                    }
                    if (transDetail.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD &&
                        transDetail.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
                    {
                        throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.amount.currency", "Each transaction detail must have currency of either USD or CAD.");
                    }
                    if (transDetail.Type != Dtos.EnumProperties.CreditOrDebit.Credit &&
                        transDetail.Type != Dtos.EnumProperties.CreditOrDebit.Debit)
                    {
                        throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.type", "Each Detail item must be specified as either Debit or Credit.");
                    }
                    if (transDetail.Type == Dtos.EnumProperties.CreditOrDebit.Credit)
                    {
                        creditAmount += transDetail.Amount.Value;
                    }
                    else
                    {
                        debitAmount += transDetail.Amount.Value;
                    }
                }
            }
            if (debitAmount != creditAmount)
            {
                throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.amount.value", string.Format("Total Credits ({0}) and Debits ({1}) must be equal across all transaction detail entries.", creditAmount, debitAmount));
            }
        }

        /// <summary>
        /// Convert general ledger transaction entity to DTO for the data model version 6 
        /// </summary>
        /// <param name="generalLedgerTransactionEntity">General Ledger Entity</param>
        /// /// <param name="GlConfig">General Ledger Config</param>
        /// <returns>General Ledger Transaction DTO</returns>
        private async Task<Dtos.GeneralLedgerTransaction> BuildGeneralLedgerTransactionDtoAsync(Domain.ColleagueFinance.Entities.GeneralLedgerTransaction generalLedgerTransactionEntity, GeneralLedgerAccountStructure GlConfig)
        {
            var generalLedgerTransactionDto = new Dtos.GeneralLedgerTransaction();

            generalLedgerTransactionDto.Id = generalLedgerTransactionEntity.Id;
            if (string.IsNullOrEmpty(generalLedgerTransactionDto.Id)) generalLedgerTransactionDto.Id = "00000000-0000-0000-0000-000000000000";
            generalLedgerTransactionDto.ProcessMode = (Dtos.EnumProperties.ProcessMode)Enum.Parse(typeof(Dtos.EnumProperties.ProcessMode), generalLedgerTransactionEntity.ProcessMode);
            generalLedgerTransactionDto.Transactions = new List<Dtos.DtoProperties.GeneralLedgerTransactionDtoProperty>();

            foreach (var transaction in generalLedgerTransactionEntity.GeneralLedgerTransactions)
            {
                var genLedgrTransactionDto = new Dtos.DtoProperties.GeneralLedgerTransactionDtoProperty()
                {
                    LedgerDate = transaction.LedgerDate,
                    Type = ConverEntityTypeToDtoType(transaction.Source),
                    TransactionNumber = transaction.TransactionNumber,
                    ReferenceNumber = transaction.ReferenceNumber,
                    Reference = new Dtos.DtoProperties.GeneralLedgerReferenceDtoProperty()
                    {
                        Person = (!string.IsNullOrEmpty(transaction.ReferencePersonId) && !(await personRepository.IsCorpAsync(transaction.ReferencePersonId)) ? new Dtos.GuidObject2(await personRepository.GetPersonGuidFromIdAsync(transaction.ReferencePersonId)) : null),
                        Organization = (!string.IsNullOrEmpty(transaction.ReferencePersonId) && (await personRepository.IsCorpAsync(transaction.ReferencePersonId)) ? new Dtos.GuidObject2(await personRepository.GetPersonGuidFromIdAsync(transaction.ReferencePersonId)) : null),
                    },
                    TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                    TransactionDetailLines = new List<Dtos.DtoProperties.GeneralLedgerDetailDtoProperty>()
                };
                foreach (var transactionDetail in transaction.TransactionDetailLines)
                {
                    string accountingString = transactionDetail.GlAccount.GetFormattedGlAccount(GlConfig.MajorComponentStartPositions);
                    accountingString = GetFormattedGlAccount(accountingString, GlConfig);

                    if (!string.IsNullOrEmpty(transactionDetail.ProjectId))
                    {
                        accountingString = ConvertAccountingstringToIncludeProjectRefNo(transactionDetail.ProjectId, accountingString);
                    }
                    var genLedgrTransactionDetailDto = new Dtos.DtoProperties.GeneralLedgerDetailDtoProperty()
                    {
                        AccountingString = accountingString,
                        Amount = new Dtos.DtoProperties.AmountDtoProperty() { Value = transactionDetail.Amount.Value, Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), transactionDetail.Amount.Currency.ToString())},
                        Description = transactionDetail.GlAccount.GlAccountDescription,
                        SequenceNumber = transactionDetail.SequenceNumber,
                        Type = (Dtos.EnumProperties.CreditOrDebit)Enum.Parse(typeof(Dtos.EnumProperties.CreditOrDebit), transactionDetail.Type.ToString())
                    };
                    genLedgrTransactionDto.TransactionDetailLines.Add(genLedgrTransactionDetailDto);
                }
                generalLedgerTransactionDto.Transactions.Add(genLedgrTransactionDto);
            }

            return generalLedgerTransactionDto;
        }

        /// <summary>
        /// Convert general ledger transaction entity to DTO for the data model version 8
        /// </summary>
        /// <param name="generalLedgerTransactionEntity">General Ledger Entity</param>
        /// /// <param name="GlConfig">General Ledger Config</param>
        /// <returns>General Ledger Transaction DTO</returns>
        private async Task<Dtos.GeneralLedgerTransaction2> BuildGeneralLedgerTransactionDto2Async(Domain.ColleagueFinance.Entities.GeneralLedgerTransaction generalLedgerTransactionEntity, GeneralLedgerAccountStructure GlConfig)
        {
            var generalLedgerTransactionDto = new Dtos.GeneralLedgerTransaction2();

            generalLedgerTransactionDto.Id = generalLedgerTransactionEntity.Id;
            if (string.IsNullOrEmpty(generalLedgerTransactionDto.Id)) generalLedgerTransactionDto.Id = "00000000-0000-0000-0000-000000000000";
            generalLedgerTransactionDto.ProcessMode = (Dtos.EnumProperties.ProcessMode)Enum.Parse(typeof(Dtos.EnumProperties.ProcessMode), generalLedgerTransactionEntity.ProcessMode);
            if (!string.IsNullOrEmpty(generalLedgerTransactionEntity.SubmittedBy))
            {
                var SubmittedByGuid = await personRepository.GetPersonGuidFromIdAsync(generalLedgerTransactionEntity.SubmittedBy);
                if (!string.IsNullOrEmpty(SubmittedByGuid))
                {
                    generalLedgerTransactionDto.SubmittedBy = new Dtos.GuidObject2(SubmittedByGuid);
                }
            }
            generalLedgerTransactionDto.Transactions = new List<Dtos.DtoProperties.GeneralLedgerTransactionDtoProperty2>();

            foreach (var transaction in generalLedgerTransactionEntity.GeneralLedgerTransactions)
            {
                var genLedgrTransactionDto = new Dtos.DtoProperties.GeneralLedgerTransactionDtoProperty2()
                {
                    LedgerDate = transaction.LedgerDate,
                    Type = ConverEntityTypeToDtoType(transaction.Source),
                    TransactionNumber = transaction.TransactionNumber,
                    ReferenceNumber = transaction.ReferenceNumber,
                    Reference = new Dtos.DtoProperties.GeneralLedgerReferenceDtoProperty()
                    {
                        Person = (!string.IsNullOrEmpty(transaction.ReferencePersonId) && !(await personRepository.IsCorpAsync(transaction.ReferencePersonId)) ? new Dtos.GuidObject2(await personRepository.GetPersonGuidFromIdAsync(transaction.ReferencePersonId)) : null),
                        Organization = (!string.IsNullOrEmpty(transaction.ReferencePersonId) && (await personRepository.IsCorpAsync(transaction.ReferencePersonId)) ? new Dtos.GuidObject2(await personRepository.GetPersonGuidFromIdAsync(transaction.ReferencePersonId)) : null),
                    },
                    TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                    TransactionDetailLines = new List<Dtos.DtoProperties.GeneralLedgerDetailDtoProperty2>()
                };
                foreach (var transactionDetail in transaction.TransactionDetailLines)
                {
                    string accountingString = transactionDetail.GlAccount.GetFormattedGlAccount(GlConfig.MajorComponentStartPositions);
                    accountingString = GetFormattedGlAccount(accountingString, GlConfig);

                    if (!string.IsNullOrEmpty(transactionDetail.ProjectId))
                    {
                        accountingString = ConvertAccountingstringToIncludeProjectRefNo(transactionDetail.ProjectId, accountingString);
                    }
                                        
                    var genLedgrTransactionDetailDto = new Dtos.DtoProperties.GeneralLedgerDetailDtoProperty2()
                    {
                        AccountingString = accountingString,
                        Amount = new Dtos.DtoProperties.AmountDtoProperty() { Value = transactionDetail.Amount.Value, Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), transactionDetail.Amount.Currency.ToString()) },
                        Description = transactionDetail.GlAccount.GlAccountDescription,
                        SequenceNumber = transactionDetail.SequenceNumber,
                        Type = (Dtos.EnumProperties.CreditOrDebit)Enum.Parse(typeof(Dtos.EnumProperties.CreditOrDebit), transactionDetail.Type.ToString()),
                        SubmittedBy = !string.IsNullOrEmpty(transactionDetail.SubmittedBy) ? new Dtos.GuidObject2(await personRepository.GetPersonGuidFromIdAsync(transactionDetail.SubmittedBy)) : null
                    };
                    genLedgrTransactionDto.TransactionDetailLines.Add(genLedgrTransactionDetailDto);
                }
                generalLedgerTransactionDto.Transactions.Add(genLedgrTransactionDto);
            }

            return generalLedgerTransactionDto;
        }

        private string GetFormattedGlAccount(string accountNumber, GeneralLedgerAccountStructure GlConfig)
        {
            string formattedGlAccount = string.Empty;
            string tempGlNo = string.Empty;
            formattedGlAccount = Regex.Replace(accountNumber, "[^0-9a-zA-Z]", "");

            int startLoc = 0;
            int x = 0, glCount = GlConfig.MajorComponents.Count;

            foreach (var glMajor in GlConfig.MajorComponents)
            {
                try
                {
                    x++;
                    if (x < glCount) { tempGlNo = tempGlNo + formattedGlAccount.Substring(startLoc, glMajor.ComponentLength) + GlConfig.glDelimiter; }
                    else { tempGlNo = tempGlNo + formattedGlAccount.Substring(startLoc, glMajor.ComponentLength); }
                    startLoc += glMajor.ComponentLength;
                }
                catch (ArgumentOutOfRangeException aex)
                {
                    throw new InvalidOperationException(string.Format("Invalid GL account number: {0}", accountNumber));
                }
            }
            formattedGlAccount = tempGlNo;

            return formattedGlAccount;
        }

        private Dtos.EnumProperties.GeneralLedgerTransactionType ConverEntityTypeToDtoType(string entityType)
        {
            switch (entityType)
            {
                case "DN":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.Donation;
                case "DNE":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.DonationEndowed;
                case "PL":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.Pledge;
                case "PLE":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.PledgeEndowed;
                default:
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.Donation;
            }
        }


        private async Task<Domain.ColleagueFinance.Entities.GeneralLedgerTransaction> BuildGeneralLedgerTransactionEntityAsync(Dtos.GeneralLedgerTransaction generalLedgerTransactionDto, int GLCompCount)
        {
            var generalLedgerTransactionEntity = new Domain.ColleagueFinance.Entities.GeneralLedgerTransaction()
                {
                    Id = (generalLedgerTransactionDto.Id != null && !string.IsNullOrEmpty(generalLedgerTransactionDto.Id)) ? generalLedgerTransactionDto.Id : string.Empty
                };

            generalLedgerTransactionEntity.ProcessMode = generalLedgerTransactionDto.ProcessMode.ToString();
            generalLedgerTransactionEntity.GeneralLedgerTransactions = new List<GenLedgrTransaction>();

            foreach (var transaction in generalLedgerTransactionDto.Transactions)
            {
                string personId = string.Empty;
                if (transaction.Reference != null)
                {
                    if (transaction.Reference.Person != null && !string.IsNullOrEmpty(transaction.Reference.Person.Id))
                    {
                        personId = await personRepository.GetPersonIdFromGuidAsync(transaction.Reference.Person.Id);
                        if (string.IsNullOrEmpty(personId) || (await personRepository.IsCorpAsync(personId)))
                        {
                            throw new ArgumentException(string.Format("The id '{0}' is not a valid person in Colleague.", transaction.Reference.Person.Id), "generalLedgerDto.transactions.reference.person.id");
                        }
                    }
                    if (transaction.Reference.Organization != null && !string.IsNullOrEmpty(transaction.Reference.Organization.Id))
                    {
                        personId = await personRepository.GetPersonIdFromGuidAsync(transaction.Reference.Organization.Id);
                        if (string.IsNullOrEmpty(personId) || !(await personRepository.IsCorpAsync(personId)))
                        {
                            throw new ArgumentException(string.Format("The id '{0}' is not a valid organization in Colleague.", transaction.Reference.Organization.Id), "generalLedgerDto.transactions.reference.organization.id");
                        }
                    }
                }
                var genLedgrTransactionEntity = new GenLedgrTransaction(ConvertDtoTypeToEntityType(transaction.Type), transaction.LedgerDate)
                {
                    TransactionNumber = transaction.TransactionNumber,
                    ReferenceNumber = transaction.ReferenceNumber,
                    ReferencePersonId = personId,
                    TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                    TransactionDetailLines = new List<GenLedgrTransactionDetail>()
                };
                foreach (var transactionDetail in transaction.TransactionDetailLines)
                {
                    string accountingString = transactionDetail.AccountingString;

                    //if GL number contains an Astrisks and if there more then one we are
                    // assuming that the delimiter is an astrisk, So we'll convert that delimiter to something
                    // else while perserving the project code if present.

                    if ((accountingString.Split('*').Length -1) > 1 )
                    {
                        int CountAstrisks = accountingString.Split('*').Length - 1;
                        if ((GLCompCount - 1) < CountAstrisks)
                        {
                            int lastIndex = accountingString.LastIndexOf('*');
                            accountingString = accountingString.Substring(0, lastIndex).Replace("*", "")
                                  + accountingString.Substring(lastIndex);
                        } else
                        {
                            accountingString = Regex.Replace(accountingString, "[^0-9a-zA-Z]", "");
                        }
                    }

                    string project = "";
                    var amount = new AmountAndCurrency(transactionDetail.Amount.Value, (Domain.ColleagueFinance.Entities.CurrencyCodes)Enum.Parse(typeof(Domain.ColleagueFinance.Entities.CurrencyCodes), transactionDetail.Amount.Currency.ToString()));
                    var type = (Domain.ColleagueFinance.Entities.CreditOrDebit)Enum.Parse(typeof(Domain.ColleagueFinance.Entities.CreditOrDebit), transactionDetail.Type.ToString());
                    var genLedgrTransactionDetailEntity = new GenLedgrTransactionDetail(accountingString, project, transactionDetail.Description, type, amount)
                    {
                        SequenceNumber = transactionDetail.SequenceNumber
                    };
                    genLedgrTransactionEntity.TransactionDetailLines.Add(genLedgrTransactionDetailEntity);
                }
                generalLedgerTransactionEntity.GeneralLedgerTransactions.Add(genLedgrTransactionEntity);
            }

            return generalLedgerTransactionEntity;
        }

        private async Task<Domain.ColleagueFinance.Entities.GeneralLedgerTransaction> BuildGeneralLedgerTransactionEntity2Async(Dtos.GeneralLedgerTransaction2 generalLedgerTransactionDto, int GLCompCount)
        {
            var generalLedgerTransactionEntity = new Domain.ColleagueFinance.Entities.GeneralLedgerTransaction()
            {
                Id = (generalLedgerTransactionDto.Id != null && !string.IsNullOrEmpty(generalLedgerTransactionDto.Id)) ? generalLedgerTransactionDto.Id : string.Empty
            };

            generalLedgerTransactionEntity.ProcessMode = generalLedgerTransactionDto.ProcessMode.ToString();
            if (generalLedgerTransactionDto.SubmittedBy != null && !string.IsNullOrEmpty(generalLedgerTransactionDto.SubmittedBy.Id))
            {
                var submittedById = await personRepository.GetPersonIdFromGuidAsync(generalLedgerTransactionDto.SubmittedBy.Id);
                if (string.IsNullOrEmpty(submittedById))
                {
                    throw new ArgumentException(string.Concat(" SubmittedBy ID '", generalLedgerTransactionDto.SubmittedBy.Id.ToString(), "' was not found. Valid Person Id is required."));
                }
                generalLedgerTransactionEntity.SubmittedBy = submittedById;
            }
            generalLedgerTransactionEntity.GeneralLedgerTransactions = new List<GenLedgrTransaction>();

            foreach (var transaction in generalLedgerTransactionDto.Transactions)
            {
                string personId = string.Empty;
                if (transaction.Reference != null)
                {
                    if (transaction.Reference.Person != null && !string.IsNullOrEmpty(transaction.Reference.Person.Id))
                    {
                        personId = await personRepository.GetPersonIdFromGuidAsync(transaction.Reference.Person.Id);
                        if (string.IsNullOrEmpty(personId) || (await personRepository.IsCorpAsync(personId)))
                        {
                            throw new ArgumentException(string.Format("The id '{0}' is not a valid person in Colleague.", transaction.Reference.Person.Id), "generalLedgerDto.transactions.reference.person.id");
                        }
                    }
                    if (transaction.Reference.Organization != null && !string.IsNullOrEmpty(transaction.Reference.Organization.Id))
                    {
                        personId = await personRepository.GetPersonIdFromGuidAsync(transaction.Reference.Organization.Id);
                        if (string.IsNullOrEmpty(personId) || !(await personRepository.IsCorpAsync(personId)))
                        {
                            throw new ArgumentException(string.Format("The id '{0}' is not a valid organization in Colleague.", transaction.Reference.Organization.Id), "generalLedgerDto.transactions.reference.organization.id");
                        }
                    }
                }
                var genLedgrTransactionEntity = new GenLedgrTransaction(ConvertDtoTypeToEntityType(transaction.Type), transaction.LedgerDate)
                {
                    TransactionNumber = transaction.TransactionNumber,
                    ReferenceNumber = transaction.ReferenceNumber,
                    ReferencePersonId = personId,
                    TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                    TransactionDetailLines = new List<GenLedgrTransactionDetail>()
                };
                foreach (var transactionDetail in transaction.TransactionDetailLines)
                {
                    string accountingString = transactionDetail.AccountingString;

                    //if GL number contains an Astrisks and if there more then one we are
                    // assuming that the delimiter is an astrisk, So we'll convert that delimiter to something
                    // else while perserving the project code if present.

                    if ((accountingString.Split('*').Length - 1) > 1)
                    {
                        int CountAstrisks = accountingString.Split('*').Length - 1;
                        if ((GLCompCount - 1) < CountAstrisks)
                        {
                            int lastIndex = accountingString.LastIndexOf('*');
                            accountingString = accountingString.Substring(0, lastIndex).Replace("*", "")
                                  + accountingString.Substring(lastIndex);
                        }
                        else
                        {
                            accountingString = Regex.Replace(accountingString, "[^0-9a-zA-Z]", "");
                        }
                    }

                    string project = "";
                    var amount = new AmountAndCurrency(transactionDetail.Amount.Value, (Domain.ColleagueFinance.Entities.CurrencyCodes)Enum.Parse(typeof(Domain.ColleagueFinance.Entities.CurrencyCodes), transactionDetail.Amount.Currency.ToString()));
                    var type = (Domain.ColleagueFinance.Entities.CreditOrDebit)Enum.Parse(typeof(Domain.ColleagueFinance.Entities.CreditOrDebit), transactionDetail.Type.ToString());
                    var submittedById =string.Empty;
                    if (transactionDetail.SubmittedBy != null && !string.IsNullOrEmpty(transactionDetail.SubmittedBy.Id))
                    {
                        submittedById = await personRepository.GetPersonIdFromGuidAsync(transactionDetail.SubmittedBy.Id);
                        if (string.IsNullOrEmpty(submittedById))
                        {
                            throw new ArgumentException(string.Concat(" Transaction Detail SubmittedBy ID '", transactionDetail.SubmittedBy.Id.ToString(), "' was not found. Valid Person Id is required."));
                        }
                    }
                    var genLedgrTransactionDetailEntity = new GenLedgrTransactionDetail(accountingString, project, transactionDetail.Description, type, amount)
                    {
                        SequenceNumber = transactionDetail.SequenceNumber,
                        SubmittedBy = submittedById
                    };
                    genLedgrTransactionEntity.TransactionDetailLines.Add(genLedgrTransactionDetailEntity);
                }
                generalLedgerTransactionEntity.GeneralLedgerTransactions.Add(genLedgrTransactionEntity);
            }

            return generalLedgerTransactionEntity;
        }


        private string ConvertDtoTypeToEntityType(Dtos.EnumProperties.GeneralLedgerTransactionType? dtoType)
        {
            switch (dtoType)
            {
                case Dtos.EnumProperties.GeneralLedgerTransactionType.Donation:
                    return "DN";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.DonationEndowed:
                    return "DNE";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.Pledge:
                    return "PL";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.PledgeEndowed:
                    return "PLE";
                default:
                    return "DN";
            }
        }

        #region V12 Changes

        /// <summary>
        /// Returns the DTO for the specified general ledger transaction for Ethos version 8
        /// </summary>
        /// <param name="id">Guid to General Ledger Transaction</param>
        /// <returns>General Ledger Transaction DTO</returns>
        public async Task<Ellucian.Colleague.Dtos.GeneralLedgerTransaction3> GetById3Async(string id)
        {
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            // Get the general ledger transaction domain entity from the repository
            var generalLedgerTransactionDomainEntity = await generalLedgerTransactionRepository.GetById2Async(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel);

            if (generalLedgerTransactionDomainEntity == null)
            {
                throw new ArgumentNullException("GeneralLedgerTransactionDomainEntity", "GeneralLedgerTransactionDomainEntity cannot be null.");
            }

            var projectIds = generalLedgerTransactionDomainEntity.GeneralLedgerTransactions
                            .SelectMany(t => t.TransactionDetailLines)
                            .Where(i => !string.IsNullOrEmpty(i.ProjectId))
                            .Select(p => p.ProjectId)
                            .ToList()
                            .Distinct();
            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            // Convert the general ledger transaction and all its child objects into DTOs.
            return await ConvertGeneralLedgerTransactionEntityToDto3Async(generalLedgerTransactionDomainEntity, glConfiguration, true);
        }


        /// <summary>
        /// Returns all general ledger transactions for the data model version 8
        /// </summary>
        /// <returns>Collection of GeneralLedgerTransactions</returns>
        public async Task<IEnumerable<Dtos.GeneralLedgerTransaction3>> Get3Async(bool bypassCache)
        {
            var generalLedgerTransactionDtos = new List<Dtos.GeneralLedgerTransaction3>();
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            // Get the general ledger transaction domain entity from the repository
            var generalLedgerTransactionDomainEntities = await generalLedgerTransactionRepository.Get2Async(CurrentUser.PersonId, generalLedgerUser.GlAccessLevel);

            if (generalLedgerTransactionDomainEntities == null)
            {
                throw new ArgumentNullException("GeneralLedgerTransactionDomainEntity", "GeneralLedgerTransactionDomainEntity cannot be null.");
            }

            var projectIds = generalLedgerTransactionDomainEntities
                            .SelectMany(s => s.GeneralLedgerTransactions)
                            .SelectMany(t => t.TransactionDetailLines)
                            .Where(i => !string.IsNullOrEmpty(i.ProjectId))
                            .Select(p => p.ProjectId)
                            .ToList()
                            .Distinct();
            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }
            // Convert the general ledger transaction and all its child objects into DTOs.
            foreach (var entity in generalLedgerTransactionDomainEntities)
            {
                if (entity != null)
                {
                    var generalLedgerTransactionDto = await ConvertGeneralLedgerTransactionEntityToDto3Async(entity, glConfiguration, bypassCache);
                    generalLedgerTransactionDtos.Add(generalLedgerTransactionDto);
                }
            }
            return generalLedgerTransactionDtos;
        }

        /// <summary>
        /// Update a single general ledger transaction for the data model version 12
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<Dtos.GeneralLedgerTransaction3> Update3Async(string id, Dtos.GeneralLedgerTransaction3 generalLedgerDto)
        {
            ValidateGeneralLedgerDto3(generalLedgerDto);

            generalLedgerTransactionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            if (!generalLedgerDto.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentOutOfRangeException("generalLedgerDto.Id", "The id in the body must match the id in the request.");
            }

            var generalLedgerTransactionDto = new Dtos.GeneralLedgerTransaction3();

            var generalLedgerTransactionDtos = new List<Dtos.GeneralLedgerTransaction3>();
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            var generalLedgerTransactionEntity = await ConvertLedgerTransactionDtoToEntity3Async(generalLedgerDto, glConfiguration.MajorComponents.Count);

            var entity = await generalLedgerTransactionRepository.Update2Async(id, generalLedgerTransactionEntity, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, glConfiguration);

            var projectIds = entity.GeneralLedgerTransactions
                                   .Where(tr => tr.TransactionDetailLines.Any())
                                   .SelectMany(trd => trd.TransactionDetailLines.Select(x => x.ProjectId))
                                   .ToList()
                                   .Distinct();

            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            generalLedgerTransactionDto = await ConvertGeneralLedgerTransactionEntityToDto3Async(entity, glConfiguration, true);

            return generalLedgerTransactionDto;
        }

        /// <summary>
        /// Create a single general ledger transaction for the data model version 12
        /// </summary>
        /// <returns>A single GeneralLedgerTransaction</returns>
        public async Task<Dtos.GeneralLedgerTransaction3> Create3Async(Dtos.GeneralLedgerTransaction3 generalLedgerDto)
        {
            ValidateGeneralLedgerDto3(generalLedgerDto);

            generalLedgerTransactionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            await CheckGeneralLedgerTransactionPermissions(generalLedgerDto);

            var generalLedgerTransactionDto = new Dtos.GeneralLedgerTransaction3();

            var generalLedgerTransactionDtos = new List<Dtos.GeneralLedgerTransaction2>();
            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);

            var generalLedgerTransactionEntity = await ConvertLedgerTransactionDtoToEntity3Async(generalLedgerDto, glConfiguration.MajorComponents.Count);

            var entity = await generalLedgerTransactionRepository.Create2Async(generalLedgerTransactionEntity, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, glConfiguration);

            var projectIds = entity.GeneralLedgerTransactions
                                   .Where(tr => tr.TransactionDetailLines.Any())
                                   .SelectMany(trd => trd.TransactionDetailLines.Select(x => x.ProjectId))
                                   .ToList()
                                   .Distinct();

            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await generalLedgerTransactionRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            generalLedgerTransactionDto = await ConvertGeneralLedgerTransactionEntityToDto3Async(entity, glConfiguration, true);

            return generalLedgerTransactionDto;
        }

        /// <summary>
        /// Checks persmissions.
        /// </summary>
        /// <param name="generalLedgerDto"></param>
        private async Task CheckGeneralLedgerTransactionPermissions(GeneralLedgerTransaction3 generalLedgerDto)
        {
            var userPermissionList = (await GetUserPermissionCodesAsync()).ToList();

            // This is the overall permission code needed to create anything with this API.
            if (!userPermissionList.Contains(ColleagueFinancePermissionCodes.CreateGLPostings))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create general ledger postings.");
                throw new PermissionsException("User is not authorized to create general ledger postings.");
            }

            var transactionTypes = generalLedgerDto.Transactions.Select(t => t.Type);

            if(transactionTypes != null && transactionTypes.Any())
            {
                transactionTypes.ToList().ForEach(type => 
                {
                    if(type == GeneralLedgerTransactionType.ActualOpenBalance || type == GeneralLedgerTransactionType.MiscGeneralLedgerTransaction)
                    {
                        if(!userPermissionList.Contains(ColleagueFinancePermissionCodes.CreateJournalEntries))
                        {
                            logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create journal entries.");
                            throw new PermissionsException("User is not authorized to create journal entries.");
                        }
                    }

                    if (type == GeneralLedgerTransactionType.ApprovedBudget || type == GeneralLedgerTransactionType.ContingentBudget || 
                        type == GeneralLedgerTransactionType.ApprovedBudgetAdjustment)
                    {
                        if (!userPermissionList.Contains(ColleagueFinancePermissionCodes.CreateBudgetEntries))
                        {
                            logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create budget entries.");
                            throw new PermissionsException("User is not authorized to create budget entries.");
                        }
                    }

                    if(type == GeneralLedgerTransactionType.EncumbranceOpenBalance || type == GeneralLedgerTransactionType.GeneralEncumbranceCreate)
                    {
                        if (!userPermissionList.Contains(ColleagueFinancePermissionCodes.CreateEncumbranceEntries))
                        {
                            logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create encumbrance entries.");
                            throw new PermissionsException("User is not authorized to create encumbrance entries.");
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Converts dto to entity.
        /// </summary>
        /// <param name="generalLedgerTransactionDto"></param>
        /// <param name="GLCompCount"></param>
        /// <returns></returns>
        private async Task<Domain.ColleagueFinance.Entities.GeneralLedgerTransaction> ConvertLedgerTransactionDtoToEntity3Async(Dtos.GeneralLedgerTransaction3 generalLedgerTransactionDto, int GLCompCount)
        {
            var generalLedgerTransactionEntity = new Domain.ColleagueFinance.Entities.GeneralLedgerTransaction()
            {
                Id = (generalLedgerTransactionDto.Id != null && !string.IsNullOrEmpty(generalLedgerTransactionDto.Id)) ? generalLedgerTransactionDto.Id : string.Empty
            };

            if (generalLedgerTransactionDto.ProcessMode == ProcessMode2.UpdateBatch || generalLedgerTransactionDto.ProcessMode == ProcessMode2.UpdateImmediate)
            {
                generalLedgerTransactionEntity.ProcessMode = ProcessMode2.UpdateImmediate.ToString();
            }
            else if (generalLedgerTransactionDto.ProcessMode == ProcessMode2.Validate)
            {
                generalLedgerTransactionEntity.ProcessMode = ProcessMode2.Validate.ToString();
            }

            if (generalLedgerTransactionDto.SubmittedBy != null && !string.IsNullOrEmpty(generalLedgerTransactionDto.SubmittedBy.Id))
            {
                var submittedById = await personRepository.GetPersonIdFromGuidAsync(generalLedgerTransactionDto.SubmittedBy.Id);
                if (string.IsNullOrEmpty(submittedById))
                {
                    throw new ArgumentException(string.Concat(" SubmittedBy ID '", generalLedgerTransactionDto.SubmittedBy.Id.ToString(), "' was not found. Valid Person Id is required."));
                }
                generalLedgerTransactionEntity.SubmittedBy = submittedById;
            }
            //V12 changes
            if (!string.IsNullOrEmpty(generalLedgerTransactionDto.Comment))
            {
                generalLedgerTransactionEntity.Comment = generalLedgerTransactionDto.Comment;
            }
            generalLedgerTransactionEntity.GeneralLedgerTransactions = new List<GenLedgrTransaction>();

            foreach (var transaction in generalLedgerTransactionDto.Transactions)
            {
                string personId = string.Empty;
                if (transaction.Reference != null)
                {
                    if (transaction.Reference.Person != null && !string.IsNullOrEmpty(transaction.Reference.Person.Id))
                    {
                        personId = await personRepository.GetPersonIdFromGuidAsync(transaction.Reference.Person.Id);
                        if (string.IsNullOrEmpty(personId) || (await personRepository.IsCorpAsync(personId)))
                        {
                            throw new ArgumentException(string.Format("The id '{0}' is not a valid person in Colleague.", transaction.Reference.Person.Id), "generalLedgerDto.transactions.reference.person.id");
                        }
                    }
                    if (transaction.Reference.Organization != null && !string.IsNullOrEmpty(transaction.Reference.Organization.Id))
                    {
                        personId = await personRepository.GetPersonIdFromGuidAsync(transaction.Reference.Organization.Id);
                        if (string.IsNullOrEmpty(personId) || !(await personRepository.IsCorpAsync(personId)))
                        {
                            throw new ArgumentException(string.Format("The id '{0}' is not a valid organization in Colleague.", transaction.Reference.Organization.Id), "generalLedgerDto.transactions.reference.organization.id");
                        }
                    }
                }

                if((transaction.Type != null && !string.IsNullOrEmpty(transaction.ReferenceNumber)) && (transaction.Type == GeneralLedgerTransactionType.ActualOpenBalance || transaction.Type == GeneralLedgerTransactionType.MiscGeneralLedgerTransaction ||
                    transaction.Type == GeneralLedgerTransactionType.ApprovedBudget || transaction.Type == GeneralLedgerTransactionType.ContingentBudget ||
                    transaction.Type == GeneralLedgerTransactionType.ApprovedBudgetAdjustment || transaction.Type == GeneralLedgerTransactionType.TemporaryBudget ||
                    transaction.Type == GeneralLedgerTransactionType.TemporaryBudgetAdjustment || transaction.Type == GeneralLedgerTransactionType.EncumbranceOpenBalance||
                    transaction.Type == GeneralLedgerTransactionType.GeneralEncumbranceCreate))
                {
                    throw new InvalidOperationException("referenceNumber must be null when creating a Journal Entry or Budget Entry or Encumbrance Entry as it will be generated by the system.");
                }

                if(transaction.LedgerDate.HasValue && (transaction.Type == GeneralLedgerTransactionType.ActualOpenBalance || transaction.Type == GeneralLedgerTransactionType.ApprovedBudget ||
                   transaction.Type == GeneralLedgerTransactionType.EncumbranceOpenBalance))
                {
                    var year = await this.GetFiscalYear(transaction.LedgerDate.Value.Date, true);
                    DateTime fiscalYearStartDate;
                    if(!year.FiscalStartMonth.Value.Equals(1))
                    {
                        fiscalYearStartDate = new DateTime(Convert.ToInt32(year.Id) - 1, year.FiscalStartMonth.Value, 1);
                    }
                    else
                    {
                        fiscalYearStartDate = new DateTime(Convert.ToInt32(year.Id), year.FiscalStartMonth.Value, 1);
                    }

                    if (!transaction.LedgerDate.Value.Date.Equals(fiscalYearStartDate)) 
                    {
                        throw new InvalidOperationException("Opening balance transactions must use the first day of the fiscal year as the ledgerDate.");
                    }
                }

                var genLedgrTransactionEntity = new GenLedgrTransaction(ConvertDtoTypeToEntityType2(transaction.Type), transaction.LedgerDate)
                {
                    TransactionNumber = transaction.TransactionNumber,
                    ReferenceNumber = transaction.ReferenceNumber,
                    ReferencePersonId = personId,
                    TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,
                    TransactionDetailLines = new List<GenLedgrTransactionDetail>()
                };
                foreach (var transactionDetail in transaction.TransactionDetailLines)
                {
                    string accountingString = transactionDetail.AccountingString;

                    //if GL number contains an Astrisks and if there more then one we are
                    // assuming that the delimiter is an astrisk, So we'll convert that delimiter to something
                    // else while perserving the project code if present.

                    if ((accountingString.Split('*').Length - 1) > 1)
                    {
                        int CountAstrisks = accountingString.Split('*').Length - 1;
                        if ((GLCompCount - 1) < CountAstrisks)
                        {
                            int lastIndex = accountingString.LastIndexOf('*');
                            accountingString = accountingString.Substring(0, lastIndex).Replace("*", "")
                                  + accountingString.Substring(lastIndex);
                        }
                        else
                        {
                            accountingString = Regex.Replace(accountingString, "[^0-9a-zA-Z]", "");
                        }
                    }

                    string project = "";
                    var amount = new AmountAndCurrency(transactionDetail.Amount.Value, (Domain.ColleagueFinance.Entities.CurrencyCodes)Enum.Parse(typeof(Domain.ColleagueFinance.Entities.CurrencyCodes), transactionDetail.Amount.Currency.ToString()));
                    var type = (Domain.ColleagueFinance.Entities.CreditOrDebit)Enum.Parse(typeof(Domain.ColleagueFinance.Entities.CreditOrDebit), transactionDetail.Type.ToString());
                    var submittedById = string.Empty;
                    if (transactionDetail.SubmittedBy != null && !string.IsNullOrEmpty(transactionDetail.SubmittedBy.Id))
                    {
                        submittedById = await personRepository.GetPersonIdFromGuidAsync(transactionDetail.SubmittedBy.Id);
                        if (string.IsNullOrEmpty(submittedById))
                        {
                            throw new ArgumentException(string.Concat(" Transaction Detail SubmittedBy ID '", transactionDetail.SubmittedBy.Id.ToString(), "' was not found. Valid Person Id is required."));
                        }
                    }

                    var genLedgrTransactionDetailEntity = new GenLedgrTransactionDetail(accountingString, project, transactionDetail.Description, type, amount);

                    genLedgrTransactionDetailEntity.SequenceNumber = transactionDetail.SequenceNumber.HasValue ? transactionDetail.SequenceNumber : default(int?);
                    genLedgrTransactionDetailEntity.SubmittedBy = submittedById;
                    //V12 changes
                    genLedgrTransactionDetailEntity.EncGiftUnits = transactionDetail.GiftUnits.HasValue ? transactionDetail.GiftUnits.Value.ToString() : string.Empty;
                    if (transactionDetail.Encumbrance != null)
                    {
                        genLedgrTransactionDetailEntity.EncAdjustmentType = transactionDetail.Encumbrance.AdjustmentType == null ? string.Empty : transactionDetail.Encumbrance.AdjustmentType.ToString();
                        genLedgrTransactionDetailEntity.EncCommitmentType = transactionDetail.Encumbrance.CommitmentType == null ? string.Empty : transactionDetail.Encumbrance.CommitmentType.ToString();
                        genLedgrTransactionDetailEntity.EncLineItemNumber = transactionDetail.Encumbrance.LineItemNumber.HasValue ? transactionDetail.Encumbrance.LineItemNumber.Value.ToString() : string.Empty;
                        genLedgrTransactionDetailEntity.EncRefNumber = string.IsNullOrEmpty(transactionDetail.Encumbrance.Number) ? string.Empty : transactionDetail.Encumbrance.Number;
                        genLedgrTransactionDetailEntity.EncSequenceNumber = transactionDetail.Encumbrance.SequenceNumber.HasValue ? transactionDetail.Encumbrance.SequenceNumber.Value : default(int?);
                    }
                    genLedgrTransactionEntity.TransactionDetailLines.Add(genLedgrTransactionDetailEntity);
                }
                generalLedgerTransactionEntity.GeneralLedgerTransactions.Add(genLedgrTransactionEntity);
            }

            return generalLedgerTransactionEntity;
        }

        /// <summary>
        /// Gets a fiscal year.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="outDate"></param>
        /// <returns></returns>
        private async Task<FiscalYear> GetFiscalYear(DateTime outDate, bool bypassCache)
        {
            FiscalYear fiscalYearLookup = null;

            var fiscalYears = await FiscalYearsAsync(bypassCache);

            foreach (var year in fiscalYears)
            {
                if (!year.FiscalStartMonth.HasValue)
                {
                    throw new InvalidOperationException("Fiscal year lookup is missing fiscal start month.");
                }
                var startMonth = year.FiscalStartMonth.Value;
                if (!year.CurrentFiscalYear.HasValue)
                {
                    throw new InvalidOperationException("Fiscal year lookup is missing fiscal year.");
                }
                var fiscalYear = year.CurrentFiscalYear.Value;

                if (outDate.Month >= startMonth && outDate.Year == Convert.ToInt32(year.Id) - 1)
                {
                    fiscalYearLookup = year;
                    break;
                }
                else if (outDate.Month < startMonth && outDate.Year == Convert.ToInt32(year.Id))
                {
                    fiscalYearLookup = year;
                    break;
                }
            }

            return fiscalYearLookup;
        }

        private string ConvertDtoTypeToEntityType2(Dtos.EnumProperties.GeneralLedgerTransactionType? dtoType)
        {
            switch (dtoType)
            {
                case Dtos.EnumProperties.GeneralLedgerTransactionType.ActualOpenBalance:
                    return "AOB";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.ApprovedBudget:
                    return "AB";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.ContingentBudget:
                    return "CB";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.EncumbranceOpenBalance:
                    return "EOB";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.GeneralEncumbranceCreate:
                    return "GEC";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.MiscGeneralLedgerTransaction:
                    return "MGLT";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.TemporaryBudget:
                    return "TB";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.TemporaryBudgetAdjustment:
                    return "TBA";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.ApprovedBudgetAdjustment:
                    return "ABA";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.Donation:
                    return "DN";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.DonationEndowed:
                    return "DNE";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.Pledge:
                    return "PL";
                case Dtos.EnumProperties.GeneralLedgerTransactionType.PledgeEndowed:
                    return "PLE";
                default:
                    return "DN";
            }
        }

        private Dtos.EnumProperties.GeneralLedgerTransactionType ConverEntityTypeToDtoType2(string entityType)
        {
            switch (entityType)
            {
                case "AOB":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.ActualOpenBalance;
                case "AB":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.ApprovedBudget;
                case "CB":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.ContingentBudget;
                case "EOB":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.EncumbranceOpenBalance;
                case "GEC":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.GeneralEncumbranceCreate;
                case "MGLT":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.MiscGeneralLedgerTransaction;
                case "TB":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.TemporaryBudget;
                case "TBA":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.TemporaryBudgetAdjustment;
                case "ABA":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.ApprovedBudgetAdjustment;
                case "DN":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.Donation;
                case "DNE":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.DonationEndowed;
                case "PL":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.Pledge;
                case "PLE":
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.PledgeEndowed;
                default:
                    return Dtos.EnumProperties.GeneralLedgerTransactionType.Donation;
            }
        }

        /// <summary>
        /// validate general ledger transaction for the data model version 12
        /// </summary>
        /// <returns></returns>
        private void ValidateGeneralLedgerDto3(Dtos.GeneralLedgerTransaction3 generalLedgerDto)
        {
            decimal? creditAmount = 0;
            decimal? debitAmount = 0;
            if (generalLedgerDto == null)
            {
                throw new ArgumentNullException("generalLedgerDto", "The body of the request is required for updates.");
            }

            if (generalLedgerDto.Transactions == null || !generalLedgerDto.Transactions.Any())
            {
                throw new ArgumentNullException("generalLedgerDto.transactions", "Transactions are missing from the request.");
            }
            if (generalLedgerDto.ProcessMode != Dtos.EnumProperties.ProcessMode2.UpdateBatch &&
                generalLedgerDto.ProcessMode != Dtos.EnumProperties.ProcessMode2.Validate &&
                generalLedgerDto.ProcessMode != Dtos.EnumProperties.ProcessMode2.UpdateImmediate)
            {
                throw new ArgumentOutOfRangeException("generalLedgerDto.processMode", "Process Mode of updateBatch or validate or updateImmediate is required.");
            }
            foreach (var trans in generalLedgerDto.Transactions)
            {
                if (trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.ActualOpenBalance &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.ApprovedBudget &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.ApprovedBudgetAdjustment &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.ContingentBudget &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.EncumbranceOpenBalance &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.GeneralEncumbranceCreate &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.MiscGeneralLedgerTransaction &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.TemporaryBudget &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.TemporaryBudgetAdjustment && 
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.Donation &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.DonationEndowed &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.Pledge &&
                    trans.Type != Dtos.EnumProperties.GeneralLedgerTransactionType.PledgeEndowed)
                {
                    throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.type", "Transaction type is not supported.");
                }
                if (trans.LedgerDate == null)
                {
                    throw new ArgumentNullException("generalLedgerDto.transactions.ledgerDate", "Each transaction must have a ledgerDate assigned.");
                }
                if (trans.TransactionDetailLines == null)
                {
                    throw new ArgumentNullException("generalLedgerDto.transactions.transactionDetailLines", "Transaction Detail is required.");
                }
                foreach (var transDetail in trans.TransactionDetailLines)
                {
                    if ((trans.Type == GeneralLedgerTransactionType.EncumbranceOpenBalance) && (transDetail.Encumbrance != null) && (!string.IsNullOrEmpty(transDetail.Encumbrance.Number)))
                    {
                        throw new ArgumentException("generalLedgerDto.transactions.type", "Encumbrance cannot have value when transaction type is encumbranceOpenBalance (AE).");
                    }
                    if (string.IsNullOrEmpty(transDetail.AccountingString))
                    {
                        throw new ArgumentNullException("generalLedgerDto.transactions.transactionDetailLines.accountingString", "Each transaction detail must have an accounting string.");
                    }
                    if (string.IsNullOrEmpty(transDetail.Description))
                    {
                        throw new ArgumentNullException("generalLedgerDto.transactions.transactionDetailLines.description", "Each transaction detail must have a description.");
                    }
                    if (transDetail.Amount == null || !transDetail.Amount.Value.HasValue || transDetail.Amount.Value.Value == 0)
                    {
                        throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.amount.value", "Each transaction detail must have an amount and value.");
                    }
                    if (transDetail.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD &&
                        transDetail.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
                    {
                        throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.amount.currency", "Each transaction detail must have currency of either USD or CAD.");
                    }
                    if (transDetail.Type != Dtos.EnumProperties.CreditOrDebit.Credit &&
                        transDetail.Type != Dtos.EnumProperties.CreditOrDebit.Debit)
                    {
                        throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.type", "Each Detail item must be specified as either Debit or Credit.");
                    }
                    if (transDetail.Type == Dtos.EnumProperties.CreditOrDebit.Credit)
                    {
                        creditAmount += transDetail.Amount.Value;
                    }
                    else
                    {
                        debitAmount += transDetail.Amount.Value;
                    }
                }
            }
            if (debitAmount != creditAmount)
            {
                throw new ArgumentOutOfRangeException("generalLedgerDto.transactions.transactionDetailLines.amount.value", string.Format("Total Credits ({0}) and Debits ({1}) must be equal across all transaction detail entries.", creditAmount, debitAmount));
            }
        }

        /// <summary>
        /// Convert general ledger transaction entity to DTO for the data model version 8
        /// </summary>
        /// <param name="generalLedgerTransactionEntity">General Ledger Entity</param>
        /// /// <param name="GlConfig">General Ledger Config</param>
        /// <returns>General Ledger Transaction DTO</returns>
        private async Task<Dtos.GeneralLedgerTransaction3> ConvertGeneralLedgerTransactionEntityToDto3Async(Domain.ColleagueFinance.Entities.GeneralLedgerTransaction generalLedgerTransactionEntity, GeneralLedgerAccountStructure GlConfig, bool bypassCache)
        {
            var generalLedgerTransactionDto = new Dtos.GeneralLedgerTransaction3();

            generalLedgerTransactionDto.Id = generalLedgerTransactionEntity.Id;
            if (string.IsNullOrEmpty(generalLedgerTransactionDto.Id)) generalLedgerTransactionDto.Id = "00000000-0000-0000-0000-000000000000";

            generalLedgerTransactionDto.ProcessMode = ConvertEntityToProcessModeDto(generalLedgerTransactionEntity.ProcessMode);
            if (!string.IsNullOrEmpty(generalLedgerTransactionEntity.SubmittedBy))
            {
                var SubmittedByGuid = await personRepository.GetPersonGuidFromIdAsync(generalLedgerTransactionEntity.SubmittedBy);
                if (!string.IsNullOrEmpty(SubmittedByGuid))
                {
                    generalLedgerTransactionDto.SubmittedBy = new Dtos.GuidObject2(SubmittedByGuid);
                }
            }
            generalLedgerTransactionDto.Comment = string.IsNullOrEmpty(generalLedgerTransactionEntity.Comment)? null : generalLedgerTransactionEntity.Comment;
            generalLedgerTransactionDto.Transactions = new List<Dtos.DtoProperties.GeneralLedgerTransactionDtoProperty3>();

            foreach (var transaction in generalLedgerTransactionEntity.GeneralLedgerTransactions)
            {
                var genLedgrTransactionDto = new Dtos.DtoProperties.GeneralLedgerTransactionDtoProperty3()
                {
                    LedgerDate = transaction.LedgerDate,
                    Type = ConverEntityTypeToDtoType2(transaction.Source),
                    TransactionNumber = transaction.TransactionNumber,
                    ReferenceNumber = string.IsNullOrEmpty(transaction.ReferenceNumber)? null :transaction.ReferenceNumber,                    
                    TransactionTypeReferenceDate = transaction.TransactionTypeReferenceDate,

                    TransactionDetailLines = new List<Dtos.DtoProperties.GeneralLedgerDetailDtoProperty3>()
                };
                var person = (!string.IsNullOrEmpty(transaction.ReferencePersonId) && !(await personRepository.IsCorpAsync(transaction.ReferencePersonId)) ? new Dtos.GuidObject2(await personRepository.GetPersonGuidFromIdAsync(transaction.ReferencePersonId)) : null);
                var organization = (!string.IsNullOrEmpty(transaction.ReferencePersonId) && (await personRepository.IsCorpAsync(transaction.ReferencePersonId)) ? new Dtos.GuidObject2(await personRepository.GetPersonGuidFromIdAsync(transaction.ReferencePersonId)) : null);
                if(person != null)
                {
                    genLedgrTransactionDto.Reference = new Dtos.DtoProperties.GeneralLedgerReferenceDtoProperty() { Person = person};
                }

                if(organization != null)
                {
                    if(genLedgrTransactionDto.Reference == null)
                    {
                        genLedgrTransactionDto.Reference = new GeneralLedgerReferenceDtoProperty() { Organization = organization };
                    }
                    else
                    {
                        genLedgrTransactionDto.Reference.Organization = organization;
                    }
                }


                var budgetPeriod = transaction.BudgetPeriodDate.HasValue ? (await FiscalPeriodsAsync(bypassCache))
                    .FirstOrDefault(i => i.Month.Equals(transaction.BudgetPeriodDate.Value.Month) && i.Year.Equals(transaction.BudgetPeriodDate.Value.Year)) : null;

                foreach (var transactionDetail in transaction.TransactionDetailLines)
                {
                    string accountingString = transactionDetail.GlAccount.GetFormattedGlAccount(GlConfig.MajorComponentStartPositions);
                    accountingString = GetFormattedGlAccount(accountingString, GlConfig);

                    if (!string.IsNullOrEmpty(transactionDetail.ProjectId))
                    {
                        accountingString = ConvertAccountingstringToIncludeProjectRefNo(transactionDetail.ProjectId, accountingString);
                    }

                    var genLedgrTransactionDetailDto = new Dtos.DtoProperties.GeneralLedgerDetailDtoProperty3()
                    {
                        AccountingString = accountingString,
                        Amount = new Dtos.DtoProperties.AmountDtoProperty() { Value = transactionDetail.Amount.Value, Currency = (Dtos.EnumProperties.CurrencyCodes)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyCodes), transactionDetail.Amount.Currency.ToString()) },
                        BudgetPeriod = budgetPeriod == null ? null : new Dtos.GuidObject2(budgetPeriod.Guid),
                        Description = transactionDetail.GlAccount.GlAccountDescription,
                        SequenceNumber = transactionDetail.SequenceNumber,
                        Type = (Dtos.EnumProperties.CreditOrDebit)Enum.Parse(typeof(Dtos.EnumProperties.CreditOrDebit), transactionDetail.Type.ToString()),
                        SubmittedBy = !string.IsNullOrEmpty(transactionDetail.SubmittedBy) ? new Dtos.GuidObject2(await personRepository.GetPersonGuidFromIdAsync(transactionDetail.SubmittedBy)) : null
                    };
                    decimal giftUnits;
                    if (!string.IsNullOrEmpty(transactionDetail.EncGiftUnits) && decimal.TryParse(transactionDetail.EncGiftUnits, out giftUnits))
                    {
                        genLedgrTransactionDetailDto.GiftUnits = giftUnits;
                    }
                    else
                    {
                        genLedgrTransactionDetailDto.GiftUnits = null;
                    }

                    var encumbrance = ConvertEntityToEncumbranceDto(transactionDetail);
                    if (encumbrance != null)
                    {
                        genLedgrTransactionDetailDto.Encumbrance = encumbrance;
                    }
                    genLedgrTransactionDto.TransactionDetailLines.Add(genLedgrTransactionDetailDto);
                }
                generalLedgerTransactionDto.Transactions.Add(genLedgrTransactionDto);
            }

            return generalLedgerTransactionDto;
        }

        /// <summary>
        /// Gets accounting string with project ref no.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="accountingString"></param>
        /// <returns></returns>
        private string ConvertAccountingstringToIncludeProjectRefNo(string projectId, string accountingString)
        {
            if(_projectReferenceIds != null && _projectReferenceIds.Any())
            {
                var projectRefId = string.Empty;
                if(_projectReferenceIds.TryGetValue(projectId, out projectRefId))
                {
                    return string.Concat(accountingString, "*", projectRefId);
                }
            }
            return accountingString;
        }

        /// <summary>
        /// Converts entity to encumbrance dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Encumbrance ConvertEntityToEncumbranceDto(GenLedgrTransactionDetail source)
        {
            if (string.IsNullOrEmpty(source.EncRefNumber) && !source.EncSequenceNumber.HasValue && !source.SequenceNumber.HasValue)
            {
                return null;
            }

           var encumbrance = new Encumbrance()
            {
                Number = string.IsNullOrEmpty(source.EncRefNumber) ? null : source.EncRefNumber,
                LineItemNumber = string.IsNullOrEmpty(source.EncLineItemNumber) ? default(int?) : Convert.ToInt32(source.EncLineItemNumber),
                SequenceNumber = source.EncSequenceNumber.HasValue ? source.EncSequenceNumber.Value : default(int?),
                AdjustmentType = ConvertEntityToAdjustmentTypeDto(source.EncAdjustmentType),
                CommitmentType = ConvertEntityToCommitmentTypeDto(source.EncCommitmentType)
            };

            if ((encumbrance.Number == null) && (encumbrance.LineItemNumber == default(int?)) && (encumbrance.SequenceNumber == default(int?)) &&
                (encumbrance.AdjustmentType == null) && (encumbrance.CommitmentType == null))
                return null;
            else
                return encumbrance;
        }

        /// <summary>
        /// Converts entity to commitment type.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private CommitmentType? ConvertEntityToCommitmentTypeDto(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            switch (source.ToUpperInvariant())
            {
                case "COMMITTED":
                    return CommitmentType.Committed;
                case "UNCOMMITTED":
                    return CommitmentType.Uncommitted;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts Entity to adjustment type.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private AdjustmentType? ConvertEntityToAdjustmentTypeDto(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            switch (source.ToUpperInvariant())
            {
                case "PARTIAL":
                    return AdjustmentType.Partial;
                case "ADJUSTMENT":
                    return AdjustmentType.Adjustment;
                case "TOTAL":
                    return AdjustmentType.Total;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts to Process mode 2 for V12.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private ProcessMode2? ConvertEntityToProcessModeDto(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            switch (source.ToUpperInvariant())
            {
                case "UPDATE":
                case "UPDATEIMMEDIATE":
                case "UPDATEBATCH":
                    return ProcessMode2.UpdateImmediate;
                case "VALIDATE":
                    return ProcessMode2.Validate;
                default:
                    return ProcessMode2.UpdateImmediate;
            }
        }

        /// <summary>
        /// Fiscal Periods.
        /// </summary>
        private IEnumerable<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg> _fiscalPeriods;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg>> FiscalPeriodsAsync(bool bypassCache)
        {
            if (_fiscalPeriods == null)
            {
                _fiscalPeriods = await _referenceDataRepository.GetFiscalPeriodsIntgAsync(bypassCache);
            }
            return _fiscalPeriods;
        }

        /// <summary>
        /// Fiscal Years.
        /// </summary>
        private IEnumerable<Domain.ColleagueFinance.Entities.FiscalYear> _fiscalYears;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.FiscalYear>> FiscalYearsAsync(bool bypassCache)
        {
            if (_fiscalYears == null)
            {
                _fiscalYears = await _referenceDataRepository.GetFiscalYearsAsync(bypassCache);
            }
            return _fiscalYears;
        }

        #endregion
    }
}
