//Copyright 2018-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Services
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
                catch (Exception)
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
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.BudgetPhaseLineItems>, int>(new List<Dtos.BudgetPhaseLineItems>(), 0);
                }
            }

            Tuple<IEnumerable<BudgetWork>, int> budgetPhasesEntitiesTuple = null;

            try
            {
                budgetPhasesEntitiesTuple = await _budgetRepository.GetBudgetPhaseLineItemsAsync(limit, offset,
                budgetPhase, accountingStringComponentValueId, bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (budgetPhasesEntitiesTuple == null || budgetPhasesEntitiesTuple.Item1 == null)
            {
                return new Tuple<IEnumerable<Dtos.BudgetPhaseLineItems>, int>(new List<Dtos.BudgetPhaseLineItems>(), 0);
            }

            try
            {
                var glAcctIds = budgetPhasesEntitiesTuple.Item1
                   .Where(x => (!string.IsNullOrEmpty(x.AccountingStringComponentValue)))
                   .Select(x => x.AccountingStringComponentValue).Distinct().ToList();
                var glAcctIdCollection = await _referenceDataRepository.GetGuidsForPooleeGLAcctsInFiscalYearsAsync(glAcctIds);

                var budgetPhaseIds = budgetPhasesEntitiesTuple.Item1
                  .Where(x => (!string.IsNullOrEmpty(x.BudgetPhase)))
                  .Select(x => x.BudgetPhase).Distinct().ToList();
                var budgetPhaseIdCollection = await _budgetRepository.GetBudgetGuidCollectionAsync(budgetPhaseIds);

                foreach (var budgetWork in budgetPhasesEntitiesTuple.Item1)
                {
                    budgetPhaseLineItemsCollection.Add(await ConvertBudgetWorkEntityToDtoAsync(budgetWork, glAcctIdCollection, budgetPhaseIdCollection, bypassCache));
                }
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error");
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return new Tuple<IEnumerable<Dtos.BudgetPhaseLineItems>, int>(budgetPhaseLineItemsCollection, budgetPhasesEntitiesTuple.Item2);

        }

        public async Task<BudgetPhaseLineItems> GetBudgetPhaseLineItemsByGuidAsync(string guid, bool bypassCache = true)
        {
            BudgetWork budgetPhaseLineItemsByGuid = null;

            try
            {
                budgetPhaseLineItemsByGuid = await _budgetRepository.GetBudgetPhaseLineItemsByGuidAsync(guid);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No budget-phase-line-items was found for GUID '" + guid + "'", ex);
            }

            try
            {
                var glAcctIdCollection = await _referenceDataRepository.GetGuidsForPooleeGLAcctsInFiscalYearsAsync(new List<string> { budgetPhaseLineItemsByGuid.AccountingStringComponentValue });
                var budgetPhaseIdCollection = await _budgetRepository.GetBudgetGuidCollectionAsync(new List<string> { budgetPhaseLineItemsByGuid.BudgetPhase });


                var retval = await ConvertBudgetWorkEntityToDtoAsync(budgetPhaseLineItemsByGuid, glAcctIdCollection, budgetPhaseIdCollection, bypassCache);
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return retval;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No budget-phase-line-items was found for GUID '" + guid + "'", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No budget-phase-line-items was found for GUID '" + guid + "'", ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a BudWork domain entity to its corresponding BudgetPhaseLineItems DTO
        /// </summary>
        /// <param name="source">Budget domain entity</param>
        /// <returns>BudgetPhases DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.BudgetPhaseLineItems> ConvertBudgetWorkEntityToDtoAsync(BudgetWork source,
            IDictionary<string, string> glAccountIdCollection, IDictionary<string, string> budgetPhaseIdCollection, bool bypassCache = false)
        {
            var budgetPhaseLineItem = new Ellucian.Colleague.Dtos.BudgetPhaseLineItems();

            if (source == null)
            {
                IntegrationApiExceptionAddError("Missing BudgetWork data to build response.", "Bad.Data");
                return null;
            }

            if (string.IsNullOrEmpty(source.RecordGuid))
            {
                IntegrationApiExceptionAddError("Unable to determine GUID for BudgetWork.",
                               "GUID.Not.Found", source.RecordGuid, source.RecordKey);
            }
            else
            {

                budgetPhaseLineItem.Id = source.RecordGuid;
            }

            //if (!string.IsNullOrEmpty(source.BudgetPhase))
            //{
            //    var budgetPhasesGuid = string.Empty;
            //    if (!budgetPhaseDict.TryGetValue(source.BudgetPhase, out budgetPhasesGuid))
            //    {
            // budgetPhasesGuid = await _budgetRepository.GetBudgetPhasesGuidFromIdAsync(source.BudgetPhase);
            //        if (string.IsNullOrEmpty(budgetPhasesGuid))
            //        {
            //            IntegrationApiExceptionAddError(string.Concat("Unable to determine GUID for BudgetPhase: ", source.BudgetPhase), "GUID.Not.Found",
            //                source.RecordGuid, source.RecordKey);
            //        }
            //        else
            //        {
            //            budgetPhaseDict.Add(source.BudgetPhase, budgetPhasesGuid);
            //        }
            //    }

            //    budgetPhaseLineItem.BudgetPhase = new GuidObject2(budgetPhasesGuid);
            //}

            if (!string.IsNullOrEmpty(source.BudgetPhase))
            {
                if (budgetPhaseIdCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Concat("Unable to determine GUID for BudgetPhase: ", source.BudgetPhase), "GUID.Not.Found",
                              source.RecordGuid, source.RecordKey);
                }
                else
                {
                    var budgetPhaseGuid = string.Empty;
                    budgetPhaseIdCollection.TryGetValue(source.BudgetPhase, out budgetPhaseGuid);
                    if (string.IsNullOrEmpty(budgetPhaseGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Unable to determine GUID for BudgetPhase: ", source.BudgetPhase), "GUID.Not.Found",
                             source.RecordGuid, source.RecordKey);
                    }
                    else
                    {
                        budgetPhaseLineItem.BudgetPhase = new GuidObject2(budgetPhaseGuid);
                    }
                }
            }


            //if (!string.IsNullOrEmpty(source.AccountingStringComponentValue))
            //{
            //    var accountingStringGuid = string.Empty;
            //    if (!accountingStringDict.TryGetValue(source.AccountingStringComponentValue, out accountingStringGuid))
            //    {
            //        accountingStringGuid = await _referenceDataRepository.GetAccountingStringComponentValuesGuidFromIdAsync(source.AccountingStringComponentValue);
            //        if (string.IsNullOrEmpty(accountingStringGuid))
            //        {
            //            IntegrationApiExceptionAddError(string.Concat("Unable to determine GUID for AccountingStringComponentValue: ", source.AccountingStringComponentValue),
            //                "GUID.Not.Found", source.RecordGuid, source.RecordKey);
            //        }
            //        else
            //        {
            //            accountingStringDict.Add(source.AccountingStringComponentValue, accountingStringGuid);
            //        }
            //    }
            //    budgetPhaseLineItem.AccountingStringComponentValues = new List<GuidObject2>() { new GuidObject2(accountingStringGuid) };
            //}

            if (!string.IsNullOrEmpty(source.AccountingStringComponentValue))
            {
                if (glAccountIdCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Concat("Unable to determine GUID for AccountingStringComponentValue: ", source.AccountingStringComponentValue),
                        "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                }
                else
                {
                    var accountingStringComponentValueGuid = string.Empty;
                    glAccountIdCollection.TryGetValue(source.AccountingStringComponentValue, out accountingStringComponentValueGuid);
                    if (string.IsNullOrEmpty(accountingStringComponentValueGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Unable to determine GUID for AccountingStringComponentValue: ", source.AccountingStringComponentValue),
                       "GUID.Not.Found", source.RecordGuid, source.RecordKey);
                    }
                    else
                    {
                        budgetPhaseLineItem.AccountingStringComponentValues = new List<GuidObject2>() { new GuidObject2(accountingStringComponentValueGuid) };
                    }
                }
            }

            if (source.LineAmount != null && source.LineAmount.HasValue)
            {
                budgetPhaseLineItem.Amount = this.ConvertEntityToAmountDto(source.LineAmount, source.HostCountry);
            }

            if (source.Comments != null && source.Comments.Any())
            {
                budgetPhaseLineItem.Comment = String.Join(" ", source.Comments)
                                                      .Replace(DmiString._VM, '\n')
                                                      .Replace(DmiString._TM, ' ')
                                                      .Replace(DmiString._SM, ' ');
            }


            return budgetPhaseLineItem;
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
                amount = new Dtos.DtoProperties.Amount2DtoProperty()
                {
                    Value = value,
                    Currency = ConvertEntityToHostCountryEnum(hostCountry)
                };
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