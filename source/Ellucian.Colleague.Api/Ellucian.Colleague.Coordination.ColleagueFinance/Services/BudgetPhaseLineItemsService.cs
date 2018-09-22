//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class BudgetPhaseLineItemsService : BaseCoordinationService, IBudgetPhaseLineItemsService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public BudgetPhaseLineItemsService(
            IBudgetRepository budgetRepository,
            IColleagueFinanceReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _budgetRepository = budgetRepository;
            _referenceDataRepository = referenceDataRepository;
        }


        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.BudgetPhaseLineItems>, int>> GetBudgetPhaseLineItemsAsync(int limit, int offset, string budgetPhaseGuid,
            List<string> accountingStringComponentValues, bool bypassCache = false)
        {
            CheckViewBudgetPhaseLineItemsPermission();
            var budgetPhasesCollection = new List<Ellucian.Colleague.Dtos.BudgetPhases>();
            var budgetPhaseLineItemsCollection = new List<Ellucian.Colleague.Dtos.BudgetPhaseLineItems>();
            var budgetPhase = string.Empty;
            var accountingStringComponentValueId = string.Empty;
            if (!string.IsNullOrEmpty(budgetPhaseGuid))
            {
                try
                {
                    budgetPhase = await _budgetRepository.GetBudgetPhasesIdFromGuidAsync(budgetPhaseGuid);
                }
                catch (KeyNotFoundException)
                {
                    return new Tuple<IEnumerable<Dtos.BudgetPhaseLineItems>, int>(new List<Dtos.BudgetPhaseLineItems>(), 0);
                }
            }

            if ((accountingStringComponentValues != null) && (accountingStringComponentValues.Any()))
            {
                try
                {
                    if (accountingStringComponentValues.Count > 1)
                    {
                        return new Tuple<IEnumerable<Dtos.BudgetPhaseLineItems>, int>(new List<Dtos.BudgetPhaseLineItems>(), 0);
                    }
                    var accountingStringComponentValue = await _referenceDataRepository.GetAccountingStringComponentValueByGuid(accountingStringComponentValues[0]);
                    if (accountingStringComponentValue == null)
                    {
                        return new Tuple<IEnumerable<Dtos.BudgetPhaseLineItems>, int>(new List<Dtos.BudgetPhaseLineItems>(), 0);
                    }
                    accountingStringComponentValueId = accountingStringComponentValue.AccountNumber;
                }
                catch (KeyNotFoundException)
                {
                    return new Tuple<IEnumerable<Dtos.BudgetPhaseLineItems>, int>(new List<Dtos.BudgetPhaseLineItems>(), 0);
                }
            }

            var budgetPhasesEntitiesTuple = await _budgetRepository.GetBudgetPhaseLineItemsAsync(limit, offset, 
                budgetPhase, accountingStringComponentValueId,  bypassCache);

            var budgetWorkEntities = budgetPhasesEntitiesTuple.Item1;
            var totalCount = budgetPhasesEntitiesTuple.Item2;

            if (budgetWorkEntities != null)
            {
                foreach (var budgetWork in budgetWorkEntities)
                {
                    budgetPhaseLineItemsCollection.Add(await ConvertBudgetWorkEntityToDtoAsync(budgetWork, bypassCache));
                }
            }
            return  new Tuple<IEnumerable<Dtos.BudgetPhaseLineItems>, int>(budgetPhaseLineItemsCollection, totalCount);

        }

        public async Task<BudgetPhaseLineItems> GetBudgetPhaseLineItemsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                CheckViewBudgetPhaseLineItemsPermission();
                return await ConvertBudgetWorkEntityToDtoAsync(await _budgetRepository.GetBudgetPhaseLineItemsByGuidAsync(guid), bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No budget phase line item was found for guid " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No budget phase line item was found for guid " + guid, ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }


        private Dictionary<string, string>  budgetPhaseDict = new Dictionary<string, string>();
        private Dictionary<string, string> accountingStringDict = new Dictionary<string, string>();
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a BudWork domain entity to its corresponding BudgetPhaseLineItems DTO
        /// </summary>
        /// <param name="source">Budget domain entity</param>
        /// <returns>BudgetPhases DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.BudgetPhaseLineItems> ConvertBudgetWorkEntityToDtoAsync(BudgetWork source,
            bool bypassCache = false)
        {
            var budgetPhaseLineItem = new Ellucian.Colleague.Dtos.BudgetPhaseLineItems();

            budgetPhaseLineItem.Id = source.RecordGuid;

            if (!string.IsNullOrEmpty(source.BudgetPhase))
            {
                var budgetPhasesGuid = string.Empty;
                if (!budgetPhaseDict.TryGetValue(source.BudgetPhase, out budgetPhasesGuid))
                {
                    budgetPhasesGuid = await _budgetRepository.GetBudgetPhasesGuidFromIdAsync(source.BudgetPhase);
                    if (string.IsNullOrEmpty(budgetPhasesGuid))
                    {
                        throw new ArgumentNullException("BudgetPhase.Id", string.Concat("Unable to determine guid for BudgetPhase: ", source.BudgetPhase));
                    }
                    budgetPhaseDict.Add(source.BudgetPhase, budgetPhasesGuid);
                }

                budgetPhaseLineItem.BudgetPhase = new GuidObject2(budgetPhasesGuid);
            }

            if (!string.IsNullOrEmpty(source.AccountingStringComponentValue))
            {
                /*var id = await _referenceDataRepository.GetAccountingStringComponentValuesGuidFromIdAsync(source.AccountingStringComponentValue);
                if (!string.IsNullOrEmpty(id))
                {
                    budgetPhaseLineItem.AccountingStringComponentValues = new List<GuidObject2>() { new GuidObject2(id) };
                }*/
                var accountingStringGuid = string.Empty;
                if (!accountingStringDict.TryGetValue(source.AccountingStringComponentValue, out accountingStringGuid))
                {
                    accountingStringGuid = await _referenceDataRepository.GetAccountingStringComponentValuesGuidFromIdAsync(source.AccountingStringComponentValue);
                    if (string.IsNullOrEmpty(accountingStringGuid))
                    {
                        throw new ArgumentNullException("AccountingStringComponentValue.Id", string.Concat("Unable to determine guid for AccountingStringComponentValue: ", source.AccountingStringComponentValue));
                    }
                    accountingStringDict.Add(source.AccountingStringComponentValue, accountingStringGuid);
                }
                budgetPhaseLineItem.AccountingStringComponentValues = new List<GuidObject2>() { new GuidObject2(accountingStringGuid) };
            }

            if (source.LineAmount != null && source.LineAmount.HasValue)
            {
                budgetPhaseLineItem.Amount = this.ConvertEntityToAmountDto(source.LineAmount, source.HostCountry);
            }

            if (source.Comments != null && source.Comments.Any())
            {
                budgetPhaseLineItem.Comment = String.Join(" ", source.Comments)
                                                      .Replace(Convert.ToChar(DynamicArray.VM), '\n')
                                                      .Replace(Convert.ToChar(DynamicArray.TM), ' ')
                                                      .Replace(Convert.ToChar(DynamicArray.SM), ' ');
            }


            return budgetPhaseLineItem;
        }

        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. This API will integrate information related to outgoing payments that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewBudgetPhaseLineItemsPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewBudgetPhaseLineItems);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view budget-phase-line-items.");
            }
        }

        /// <summary>
        /// Converts entity to amount.
        /// </summary>
        /// <param name="credit"></param>
        /// <param name="debit"></param>
        /// <param name="hostCountry"></param>
        /// <returns></returns>
        private Dtos.DtoProperties.Amount2DtoProperty ConvertEntityToAmountDto(decimal? value, string hostCountry)
        {
            Dtos.DtoProperties.Amount2DtoProperty amount = null;
            if (value.HasValue)
            {
                amount = new Dtos.DtoProperties.Amount2DtoProperty() {
                    Value = value,
                    Currency = ConvertEntityToHostCountryEnum(hostCountry) };
            }

            return amount;
        }

        /// <summary>
        /// Converts entity to host country.
        /// </summary>
        /// <param name="hostCountry"></param>
        /// <returns></returns>
        private CurrencyIsoCode ConvertEntityToHostCountryEnum(string hostCountry)
        {
            CurrencyIsoCode outValue;
            if (hostCountry.ToUpper().Equals("USA", StringComparison.OrdinalIgnoreCase) && Enum.TryParse("USD", true, out outValue))
            {
                return outValue;
            }
            if (hostCountry.ToUpper().Equals("CAN", StringComparison.OrdinalIgnoreCase) && Enum.TryParse("CAD", true, out outValue))
            {
                return outValue;
            }
            if (hostCountry.ToUpper().Equals("CANADA", StringComparison.OrdinalIgnoreCase) && Enum.TryParse("CAD", true, out outValue))
            {
                return outValue;
            }
            return CurrencyIsoCode.NotSet;
        }
    }
}