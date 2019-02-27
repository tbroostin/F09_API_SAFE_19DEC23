// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Defines methods necessary to read/write budget adjustments.
    /// </summary>
    public interface IBudgetAdjustmentsRepository
    {
        /// <summary>
        /// Create a new budget adjustment.
        /// </summary>
        /// <param name="budgetAdjustment">Budget adjustment.</param>
        /// <returns>Budget adjustment response</returns>
        Task<BudgetAdjustment> CreateAsync(BudgetAdjustment budgetAdjustment);

        /// <summary>
        /// Update an existing budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment to update.</param>
        /// <param name="budgetAdjustment">The new budget adjustment data.</param>
        /// <returns>The updated budget adjustment.</returns>
        Task<BudgetAdjustment> UpdateAsync(string id, BudgetAdjustment budgetAdjustment);

        /// <summary>
        /// Get a budget adjustment.
        /// </summary>
        /// <param name="id">The ID for the budget adjustment.</param>
        /// <returns>A budget adjustment.</returns>
        Task<BudgetAdjustment> GetBudgetAdjustmentAsync(string id);

        /// <summary>
        /// Get a list of budget adjustments for the user logged in.
        /// </summary>
        /// <param name="personId">Person ID for the current user.</param>
        /// <returns>List of budget adjustment summary DTOs.</returns>
        Task<IEnumerable<BudgetAdjustmentSummary>> GetBudgetAdjustmentsSummaryAsync(string personId);

        /// <summary>
        /// Get a list of budget adjustments pending approval for the user logged in.
        /// </summary>
        /// <param name="personId">Person ID for the current user.</param>
        /// <returns>List of budget adjustments pending approval summary DTOs.</returns>
        Task<IEnumerable<BudgetAdjustmentPendingApprovalSummary>> GetBudgetAdjustmentsPendingApprovalSummaryAsync(string personId);

        /// <summary>
        /// Validates the budget adjustment entity using a Colleague Transaction.
        /// </summary>
        /// <param name="budgetAdjustmentEntity">The entity to validate.</param>
        /// <returns>List of strings that contain error messages from Colleague.</returns>
        Task<List<string>> ValidateBudgetAdjustmentAsync(BudgetAdjustment budgetAdjustmentEntity);
    }
}