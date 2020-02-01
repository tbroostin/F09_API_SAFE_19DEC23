// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
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
    [RegisterType]
    public class AccountingStringService : BaseCoordinationService, IAccountingStringService
    {
        private readonly IAccountingStringRepository _accountingStringRepository;
        private readonly IColleagueFinanceReferenceDataRepository _colleagueFinanceReferenceDataRepository;
        private readonly IGrantsRepository _grantsRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ILogger _logger;
        private IDictionary<string, string> _pooleeGlAccts;
        private IDictionary<string, string> _grants;

        // This constructor initializes the private attributes.
        public AccountingStringService(IAccountingStringRepository accountingStringRepository, 
            IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository, IGrantsRepository grantsRepository, 
            IAdapterRegistry adapterRegistry, IConfigurationRepository configurationRepository, 
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository : configurationRepository)
        {
            _accountingStringRepository = accountingStringRepository;
            _colleagueFinanceReferenceDataRepository = colleagueFinanceReferenceDataRepository;
            _grantsRepository = grantsRepository;
            _configurationRepository = configurationRepository;
            _logger = logger;
        }

        #region Accounting String
        /// <summary>
        /// Gets accountingstring by filter criteria
        /// </summary>
        /// <param name="accountingStringValue">accounting string to filter by</param>
        /// <param name="validOn">date to check if valid on</param>
        /// <returns>accounting string if found and valid</returns>
        public async Task<Dtos.AccountingString> GetAccoutingStringByFilterCriteriaAsync(string accountingStringValue, DateTime? validOn = null)
        {
            try
            {
                CheckUserAccountingStringsViewPermissions();

                //default accountingString to what just came in
                var accountingString = string.Empty;
                //default project number to empty
                string projectNumber = string.Empty;

                //check if the incoming string contains a * which is the expected delimeter for the split between account string and project number
                if (accountingStringValue.Contains("*"))
                {
                    var accountNumberWithProject = accountingStringValue.Split("*".ToCharArray());
                    accountingString = accountNumberWithProject[0].ToString().Replace("-", "_");
                    projectNumber = accountNumberWithProject[1].ToString();
                }
                else
                {
                    accountingString = accountingStringValue.Replace("-", "_"); 
                }

                //create accountingString entity with transformed input filters
                var accountingStringEntity = new Domain.ColleagueFinance.Entities.AccountingString(accountingString, projectNumber, validOn);

                //run validation method from repository, will return the entity if validation succeeded, will error if fails
                var validationResult = await _accountingStringRepository.GetValidAccountingString(accountingStringEntity);

                return new Dtos.AccountingString()
                {
                    AccountingStringValue = accountingStringValue,
                    Description = validationResult.Description
                };
            }
            catch (RepositoryException repositoryException)
            {
                throw repositoryException;
            }
            catch (ArgumentNullException argumentNullException)
            {
                throw argumentNullException;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unexpected Error in AccountingString Service");
                throw exception;
            }
        }


        /// <summary>
        /// Provides an integration user permission to view/get holds (a.k.a. restrictions) from Colleague.
        /// </summary>
        private void CheckUserAccountingStringsViewPermissions()
        {
            // access is ok if the current user has the view accounting strings permission
            if (!HasPermission(ColleagueFinancePermissionCodes.ViewAccountingStrings))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view accounting strings.");
                throw new PermissionsException("User is not authorized to view accounting strings.");
            }
        }

        #endregion

        #region Accounting String Components

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Gets all accounting-string-components
        /// </summary>
        /// <returns>Collection of AccountingStringComponents DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponent>> GetAccountingStringComponentsAsync(bool bypassCache = false)
        {
            var accountingStringComponentsCollection = new List<Ellucian.Colleague.Dtos.AccountingStringComponent>();

            var accountingStringComponentsEntities = await _colleagueFinanceReferenceDataRepository.GetAccountComponentsAsync(bypassCache);
            if (accountingStringComponentsEntities != null && accountingStringComponentsEntities.Any())
            {
                foreach (var accountingStringComponents in accountingStringComponentsEntities)
                {
                    accountingStringComponentsCollection.Add(ConvertAccountComponentsEntityToDto(accountingStringComponents));
                }
            }
            return accountingStringComponentsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get a AccountingStringComponents from its GUID
        /// </summary>
        /// <returns>AccountingStringComponents DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AccountingStringComponent> GetAccountingStringComponentsByGuidAsync(string guid)
        {
            try
            {
                return ConvertAccountComponentsEntityToDto((await _colleagueFinanceReferenceDataRepository.GetAccountComponentsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("accounting-string-components not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("accounting-string-components not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Converts a AccountComponents domain entity to its corresponding AccountingStringComponents DTO
        /// </summary>
        /// <param name="source">AccountComponents domain entity</param>
        /// <returns>AccountingStringComponent DTO</returns>
        private Ellucian.Colleague.Dtos.AccountingStringComponent ConvertAccountComponentsEntityToDto(AccountComponents source)
        {
            var accountingStringComponents = new Ellucian.Colleague.Dtos.AccountingStringComponent
            {
                Id = source.Guid,
                Code = source.Code,
                Title = source.Description,
                Description = null
            };

            return accountingStringComponents;
        }
        #endregion

        #region Accounting String Formats

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8</remarks>
        /// <summary>
        /// Gets all accounting-string-formats
        /// </summary>
        /// <returns>Collection of AccountingStringFormats DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringFormats>> GetAccountingStringFormatsAsync(bool bypassCache = false)
        {
            var accountingStringFormatsCollection = new List<Ellucian.Colleague.Dtos.AccountingStringFormats>();

            var accountingStringFormatsEntities = await _colleagueFinanceReferenceDataRepository.GetAccountFormatsAsync(bypassCache);

            var accountingComponents = await _colleagueFinanceReferenceDataRepository.GetAccountComponentsAsync(true);

            if (accountingStringFormatsEntities != null && accountingStringFormatsEntities.Any())
            {
                foreach (var accountingStringFormats in accountingStringFormatsEntities)
                {
                    accountingStringFormatsCollection.Add(ConvertAccountingStringFormatsEntityToDto(accountingStringFormats, accountingComponents));
                }
            }
            return accountingStringFormatsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a AccountingStringFormats from its GUID
        /// </summary>
        /// <returns>AccountingStringFormats DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AccountingStringFormats> GetAccountingStringFormatsByGuidAsync(string guid)
        {
            try
            {
                var accountingComponents = await _colleagueFinanceReferenceDataRepository.GetAccountComponentsAsync(true);
                var asf = (await _colleagueFinanceReferenceDataRepository.GetAccountFormatsAsync(true)).Where(r => r.Guid == guid).First();
                return ConvertAccountingStringFormatsEntityToDto(asf, accountingComponents);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("accounting-string-formats not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("accounting-string-formats not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AccountingStringFormats domain entity to its corresponding AccountingStringFormats DTO
        /// </summary>
        /// <param name="source">AccountingStringFormats domain entity</param>
        /// <returns>AccountingStringFormats DTO</returns>
        private Ellucian.Colleague.Dtos.AccountingStringFormats ConvertAccountingStringFormatsEntityToDto(AccountingFormat source, IEnumerable<AccountComponents> accountingComponents)
        {
            var accountingStringFormats = new Ellucian.Colleague.Dtos.AccountingStringFormats();

            accountingStringFormats.Id = source.Guid;
            accountingStringFormats.Delimiter = "*";

            accountingStringFormats.Components = new List<Components>();

            int x = 1;
            foreach(var ac in accountingComponents)
            {
                var c = new Dtos.Components() {Component = new GuidObject2(ac.Guid), order = x };
                accountingStringFormats.Components.Add(c);
                x++;
            }

            return accountingStringFormats;
        }


        #endregion

        #region Accounting string components values

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all accounting-string-component-values
        /// </summary>
        /// <returns>Collection of AccountingStringComponentValues DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponentValues>, int>> GetAccountingStringComponentValuesAsync(int offset, int limit, 
            string component, string transactionStatus, string typeAccount, string typeFund,bool bypassCache = false)
        {
            await CheckAccountingStringComponentValuesViewPermission();

            var accountingStringComponentValuesCollection = new List<Ellucian.Colleague.Dtos.AccountingStringComponentValues>();

            if (!string.IsNullOrEmpty(typeFund))
            {
                return new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(accountingStringComponentValuesCollection, 0);
            }

            var accountingComponents = await _colleagueFinanceReferenceDataRepository.GetAccountComponentsAsync(bypassCache);

            string guidComponent = string.Empty;
            if (!string.IsNullOrEmpty(component))
            {
                try
                {
                    guidComponent = accountingComponents.FirstOrDefault(x => x.Guid == component).Code;
                }
                catch (Exception e)
                {
                    return new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(accountingStringComponentValuesCollection, 0);
                }                
            }

            var accountingStringComponentValuesEntities = await _colleagueFinanceReferenceDataRepository.GetAccountingStringComponentValues2Async(
                offset, limit, guidComponent, transactionStatus, typeAccount);

            if (accountingStringComponentValuesEntities.Item1 != null && accountingStringComponentValuesEntities.Item1.Any())
            {
                foreach (var accountingStringComponentValues in accountingStringComponentValuesEntities.Item1)
                {
                    accountingStringComponentValuesCollection.Add(ConvertAccountingStringComponentValuesEntityToDto(accountingStringComponentValues, accountingComponents));
                }
            }
            return new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(accountingStringComponentValuesCollection, accountingStringComponentValuesEntities.Item2);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all accounting-string-component-values
        /// </summary>
        /// <returns>Collection of AccountingStringComponentValues DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponentValues2>, int>> GetAccountingStringComponentValues2Async(int offset, int limit, string component, string transactionStatus, string typeAccount, string typeFund, bool bypassCache = false)
        {
            await CheckAccountingStringComponentValuesViewPermission();

            var accountingStringComponentValuesCollection = new List<Ellucian.Colleague.Dtos.AccountingStringComponentValues2>();

            if (!string.IsNullOrEmpty(typeFund))
            {
                return new Tuple<IEnumerable<Dtos.AccountingStringComponentValues2>, int>(accountingStringComponentValuesCollection, 0);
            }

            var accountingComponents = await _colleagueFinanceReferenceDataRepository.GetAccountComponentsAsync(bypassCache);

            string guidComponent = string.Empty;
            if (!string.IsNullOrEmpty(component))
            {
                var acctComponent = accountingComponents.FirstOrDefault(x => x.Guid == component);
                if(acctComponent == null)
                {
                    return new Tuple<IEnumerable<Dtos.AccountingStringComponentValues2>, int>(accountingStringComponentValuesCollection, 0);
                }
                guidComponent = acctComponent.Code;
            }

            var accountingStringComponentValuesEntities = await _colleagueFinanceReferenceDataRepository.GetAccountingStringComponentValues2Async(
                offset, limit, guidComponent, transactionStatus, typeAccount);

            if (accountingStringComponentValuesEntities.Item1 != null && accountingStringComponentValuesEntities.Item1.Any())
            {
                var glAccts = accountingStringComponentValuesEntities.Item1
                    .Where(i => i.PooleeAccounts != null && i.PooleeAccounts.Keys != null && i.PooleeAccounts.Keys.Any())
                    .SelectMany(s => s.PooleeAccounts.Values).Distinct().ToList();
                 _pooleeGlAccts = await _colleagueFinanceReferenceDataRepository.GetGuidsForPooleeGLAcctsInFiscalYearsAsync(glAccts);

                foreach (var accountingStringComponentValues in accountingStringComponentValuesEntities.Item1)
                {
                    accountingStringComponentValuesCollection.Add(await ConvertAccountingStringComponentValuesEntityToDto2(accountingStringComponentValues, accountingComponents, bypassCache));
                }
            }
            return new Tuple<IEnumerable<Dtos.AccountingStringComponentValues2>, int>(accountingStringComponentValuesCollection, accountingStringComponentValuesEntities.Item2);
        }

        

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a AccountingStringComponentValues from its GUID
        /// </summary>
        /// <returns>AccountingStringComponentValues DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AccountingStringComponentValues> GetAccountingStringComponentValuesByGuidAsync(string guid)
        {
            try
            {
                await CheckAccountingStringComponentValuesViewPermission();
                var accountingComponents = await _colleagueFinanceReferenceDataRepository.GetAccountComponentsAsync(true);
                return ConvertAccountingStringComponentValuesEntityToDto((await _colleagueFinanceReferenceDataRepository.GetAccountingStringComponentValueByGuid(guid)), accountingComponents);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("accounting-string-component-values not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("accounting-string-component-values not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a AccountingStringComponentValues from its GUID
        /// </summary>
        /// <returns>AccountingStringComponentValues DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AccountingStringComponentValues2> GetAccountingStringComponentValues2ByGuidAsync(string guid, bool bypassCache)
        {
            try
            {
                await CheckAccountingStringComponentValuesViewPermission();
                var accountingComponents = await _colleagueFinanceReferenceDataRepository.GetAccountComponentsAsync(bypassCache);
                var ascv = await _colleagueFinanceReferenceDataRepository.GetAccountingStringComponentValue2ByGuid(guid);

                var glAccts = ascv.PooleeAccounts!= null? ascv.PooleeAccounts : new Dictionary<string, string>();
                _pooleeGlAccts = await _colleagueFinanceReferenceDataRepository.GetGuidsForPooleeGLAcctsInFiscalYearsAsync(glAccts.Values);

                return await ConvertAccountingStringComponentValuesEntityToDto2(ascv, accountingComponents, bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("accounting-string-component-values not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("accounting-string-component-values not found for GUID " + guid, ex);
            }
        }        
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AccountingStringComponentValues domain entity to its corresponding AccountingStringComponentValues DTO
        /// </summary>
        /// <param name="source">AccountingStringComponentValues domain entity</param>
        /// <returns>AccountingStringComponentValues DTO</returns>
        private Ellucian.Colleague.Dtos.AccountingStringComponentValues ConvertAccountingStringComponentValuesEntityToDto(Domain.ColleagueFinance.Entities.AccountingStringComponentValues source, IEnumerable<AccountComponents> accountingComponents)
        {
            var accountingStringComponentValues = new Ellucian.Colleague.Dtos.AccountingStringComponentValues();

            accountingStringComponentValues.Id = source.Guid;
            var accountingString = source.AccountNumber.Replace("_", "-");
            accountingStringComponentValues.Value = accountingString;
            accountingStringComponentValues.Description = source.Description;

            switch (source.AccountDef)
            {
                case "GL":
                    accountingStringComponentValues.Component = new GuidObject2(accountingComponents.FirstOrDefault(x => x.Code == "GL.ACCT").Guid);
                    break;
                case "Project":
                    accountingStringComponentValues.Component = new GuidObject2(accountingComponents.FirstOrDefault(x => x.Code == "PROJECT").Guid.ToString());
                    break;
            }
            
            switch (source.Status)
            {
                case "available":
                    accountingStringComponentValues.TransactionStatus = Dtos.EnumProperties.AccountingTransactionStatus.available;
                    break;
                case "unavailable":
                    accountingStringComponentValues.TransactionStatus = Dtos.EnumProperties.AccountingTransactionStatus.unavailable;
                    break;
            }

            accountingStringComponentValues.Type = new AccountingStringComponentValuesType();
            switch (source.Type)
            {
                case "asset":
                    accountingStringComponentValues.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.asset;
                    break;
                case "liability":
                    accountingStringComponentValues.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.liability;
                    break;
                case "fundBalance":
                    accountingStringComponentValues.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.fundBalance;
                    break;
                case "revenue":
                    accountingStringComponentValues.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.revenue;
                    break;
                case "expense":
                    accountingStringComponentValues.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.expense;
                    break;
            }

            return accountingStringComponentValues;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AccountingStringComponentValues domain entity to its corresponding AccountingStringComponentValues DTO
        /// </summary>
        /// <param name="source">AccountingStringComponentValues domain entity</param>
        /// <returns>AccountingStringComponentValues DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.AccountingStringComponentValues2> ConvertAccountingStringComponentValuesEntityToDto2(Domain.ColleagueFinance.Entities.AccountingStringComponentValues source, IEnumerable<AccountComponents> accountingComponents, bool bypassCache)
        {
            var accountingStringComponentValues = new Ellucian.Colleague.Dtos.AccountingStringComponentValues2();

            accountingStringComponentValues.Id = source.Guid;
            var accountingString = source.AccountNumber.Replace("_", "-");
            accountingStringComponentValues.Value = accountingString;
            accountingStringComponentValues.Description = source.Description;
            accountingStringComponentValues.BudgetPools = await ConvertEntityToBudgetPoolDtoAsync(source.PooleeAccounts, bypassCache);

            switch (source.AccountDef)
            {
                case "GL":
                    accountingStringComponentValues.Component = new GuidObject2(accountingComponents.FirstOrDefault(x => x.Code == "GL.ACCT").Guid);
                    break;
                case "Project":
                    accountingStringComponentValues.Component = new GuidObject2(accountingComponents.FirstOrDefault(x => x.Code == "PROJECT").Guid.ToString());
                    break;
            }

            switch (source.Status)
            {
                case "available":
                    accountingStringComponentValues.TransactionStatus = Dtos.EnumProperties.AccountingTransactionStatus.available;
                    break;
                case "unavailable":
                    accountingStringComponentValues.TransactionStatus = Dtos.EnumProperties.AccountingTransactionStatus.unavailable;
                    break;
            }

            accountingStringComponentValues.Type = new AccountingStringComponentValuesType();
            switch (source.Type)
            {
                case "asset":
                    accountingStringComponentValues.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.asset;
                    break;
                case "liability":
                    accountingStringComponentValues.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.liability;
                    break;
                case "fundBalance":
                    accountingStringComponentValues.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.fundBalance;
                    break;
                case "revenue":
                    accountingStringComponentValues.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.revenue;
                    break;
                case "expense":
                    accountingStringComponentValues.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.expense;
                    break;
            }

            return accountingStringComponentValues;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 15</remarks>
        /// <summary>
        /// Gets all accounting-string-component-values
        /// </summary>
        /// <returns>Collection of AccountingStringComponentValues DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponentValues3>, int>> GetAccountingStringComponentValues3Async(int offset, int limit,
            Dtos.AccountingStringComponentValues3 criteriaValues = null, DateTime? effectiveOn = null, bool bypassCache = false)
        {
            await CheckAccountingStringComponentValuesViewPermission();

            string component = string.Empty, transactionStatus = string.Empty, typeAccount = string.Empty, typeFund = string.Empty, status = string.Empty;
            List<string> grants = null;

            if (criteriaValues != null)
            {
                component = criteriaValues.Component != null && !string.IsNullOrEmpty(criteriaValues.Component.Id)
                        ? criteriaValues.Component.Id : string.Empty;
                //status: "active" or "inactive" (see property above for defining logic) (this filter's logic was formerly on transactionStatus)
                Dtos.EnumProperties.Status statusEnum = criteriaValues.Status;
                if (statusEnum != Dtos.EnumProperties.Status.NotSet)
                {
                    if (statusEnum == Dtos.EnumProperties.Status.Active)
                    {
                        status = "available";
                    }
                    else if (statusEnum == Dtos.EnumProperties.Status.Inactive)
                    {
                        //Colleague always returns 'available'. So if they query for 'available', return all records / ignore the filter. 
                        //If they query for 'unavailable', return an empty set.
                        return new Tuple<IEnumerable<Dtos.AccountingStringComponentValues3>, int>(new List<Ellucian.Colleague.Dtos.AccountingStringComponentValues3>(), 0);
                    }
                }
                
                typeAccount = criteriaValues.Type != null && criteriaValues.Type.Account != null ?
                   criteriaValues.Type.Account.ToString() : string.Empty;

                typeFund = criteriaValues.Type != null && criteriaValues.Type.Fund != null ? criteriaValues.Type.Fund : string.Empty;

                //grants
                if (criteriaValues.Grants != null && criteriaValues.Grants.Any())
                {
                    var grantIds = criteriaValues.Grants.Select(id => id.Id);
                    var grantKeys = await _grantsRepository.GetProjectCFIdsAsync(grantIds.Distinct().ToArray());
                    if (grantKeys == null || !grantKeys.Any())
                    {
                        return new Tuple<IEnumerable<Dtos.AccountingStringComponentValues3>, int>(new List<Ellucian.Colleague.Dtos.AccountingStringComponentValues3>(), 0);
                    }

                    if (grantKeys != null && grantKeys.Any())
                    {
                        grants = new List<string>();
                        grantKeys.ToList().ForEach(id =>
                        {
                            grants.Add(id);
                        });
                    }
                }
            }

            var accountingStringComponentValuesCollection = new List<Ellucian.Colleague.Dtos.AccountingStringComponentValues3>();

            //This concept does not apply to Colleague so filtering on it cannot match any records. Therefore, return an empty set if it's used.
            if (!string.IsNullOrEmpty(typeFund))
            {
                return new Tuple<IEnumerable<Dtos.AccountingStringComponentValues3>, int>(accountingStringComponentValuesCollection, 0);
            }

            IEnumerable<AccountComponents> accountingComponents = await AccountingComponentsAsync(bypassCache);
            string guidComponent = string.Empty;
            if (!string.IsNullOrEmpty(component))
            {
                var acctComponent = accountingComponents.FirstOrDefault(x => x.Guid == component);
                if (acctComponent == null)
                {
                    return new Tuple<IEnumerable<Dtos.AccountingStringComponentValues3>, int>(accountingStringComponentValuesCollection, 0);
                }
                guidComponent = acctComponent.Code;
            }

            var accountingStringComponentValuesEntities = await _colleagueFinanceReferenceDataRepository.GetAccountingStringComponentValues3Async(
                offset, limit, guidComponent, typeAccount, status, grants, effectiveOn);

            if (accountingStringComponentValuesEntities != null && accountingStringComponentValuesEntities.Item1 != null && accountingStringComponentValuesEntities.Item1.Any())
            {
                var glAccts = accountingStringComponentValuesEntities.Item1
                                .Where(i => i.PooleeAccounts != null && i.PooleeAccounts.Keys != null && i.PooleeAccounts.Keys.Any())
                                .SelectMany(s => s.PooleeAccounts.Values).Distinct().ToList();
                _pooleeGlAccts = await _colleagueFinanceReferenceDataRepository.GetGuidsForPooleeGLAcctsInFiscalYearsAsync(glAccts);

                var grantIds = accountingStringComponentValuesEntities.Item1.SelectMany(g => g.GrantIds);
                _grants = await _grantsRepository.GetProjectCFGuidsAsync(grantIds.Distinct().ToArray());

                _acctStructIntg = await GetAcctStructureIntgAsync(bypassCache);
                _acctSubComponentValues = await GetAccountingStringSubcomponentValuesAsync(bypassCache);

                foreach (var accountingStringComponentValues in accountingStringComponentValuesEntities.Item1)
                {
                    accountingStringComponentValuesCollection.Add(await ConvertAccountingStringComponentValuesEntityToDto3(accountingStringComponentValues, accountingComponents, bypassCache));
                }
            }
            return accountingStringComponentValuesCollection != null && accountingStringComponentValuesCollection.Any() ? 
                new Tuple<IEnumerable<Dtos.AccountingStringComponentValues3>, int>(accountingStringComponentValuesCollection, accountingStringComponentValuesEntities.Item2) :
                new Tuple<IEnumerable<AccountingStringComponentValues3>, int>(new List<Dtos.AccountingStringComponentValues3>(), 0);
        }

        /// <summary>
        /// Gets accounting string component value by guid.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<AccountingStringComponentValues3> GetAccountingStringComponentValues3ByGuidAsync(string id, bool bypassCache)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentNullException("Id is required.");
                }

                await CheckAccountingStringComponentValuesViewPermission();
                var accountingComponents = await _colleagueFinanceReferenceDataRepository.GetAccountComponentsAsync(bypassCache);
                var ascv = await _colleagueFinanceReferenceDataRepository.GetAccountingStringComponentValue3ByGuid(id);

                var glAccts = ascv.PooleeAccounts != null ? ascv.PooleeAccounts : new Dictionary<string, string>();
                _pooleeGlAccts = await _colleagueFinanceReferenceDataRepository.GetGuidsForPooleeGLAcctsInFiscalYearsAsync(glAccts.Values);

                var grantIds = ascv.GrantIds;
                _grants = await _grantsRepository.GetProjectCFGuidsAsync(grantIds.Distinct().ToArray());

                _acctStructIntg = await GetAcctStructureIntgAsync(bypassCache);
                _acctSubComponentValues = await GetAccountingStringSubcomponentValuesAsync(bypassCache);

                return await ConvertAccountingStringComponentValuesEntityToDto3(ascv, accountingComponents, bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("accounting-string-component-values not found for GUID " + id, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("accounting-string-component-values not found for GUID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AccountingStringComponentValues domain entity to its corresponding AccountingStringComponentValues DTO
        /// </summary>
        /// <param name="source">AccountingStringComponentValues domain entity</param>
        /// <returns>AccountingStringComponentValues DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.AccountingStringComponentValues3> ConvertAccountingStringComponentValuesEntityToDto3(Domain.ColleagueFinance.Entities.AccountingStringComponentValues source, IEnumerable<AccountComponents> accountingComponents, bool bypassCache)
        {
            var ascv = new Ellucian.Colleague.Dtos.AccountingStringComponentValues3();

            ascv.Id = source.Guid;
            var accountingString = source.AccountNumber.Replace("_", "-");
            ascv.Value = accountingString;
            ascv.Description = source.Description;
            ascv.BudgetPools = await ConvertEntityToBudgetPoolDtoAsync(source.PooleeAccounts, bypassCache);
            ascv.EffectiveStartOn = source.StartDate.HasValue ? source.StartDate.Value : default(DateTime?);
            ascv.EffectiveEndOn = source.EndDate.HasValue ? source.EndDate.Value : default(DateTime?);


            switch (source.AccountDef)
            {
                case "GL":
                    ascv.Component = new GuidObject2(accountingComponents.FirstOrDefault(x => x.Code == "GL.ACCT").Guid);
                    break;
                case "Project":
                    ascv.Component = new GuidObject2(accountingComponents.FirstOrDefault(x => x.Code == "PROJECT").Guid.ToString());
                    break;
            }

            //subcomponent
            var acctIntgStructs = await GetAcctStructureIntgAsync(bypassCache);
            var subComponents = await GetAccountingStringSubcomponentValuesAsync(bypassCache);
            if (acctIntgStructs != null && acctIntgStructs.Any() && source.AccountDef.Equals("GL", StringComparison.OrdinalIgnoreCase))
            {
                List<GuidObject2> tempGuidObjList = new List<GuidObject2>();
                foreach (var acctIntgStruct in acctIntgStructs)
                {
                    int tempStart = 0, tempLength = 0;
                    if (int.TryParse((Convert.ToInt32(acctIntgStruct.StartPosition.Value) - 1).ToString(), out tempStart))
                    {
                        tempLength = Convert.ToInt32(acctIntgStruct.Length);
                        var compValue = source.Id.Substring(tempStart, tempLength);
                        var subComponent = subComponents.Where(sub => sub.Type.Equals(acctIntgStruct.Type) && sub.Code.Equals(compValue, StringComparison.OrdinalIgnoreCase));
                        var guids = subComponent.Select(sub => sub.Guid).ToList();

                        guids.Distinct().ToList().ForEach(sub =>
                        {
                            tempGuidObjList.Add(new GuidObject2(sub));
                        });
                    }
                }
                if (tempGuidObjList.Any())
                {
                    ascv.SubComponents = tempGuidObjList;
                }
            }
            
            /*
                 Always return "available". TransactionStatus is a Banner concept that means "this FOAPAL component exists only for reporting purposes (or similar) 
                 but can never be used on a transaction". Colleague doesn't have an equivalent concept. As of v15, the model now has a separate 'status' property 
                 that reflects the way Colleague previously used this field. So move the former logic to 'status' and always return transactionStatus = 'available'.
            */
            ascv.TransactionStatus = Dtos.EnumProperties.AccountingTransactionStatus.available;

            switch (source.Status)
            {
                case "available":
                    ascv.Status = Dtos.EnumProperties.Status.Active;
                    break;
                case "unavailable":
                    ascv.Status = Dtos.EnumProperties.Status.Inactive;
                    break;
            }

            ascv.Type = new AccountingStringComponentValuesType();
            switch (source.Type)
            {
                case "asset":
                    ascv.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.asset;
                    break;
                case "liability":
                    ascv.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.liability;
                    break;
                case "fundBalance":
                    ascv.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.fundBalance;
                    break;
                case "revenue":
                    ascv.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.revenue;
                    break;
                case "expense":
                    ascv.Type.Account = Dtos.EnumProperties.AccountingTypeAccount.expense;
                    break;
            }

            //Grants
            if (_grants != null && _grants.Any() && source.GrantIds.Any())
            {
                List<GuidObject2> grantGuidObjs = new List<GuidObject2>();
                var grantGuids = _grants.Where(k => source.GrantIds.Contains(k.Key)).Select(v => v.Value);
                if(grantGuids != null && grantGuids.Any())
                {
                    grantGuids.ToList().ForEach(g => 
                    {
                        grantGuidObjs.Add(new GuidObject2(g));
                    });
                    ascv.Grants = grantGuidObjs;
                }
            }

            return ascv;
        }

        /// <summary>
        /// Converts entity to budget pool dtos.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Dtos.DtoProperties.BudgetPool>> ConvertEntityToBudgetPoolDtoAsync(IDictionary<string, string> source, bool bypassCache)
        {
            List<Dtos.DtoProperties.BudgetPool> budgetPools = new List<Dtos.DtoProperties.BudgetPool>();
            if (source != null && source.Any())
            {
                foreach (var item in source)
                {
                    Dtos.DtoProperties.BudgetPool budgPool = new BudgetPool()
                    {
                        FiscalYear = await ConvertEntityToFiscalYearGuidObjectDtoAsync(item.Key, bypassCache),
                        AccountingComponent = ConvertEntityToGlAcctGuidObject(item.Value)
                    };
                    budgetPools.Add(budgPool);
                }
            }

            return budgetPools.Any() ? budgetPools : null;
        }

        /// <summary>
        /// Converts entity to fiscal year guid object.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertEntityToFiscalYearGuidObjectDtoAsync(string source, bool bypassCache)
        {
            var year = (await FiscalYearsAsync(bypassCache)).FirstOrDefault(i => i.Id.Equals(source, StringComparison.OrdinalIgnoreCase));
            if(year == null)
            {
                throw new KeyNotFoundException(string.Format("Fiscal year not found for id {0}.", source));
            }
            return new GuidObject2(year.Guid);
        }

        /// <summary>
        /// Converts entity to gl account dto guid object.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private GuidObject2 ConvertEntityToGlAcctGuidObject(string source)
        {
            if(!string.IsNullOrEmpty(source))
            {
                var guid = _pooleeGlAccts.FirstOrDefault(i => i.Key.Equals(source, StringComparison.OrdinalIgnoreCase));
                if (guid.Equals(default(KeyValuePair<string, string>)))
                {
                    throw new KeyNotFoundException(string.Format("Accounting string component value not found for id {0}.", source));
                }

                return new GuidObject2(guid.Value);
            }
            return null;
        }


        /// <summary>
        /// Gets accounting components.
        /// </summary>
        IEnumerable<AccountComponents> _accountingComponents = null;
        private async Task<IEnumerable<AccountComponents>> AccountingComponentsAsync(bool bypassCache)
        {
            if (_accountingComponents == null)
            {
                _accountingComponents = await _colleagueFinanceReferenceDataRepository.GetAccountComponentsAsync(bypassCache);
            }
            return _accountingComponents;
        }

        /// <summary>
        /// Checks persmissions.
        /// </summary>
        /// <param name="generalLedgerDto"></param>
        private async Task CheckAccountingStringComponentValuesViewPermission()
        {
            var userPermissionList = (await GetUserPermissionCodesAsync()).ToList();

            // This is the overall permission code needed to create anything with this API.
            if (!userPermissionList.Contains(ColleagueFinancePermissionCodes.ViewAccountingStrings))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view accounting string component values.");
                throw new PermissionsException("User is not authorized to view accounting string component values.");
            }
        }

        /// <summary>
        /// Accounting structure values.
        /// </summary>
        private IEnumerable<AcctStructureIntg> _acctStructIntg;
        private async Task<IEnumerable<AcctStructureIntg>> GetAcctStructureIntgAsync(bool bypassCache)
        {
            if (_acctStructIntg == null)
            { 
                _acctStructIntg = await _colleagueFinanceReferenceDataRepository.GetAcctStructureIntgAsync(bypassCache);
                return _acctStructIntg;
            }
            return _acctStructIntg;
        }

        /// <summary>
        /// Gets accounting subcomponents.
        /// </summary>
        IEnumerable<Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues> _acctSubComponentValues;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetAccountingStringSubcomponentValuesAsync(bool bypassCache)
        {
            if (_acctSubComponentValues == null)
            {
                _acctSubComponentValues = await _colleagueFinanceReferenceDataRepository.GetAllAccountingStringSubcomponentValuesAsync(bypassCache);
                return _acctSubComponentValues;
            }
            return _acctSubComponentValues;
        }

        /// <summary>
        /// Gets fiscal year entities.
        /// </summary>
        IEnumerable<FiscalYear> _fiscalYears;
        private async Task<IEnumerable<FiscalYear>> FiscalYearsAsync(bool bypassCache)
        {
            if (_fiscalYears == null)
            {
                _fiscalYears = await _colleagueFinanceReferenceDataRepository.GetFiscalYearsAsync(bypassCache);
            }
            return _fiscalYears;
        }

        #endregion Accounting string components values
    }
}