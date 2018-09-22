// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the ICostCenterService interface.
    /// </summary>
    [RegisterType]
    public class CostCenterService : BaseCoordinationService, ICostCenterService
    {
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private ICostCenterRepository costCenterRepository;

        // This constructor initializes the private attributes.
        public CostCenterService(ICostCenterRepository costCenterRepository,
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
            this.costCenterRepository = costCenterRepository;
        }

        /// <summary>
        /// Returns the GL cost center DTOs that are associated with the user logged into self-service for the 
        /// specified fiscal year. We want all the cost centers for the user, so no cost center id is passed in.
        /// </summary>
        /// <param name="fiscalYear">General Ledger fiscal year; it can be null.</param>
        /// <returns>List of GL cost center DTOs for the fiscal year.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.CostCenter>> GetAsync(string fiscalYear)
        {
            // The first time the user gets to the cost center view we need to default the fiscal year to
            // the one for today's date because the user does not yet get a change to select one from the list.
            // Get the fiscal year configuration data to get the fiscal year for today's date.
            if (string.IsNullOrEmpty(fiscalYear))
            {
                var fiscalYearConfiguration = await generalLedgerConfigurationRepository.GetFiscalYearConfigurationAsync();
                fiscalYear = fiscalYearConfiguration.FiscalYearForToday.ToString();
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get cost center configuration so we know how to calculate the cost center.
            var costCenterStructure = await generalLedgerConfigurationRepository.GetCostCenterStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get his list of assigned expense and revenue GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);

            // If the user does not have any expense or revenue accounts assigned, return an empty list of DTOs.
            // Create the adapter to convert cost center domain entities to DTOs.
            var costCenterAdapter = new CostCenterEntityToDtoAdapter(_adapterRegistry, logger);
            var costCenterDtos = new List<Dtos.ColleagueFinance.CostCenter>();

            // If the user does not have any expense or revenue accounts assigned, return an empty list of DTOs.
            if ((generalLedgerUser.ExpenseAccounts != null && generalLedgerUser.ExpenseAccounts.Any()) || (generalLedgerUser.RevenueAccounts != null && generalLedgerUser.RevenueAccounts.Any()))
            {
                // We are using the same repository method to get a list of cost centers for the user or the one cost center they selected.
                // Do not pass a cost center ID so all cost centers are returned.
                var costCenters = await costCenterRepository.GetCostCentersAsync(generalLedgerUser, costCenterStructure, glClassConfiguration,
                    null, fiscalYear, null, CurrentUser.PersonId);

                // Convert the domain entities into DTOs
                foreach (var entity in costCenters)
                {
                    if (entity != null)
                    {
                        var costCenterDto = costCenterAdapter.MapToType(entity, glAccountStructure.MajorComponentStartPositions);
                        costCenterDtos.Add(costCenterDto);
                    }
                }
            }

            return costCenterDtos;
        }

        /// <summary>
        /// Returns the GL cost center DTOs that are associated with the user logged into self-service for the 
        /// specified fiscal year. We want all the cost centers for the user, so no cost center id is passed in.
        /// </summary>
        /// <param name="criteria">Cost center filter criteria.</param>
        /// <returns>List of GL cost center DTOs for the fiscal year.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.CostCenter>> QueryCostCentersAsync(CostCenterQueryCriteria criteria)
        {
            // The query criteria can be empty, but it cannot be null.
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Filter component criteria must be specified.");
            }

            // Get the cost center ID from the criteria object so we can pass it into the repository. A null value IS acceptable.
            string costCenterId = null;
            if (criteria.Ids != null)
            {
                costCenterId = criteria.Ids.FirstOrDefault();
            }
            // The first time the user gets to the cost center view we need to default the fiscal year to
            // the one for today's date because the user does not yet get a change to select one from the list.
            // Get the fiscal year configuration data to get the fiscal year for today's date.
            var fiscalYear = criteria.FiscalYear;
            if (string.IsNullOrEmpty(fiscalYear))
            {
                var fiscalYearConfiguration = await generalLedgerConfigurationRepository.GetFiscalYearConfigurationAsync();
                fiscalYear = fiscalYearConfiguration.FiscalYearForToday.ToString();
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get cost center configuration so we know how to calculate the cost center.
            var costCenterStructure = await generalLedgerConfigurationRepository.GetCostCenterStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get his list of assigned expense and revenue GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);

            // If the user does not have any expense or revenue accounts assigned, return an empty list of DTOs.
            // Create the adapter to convert cost center domain entities to DTOs.
            var costCenterAdapter = new CostCenterEntityToDtoAdapter(_adapterRegistry, logger);
            var costCenterDtos = new List<Dtos.ColleagueFinance.CostCenter>();

            // If the user does not have any expense or revenue accounts assigned, return an empty list of DTOs.
            if ((generalLedgerUser.ExpenseAccounts != null && generalLedgerUser.ExpenseAccounts.Any()) || (generalLedgerUser.RevenueAccounts != null && generalLedgerUser.RevenueAccounts.Any()))
            {
                // If we have expense or revenue accounts for the general ledger user, convert the filter criteria DTO
                // into a domain entity, and pass it into the cost center repository.
                var costCenterCriteriaAdapter = new CostCenterQueryCriteriaDtoToEntityAdapter(_adapterRegistry, logger);
                var queryCriteriaEntity = costCenterCriteriaAdapter.MapToType(criteria);

                // We are using the same repository method to get a list of cost centers for the user or the one cost center they selected.
                // Do not pass a cost center ID so all cost centers are returned.
                var costCenters = await costCenterRepository.GetCostCentersAsync(generalLedgerUser, costCenterStructure, glClassConfiguration,
                    costCenterId, fiscalYear, queryCriteriaEntity, CurrentUser.PersonId);

                // Convert the domain entities into DTOs
                foreach (var entity in costCenters)
                {
                    if (entity != null)
                    {
                        var costCenterDto = costCenterAdapter.MapToType(entity, glAccountStructure.MajorComponentStartPositions);
                        costCenterDtos.Add(costCenterDto);
                    }
                }
            }

            return costCenterDtos;
        }

        /// <summary>
        /// Returns the cost center DTO selected by the user.
        /// Uses the same repository method that retreives all the cost centers for the user
        /// but we passed the cost center id so only that one is returned.
        /// </summary>
        /// <param name="costCenterId">Selected cost center ID.</param>
        /// <param name="fiscalYear">General Ledger fiscal year; it can be null.</param>
        /// <returns>Cost Center DTO.</returns>
        public async Task<Dtos.ColleagueFinance.CostCenter> GetCostCenterAsync(string costCenterId, string fiscalYear)
        {
            // The first time the user gets to the cost center view we need to default the fiscal year to
            // the one for today's date because the user does not yet get a change to select one from the list.
            // Get the fiscal year configuration data to get the fiscal year for today's date.
            if (string.IsNullOrEmpty(fiscalYear))
            {
                var fiscalYearConfiguration = await generalLedgerConfigurationRepository.GetFiscalYearConfigurationAsync();
                fiscalYear = fiscalYearConfiguration.FiscalYearForToday.ToString();
            }


            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // Get cost center configuration so we know how to calculate the cost center.
            var costCenterStructure = await generalLedgerConfigurationRepository.GetCostCenterStructureAsync();

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();

            // Get the ID for the person who is logged in, and use the ID to get his list of assigned expense and revenue GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);

            // If the user does not have any expense or revenue accounts assigned, return an empty DTO.
            var costCenterDto = new Dtos.ColleagueFinance.CostCenter();
            if ((generalLedgerUser.ExpenseAccounts != null && generalLedgerUser.ExpenseAccounts.Any()) || (generalLedgerUser.RevenueAccounts != null && generalLedgerUser.RevenueAccounts.Any()))
            {
                // We are using the same repository method to get a list of cost centers for the user or the one cost center they selected.
                // Pass in the cost center ID of the one we want to have returned.
                // The repository still returns a list though in this case it contains only one.
                var costCenters = await costCenterRepository.GetCostCentersAsync(generalLedgerUser, costCenterStructure, glClassConfiguration,
                    costCenterId, fiscalYear, null, CurrentUser.PersonId);
                var costCenter = costCenters.ToList().FirstOrDefault();

                // Create the adapter to convert cost center domain entity to DTO.
                var costCenterAdapter = new CostCenterEntityToDtoAdapter(_adapterRegistry, logger);

                // The GL accounts for this one cost center may not have existed yet for the fiscal year,
                // so the repository would have return an empty domain entity.
                if (costCenter != null)
                {
                    // Convert the domain entity into a DTO.
                    costCenterDto = costCenterAdapter.MapToType(costCenter, glAccountStructure.MajorComponentStartPositions);
                }
            }

            return costCenterDto;
        }

        /// <summary>
        /// Get a list of fiscal years for the cost centers views.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetFiscalYearsAsync()
        {
            var fiscalYearInfo = await generalLedgerConfigurationRepository.GetFiscalYearConfigurationAsync();
            var fiscalYears = await generalLedgerConfigurationRepository.GetAllFiscalYearsAsync(fiscalYearInfo.FiscalYearForToday);

            return fiscalYears;
        }

        /// <summary>
        /// Get today's fiscal year based on the General Ledger configuration.
        /// </summary>
        /// <returns>Today's fiscal year.</returns>
        public async Task<string> GetFiscalYearForTodayAsync()
        {
            var fiscalYearInfo = await generalLedgerConfigurationRepository.GetFiscalYearConfigurationAsync();
            var fiscalYearForToday = fiscalYearInfo.FiscalYearForToday;

            return fiscalYearForToday.ToString();
        }
    }
}
