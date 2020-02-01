// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Provider for General Ledger configuration services.
    /// </summary>
    [RegisterType]
    public class GeneralLedgerConfigurationService : BaseCoordinationService, IGeneralLedgerConfigurationService
    {
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;

        // Constructor for the General Ledger Configuration coordination service.
        public GeneralLedgerConfigurationService(IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
        }

        /// <summary>
        /// Returns a GeneralLedgerConfiguration DTO.
        /// </summary>
        /// <returns>A GeneralLedgerConfiguration DTO.</returns>
        public async Task<Dtos.ColleagueFinance.GeneralLedgerConfiguration> GetGeneralLedgerConfigurationAsync()
        {
            // Obtain the configuration for the GL account structure.
             var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

             var glconfigurationAdapter = new GlAccountStructureEntityToGlConfigurationDtoAdapter(_adapterRegistry, logger);
             var glConfigurationDto = new Dtos.ColleagueFinance.GeneralLedgerConfiguration();
             if (glAccountStructure != null)
             {
                 glConfigurationDto = glconfigurationAdapter.MapToType(glAccountStructure);
             }

             return glConfigurationDto;
        }

        /// <summary>
        /// Get configuration information necessary to validate a budget adjustment.
        /// </summary>
        /// <returns>BudgetAdjustmentConfiguration DTO.</returns>
        public async Task<BudgetAdjustmentConfiguration> GetBudgetAdjustmentConfigurationAsync()
        {
            var configurationDto = new BudgetAdjustmentConfiguration();

            // Get the fiscal year configuraton info.
            var fiscalYearConfigurationEntity = await generalLedgerConfigurationRepository.GetFiscalYearConfigurationAsync();
            if(fiscalYearConfigurationEntity == null)
            {
                throw new ConfigurationException("Fiscal year configuration must be defined.");
            }

            // Get the GL class configuraton info.
            var glClassConfigurationEntity = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfigurationEntity == null)
            {
                throw new ConfigurationException("GL class configuration must be defined.");
            }

            var costCenterStructure = await generalLedgerConfigurationRepository.GetCostCenterStructureAsync();
            if (costCenterStructure == null || !costCenterStructure.CostCenterComponents.Any())
            {
                throw new ConfigurationException("Cost center structure not defined.");
            }

            // Obtain the parameter that determines if the same cost center is required.
            var budgetAdjustmentParameters = await this.generalLedgerConfigurationRepository.GetBudgetAdjustmentParametersAsync();

            // Parameters to validate the budget adjustment transaction date.
            configurationDto.CurrentFiscalMonth = fiscalYearConfigurationEntity.CurrentFiscalMonth;
            configurationDto.CurrentFiscalYear = fiscalYearConfigurationEntity.CurrentFiscalYear;
            configurationDto.StartMonth = fiscalYearConfigurationEntity.StartMonth;
            configurationDto.StartOfFiscalYear = fiscalYearConfigurationEntity.StartOfFiscalYear;
            configurationDto.EndOfFiscalYear = fiscalYearConfigurationEntity.EndOfFiscalYear;
            configurationDto.ExtendedEndOfFiscalYear = fiscalYearConfigurationEntity.ExtendedEndOfFiscalYear;
            configurationDto.NumberOfFuturePeriods = fiscalYearConfigurationEntity.NumberOfFuturePeriods;
            configurationDto.OpenFiscalYears = await generalLedgerConfigurationRepository.GetAllOpenFiscalYears();

            // Parameters to validate that the GL accounts in the budget adjustment 
            // have only an expense GL class.
            configurationDto.Length = glClassConfigurationEntity.GlClassLength;
            configurationDto.StartPosition = glClassConfigurationEntity.GlClassStartPosition;
            configurationDto.ExpenseClassValues = glClassConfigurationEntity.ExpenseClassValues;

            // Parameter to validate that the GL accounts are from the same cost center.
            configurationDto.SameCostCenterRequired = budgetAdjustmentParameters.SameCostCenterRequired;

            // Approval parameters.
            configurationDto.ApprovalRequired = budgetAdjustmentParameters.ApprovalRequired;
            configurationDto.SameCostCenterApprovalRequired = budgetAdjustmentParameters.SameCostCenterApprovalRequired;

            var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerComponent, Ellucian.Colleague.Dtos.ColleagueFinance.GeneralLedgerComponent>();
            var costCenterComponentDtos = new List<GeneralLedgerComponent>();
            foreach (var component in costCenterStructure.CostCenterComponents)
            {
                costCenterComponentDtos.Add(adapter.MapToType(component));
            }
            configurationDto.CostCenterComponents = costCenterComponentDtos;

            return configurationDto;
        }

        /// <summary>
        /// Get indicator that determines whether budget adjustments are turned on or off.
        /// </summary>
        /// <returns>BudgetAdjustmentsEnabled DTO</returns>
        public async Task<BudgetAdjustmentsEnabled> GetBudgetAdjustmentEnabledAsync()
        {
            var enabledEntity = await this.generalLedgerConfigurationRepository.GetBudgetAdjustmentEnabledAsync();
            var enabledDto = new BudgetAdjustmentsEnabled()
            {
                Enabled = false
            };

            // Set the enabled property using the entity value, if the entity is not null.
            if (enabledEntity != null)
            {
                enabledDto.Enabled = enabledEntity.Enabled;
            }

            return enabledDto;
        }

        /// <summary>
        /// Get fiscal year configuration information necessary to validate fiscal year dates used in finance query.
        /// </summary>
        /// <returns>GlFiscalYearConfiguration DTO</returns>
        public async Task<GlFiscalYearConfiguration> GetGlFiscalYearConfigurationAsync()
        {
            var glFiscalYearConfiguration = new GlFiscalYearConfiguration();

            // Get the fiscal year configuraton info.
            var fiscalYearConfigurationEntity = await generalLedgerConfigurationRepository.GetFiscalYearConfigurationAsync();
            if (fiscalYearConfigurationEntity == null)
            {
                throw new ConfigurationException("Fiscal year configuration must be defined.");
            }

            // Parameters to validate the actuals start & end date in finance query.
            glFiscalYearConfiguration.StartOfFiscalYear = fiscalYearConfigurationEntity.StartOfFiscalYear;
            glFiscalYearConfiguration.EndOfFiscalYear = fiscalYearConfigurationEntity.EndOfFiscalYear;
            glFiscalYearConfiguration.StartMonth = fiscalYearConfigurationEntity.StartMonth;
            glFiscalYearConfiguration.CurrentFiscalMonth = fiscalYearConfigurationEntity.CurrentFiscalMonth;
            glFiscalYearConfiguration.CurrentFiscalYear = fiscalYearConfigurationEntity.CurrentFiscalYear;
            glFiscalYearConfiguration.ExtendedEndOfFiscalYear = fiscalYearConfigurationEntity.ExtendedEndOfFiscalYear;
            glFiscalYearConfiguration.NumberOfFuturePeriods = fiscalYearConfigurationEntity.NumberOfFuturePeriods;
            glFiscalYearConfiguration.OpenFiscalYears = await generalLedgerConfigurationRepository.GetAllOpenFiscalYears();
            return glFiscalYearConfiguration;
        }
    }
}
