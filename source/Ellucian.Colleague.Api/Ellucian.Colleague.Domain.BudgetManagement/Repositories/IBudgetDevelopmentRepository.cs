// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.BudgetManagement.Repositories
{
    public interface IBudgetDevelopmentRepository
    {
        /// <summary>
        /// Gets the working budget domain entity.
        /// </summary>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="budgetConfigurationComparables">The list of budget development configuration comparables.</param>
        /// <param name="criteria">Filter query criteria.</param>
        /// <param name="currentUserPersonId">The user PERSON ID of the current user.</param>
        /// <param name="majorComponentStartPosition">List of the major component start positions.</param>
        /// <param name="startPosition">Start position of the budget line items to return.</param>
        /// <param name="recordCount">Number of budget line items to return.</param>
        /// <returns>A working budget domain entity.</returns>
        [Obsolete("Obsolete as of API version 1.25. Use the latest version of this method.")]
        Task<WorkingBudget> GetBudgetDevelopmentWorkingBudgetAsync(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> budgetConfigurationComparables, WorkingBudgetQueryCriteria criteria,
            string currentUserPersonId, IList<string> majorComponentStartPosition, int startPosition, int recordCount);

        /// <summary>
        /// Gets the working budget domain entity.
        /// </summary>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="budgetConfigurationComparables">The list of budget development configuration comparables.</param>
        /// <param name="criteria">Filter query criteria.</param>
        /// <param name="currentUserPersonId">The user PERSON ID of the current user.</param>
        /// <param name="glAccountStructure">The GL account structure domain entity.</param>
        /// <param name="startPosition">Start position of the budget line items to return.</param>
        /// <param name="recordCount">Number of budget line items to return.</param>
        /// <returns>A working budget domain entity.</returns>
        Task<WorkingBudget2> GetBudgetDevelopmentWorkingBudget2Async(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> budgetConfigurationComparables, WorkingBudgetQueryCriteria criteria,
            string currentUserPersonId, GeneralLedgerAccountStructure glAccountStructure, int startPosition, int recordCount);

        /// <summary>
        /// Gets a list of working budget line items.
        /// </summary>
        /// <param name="workingBudgetId">A working budget ID.</param>
        /// <param name="currentUserPersonId">The current user PERSON ID.</param>
        /// <param name="budgetAccountIds">A list of budget account IDs.</param>
        /// <returns>A list of budget line item domain entities.</returns>
        Task<List<BudgetLineItem>> GetBudgetDevelopmentBudgetLineItemsAsync(string workingBudgetId, string currentUserPersonId, List<string> budgetAccountIds);

        /// <summary>
        /// Updates a working budget line item.
        /// </summary>
        /// <param name="currentUserPersonId">The user PERSON ID of the current user.</param>
        /// <param name="workingBudgetId">A working budget ID.</param>
        /// <param name="budgetAccountIds">A list of budget line item budget account IDs.</param>
        /// <param name="newBudgetAmounts">A list of budget line item new budget amounts.</param>
        /// <param name="justificationNotes">A list of budget line item justification notes.</param>
        /// <returns>A list of budget line item domain entities.</returns>
        Task<List<BudgetLineItem>> UpdateBudgetDevelopmentBudgetLineItemsAsync(string currentUserPersonId, string workingBudgetId, List<string> budgetAccountIds, List<long?> newBudgetAmounts, List<string> justificationNotes);

        /// <summary>
        /// Gets the list of budget officers for the working budget.
        /// </summary>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="currentUserPersonId">The user PERSON ID of the current user.</param>
        /// <returns>A list of budget officer domain entities for the working budget.</returns>
        Task<List<BudgetOfficer>> GetBudgetDevelopmentBudgetOfficersAsync(string workingBudgetId, string currentUserPersonId);

        /// <summary>
        /// Gets the list of reporting units for the user for the working budget.
        /// </summary>
        /// <param name="workingBudgetId">The working budget ID.</param>
        /// <param name="currentUserPersonId">The user PERSON ID of the current user.</param>
        /// <returns>A list of reporting unit domain entities for the user for the working budget.</returns>
        Task<List<BudgetReportingUnit>> GetBudgetDevelopmentReportingUnitsAsync(string workingBudgetId, string currentUserPersonId);
    }
}
