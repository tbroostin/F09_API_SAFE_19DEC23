// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for the Budget adjustment service.
    /// </summary>
    public interface IBudgetAdjustmentService
    {
        /// <summary>
        /// Create a new budget adjustment.
        /// </summary>
        /// <param name="budgetAdjustmentDto">Budget adjustment DTO.</param>
        /// <returns>Budget adjustment DTO</returns>
        Task<BudgetAdjustment> CreateBudgetAdjustmentAsync(BudgetAdjustment budgetAdjustmentDto);

        /// <summary>
        /// Update a budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment to update.</param>
        /// <param name="budgetAdjustmentDto">The updated budget adjustment data</param>
        /// <returns>Budget adjustment DTO</returns>
        Task<BudgetAdjustment> UpdateBudgetAdjustmentAsync(string id, BudgetAdjustment budgetAdjustmentDto);

        /// <summary>
        /// Get a budget adjustment.
        /// </summary>
        /// <param name="id">The ID for the budget adjustment.</param>
        /// <returns>A budget adjustment.</returns>
        Task<BudgetAdjustment> GetBudgetAdjustmentAsync(string id);

        /// <summary>
        /// Get the list of budget adjustments summary for the user logged in.
        /// </summary>
        /// <returns>List of budget adjustment summary DTOs.</returns>
        Task<IEnumerable<BudgetAdjustmentSummary>> GetBudgetAdjustmentsSummaryAsync();

        /// <summary>
        /// Get a list of budget adjustments pending approval for the user logged in.
        /// </summary>
        /// <param name="personId">Person ID for the current user.</param>
        /// <returns>List of budget adjustments pending approval summary DTOs.</returns>
        Task<IEnumerable<BudgetAdjustmentPendingApprovalSummary>> GetBudgetAdjustmentsPendingApprovalSummaryAsync();
    }
}