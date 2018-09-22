// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Methods implemented for a General Ledger Configuration service.
    /// </summary>
    public interface IGeneralLedgerConfigurationService
    {
        /// <summary>
        /// Get General Ledger Configuration parameters.
        /// </summary>
        /// <returns></returns>
        Task<GeneralLedgerConfiguration> GetGeneralLedgerConfigurationAsync();

        /// <summary>
        /// Get configuration information necessary to validate a budget adjustment.
        /// </summary>
        /// <returns>BudgetAdjustmentConfiguration DTO</returns>
        Task<BudgetAdjustmentConfiguration> GetBudgetAdjustmentConfigurationAsync();

        /// <summary>
        /// Get indicator that determines whether budget adjustments are turned on or off.
        /// </summary>
        /// <returns>BudgetAdjustmentsEnabled DTO</returns>
        Task<BudgetAdjustmentsEnabled> GetBudgetAdjustmentEnabledAsync();
    }
}

