// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.BudgetManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Services
{
    /// <summary>
    /// Methods implemented for a Budget Development service.
    /// </summary>
    public interface IBudgetDevelopmentService
    {
        /// <summary>
        /// Get working budget for a user.
        /// </summary>
        /// <param name="startPosition">Start position of the budget line items to return.</param>
        /// <param name="recordCount">Number of budget line items to return.</param>
        /// <returns>A working budget containing a set of budget line items.</returns>
        Task<WorkingBudget> GetBudgetDevelopmentWorkingBudgetAsync(int startPosition, int recordCount);

        /// <summary>
        /// Get the working budget for a user based on their criteria.
        /// </summary>
        /// <param name="criteria">Working budget filter criteria.</param>
        /// <returns>The filtered line items in the working budget.</returns>
        [Obsolete("Obsolete as of API version 1.25. Use the latest version of this method.")]
        Task<WorkingBudget> QueryWorkingBudgetAsync(WorkingBudgetQueryCriteria criteria);

        /// <summary>
        /// Get the working budget for a user based on their criteria.
        /// </summary>
        /// <param name="criteria">Working budget filter criteria.</param>
        /// <returns>The filtered line items in the working budget.</returns>
        Task<WorkingBudget2> QueryWorkingBudget2Async(WorkingBudgetQueryCriteria criteria);

        /// <summary>
        /// Updates the working budget for a user.
        /// </summary>
        /// <param name="budgetLineItems">A list of budget line items for which the working budget amount will be updated.</param>
        /// <returns>A list of budget line items that have been updated for the working budget.</returns>
        Task<List<BudgetLineItem>> UpdateBudgetDevelopmentWorkingBudgetAsync(List<BudgetLineItem> budgetLineItems);

        /// <summary>
        /// Get the budget officers for the working budget for a user.
        /// </summary>
        /// <returns>Budget officers for a working budget for a user.</returns>
        Task<List<BudgetOfficer>> GetBudgetDevelopmentBudgetOfficersAsync();

        /// <summary>
        /// Get the reporting units for a user in the working budget.
        /// </summary>
        /// <returns>Reporting units for a working budget for a user.</returns>
        Task<List<BudgetReportingUnit>> GetBudgetDevelopmentReportingUnitsAsync();
    }
}
