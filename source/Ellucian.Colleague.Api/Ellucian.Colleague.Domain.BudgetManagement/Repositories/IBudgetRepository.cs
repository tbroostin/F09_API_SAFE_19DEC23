// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.BudgetManagement.Repositories
{
    /// <summary>
    /// Repository for Budgets
    /// </summary>
    public interface IBudgetRepository
    {
        /// <summary>
        /// Get a budget phase by guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<Budget> GetBudgetPhasesAsync(string guid);

        /// <summary>
        /// Get a collection of budget phases
        /// </summary>
        /// <param name="budgetCode"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Budget>, int>> GetBudgetPhasesAsync(string budgetCode, bool bypassCache);

        /// <summary>
        /// Get a collection of budget phase line items
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="budgetCode"></param>
        /// <param name="accountingStringComponentValue"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<BudgetWork>, int>> GetBudgetPhaseLineItemsAsync(int limit, int offset, string budgetCode,
            string accountingStringComponentValue, bool bypassCache);


        /// <summary>
        /// Get budgets Guid from Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>string id</returns>
        Task<string> GetBudgetPhasesGuidFromIdAsync(string id);

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        Task<string> GetBudgetPhasesIdFromGuidAsync(string guid);

       
        /// <summary>
        /// Get a budget by guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<Budget> GetBudgetCodesAsync(string guid);

        /// <summary>
        /// Get a collection of budget codes
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Budget>, int>> GetBudgetCodesAsync(bool bypassCache);

        /// <summary>
        /// Get budget codes guid from id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>string id</returns>
        Task<string> GetBudgetCodesGuidFromIdAsync(string id);

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        Task<string> GetBudgetCodesIdFromGuidAsync(string guid);

        /// <summary>
        /// Get BudgetPhaseLineItem from guid
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns></returns>
        Task<BudgetWork> GetBudgetPhaseLineItemsByGuidAsync(string guid);

        /// <summary>
        /// Gets a dictionary of guids for Budget records.
        /// </summary>
        /// <param name="budgetIds"></param>
        /// <returns></returns>
        Task<IDictionary<string, string>> GetBudgetGuidCollectionAsync(IEnumerable<string> budgetIds);


    }
}
