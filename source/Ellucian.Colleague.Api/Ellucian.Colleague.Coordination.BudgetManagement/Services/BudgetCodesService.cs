//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.BudgetManagement;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Services
{
    [RegisterType]
    public class BudgetCodesService : BaseCoordinationService, IBudgetCodesService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;


        public BudgetCodesService(
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

        /// <summary>
        /// Gets all budget-codes
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="BudgetCodes">budgetCodes</see> objects</returns>          
        public async Task<IEnumerable<BudgetCodes>> GetBudgetCodesAsync(bool bypassCache = false)
        {
            var budgetCodesCollection = new List<Ellucian.Colleague.Dtos.BudgetCodes>();

            var budgetCodesEntities = await _budgetRepository.GetBudgetCodesAsync(bypassCache);
            var totalRecords = budgetCodesEntities.Item2;
            if (budgetCodesEntities.Item1 != null)
            {
                string corpName = await _referenceDataRepository.GetCorpNameAsync();

                foreach (var budgetCodes in budgetCodesEntities.Item1)
                {
                    budgetCodesCollection.Add(await ConvertBudgetCodesEntityToDtoAsync(budgetCodes, corpName, bypassCache));
                }
            }
            return budgetCodesCollection;
        }

        /// <summary>
        /// Get a budgetCodes by guid.
        /// </summary>
        /// <param name="guid">Guid of the budgetCodes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="BudgetCodes">budgetCodes</see></returns>
        public async Task<BudgetCodes> GetBudgetCodesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                string corpName = await _referenceDataRepository.GetCorpNameAsync();
                return await ConvertBudgetCodesEntityToDtoAsync(await _budgetRepository.GetBudgetCodesAsync(guid), corpName, bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No budget code was found for guid " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No budget code was found for guid " + guid, ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Budget domain entity to its corresponding BudgetCodes DTO
        /// </summary>
        /// <param name="source">Budget domain entity</param>
        /// <returns>BudgetCodes DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.BudgetCodes> ConvertBudgetCodesEntityToDtoAsync(Budget source, string corpName, bool bypassCache = false)
        {
            var budgetCodes = new Ellucian.Colleague.Dtos.BudgetCodes();

            if(string.IsNullOrEmpty(source.BudgetCodeGuid))
            {
                throw new InvalidOperationException(string.Format("GUID is required. Id: {0}", source.RecordKey));
            }
            budgetCodes.Id = source.BudgetCodeGuid;
            budgetCodes.Code = source.RecordKey;
            budgetCodes.Description = null;

            budgetCodes.Title = string.IsNullOrEmpty(source.Title)
                 ? source.RecordKey : source.Title;

            if (!string.IsNullOrEmpty(corpName))
            {
                budgetCodes.ReportingSegment = corpName;
            }

            if (!string.IsNullOrEmpty(source.FiscalYear))
            {
                var fiscalYears = await FiscalYearsAsync(bypassCache);
                if (fiscalYears != null && fiscalYears.Any())
                {
                    var fiscalYear = fiscalYears.FirstOrDefault(fy => fy.Id == source.FiscalYear);
                    if (fiscalYear != null)
                    {
                        budgetCodes.FiscalYear = new GuidObject2(fiscalYear.Guid);
                    }
                }
            }
            return budgetCodes;
        }
    }
}