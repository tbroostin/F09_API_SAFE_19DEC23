// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Interface to the General Ledger Configuration repository.
    /// </summary>
    public interface IGeneralLedgerConfigurationRepository
    {
        /// <summary>
        /// Get the General Ledger Account structure configuration for Colleague Financials.
        /// </summary>
        /// <returns>General Ledger account structure.</returns>
        Task<GeneralLedgerAccountStructure> GetAccountStructureAsync();

        /// <summary>
        /// Get the General Ledger Cost Center structure configuration for Colleague Financials.
        /// </summary>
        /// <returns>General Ledger account structure.</returns>
        Task<CostCenterStructure> GetCostCenterStructureAsync();

        /// <summary>
        /// Returns the GL fiscal year information.
        /// </summary>
        /// <returns>Fiscal year configuration.</returns>
        Task<GeneralLedgerFiscalYearConfiguration> GetFiscalYearConfigurationAsync();

        /// <summary>
        /// Return the GL class configuration.
        /// </summary>
        /// <returns>GL class configuration.</returns>
        Task<GeneralLedgerClassConfiguration> GetClassConfigurationAsync();

        /// <summary>
        /// Return a set of fiscal years; the current year plus up to five previous years, if available.
        /// </summary>
        /// <param name="currentFiscalYear"></param>
        /// <returns>Set of fiscal years.</returns>
        Task<IEnumerable<string>> GetAllFiscalYearsAsync(int currentFiscalYear);

        /// <summary>
        /// Get all of the open fiscal years.
        /// </summary>
        /// <returns>Set of fiscal years.</returns>
        Task<IEnumerable<string>> GetAllOpenFiscalYears();

        /// <summary>
        /// Get indicator that determines whether budget adjustments are turned on or off.
        /// </summary>
        /// <returns>BudgetAdjustmentsEnabled entity.</returns>
        Task<BudgetAdjustmentsEnabled> GetBudgetAdjustmentEnabledAsync();

        /// <summary>
        /// Get the exclusion data for evaluating which GL accounts may be used in a budget adjustment.
        /// </summary>
        /// <returns>BudgetAdjustmentAccountExclusions entity.</returns>
        Task<BudgetAdjustmentAccountExclusions> GetBudgetAdjustmentAccountExclusionsAsync();

        /// <summary>
        /// Get parameters that control allowed/required business rules for budget adjustments.
        /// </summary>
        /// <returns>BudgetAdjustmentsParameters entity.</returns>
        Task<BudgetAdjustmentParameters> GetBudgetAdjustmentParametersAsync();
    }
}
