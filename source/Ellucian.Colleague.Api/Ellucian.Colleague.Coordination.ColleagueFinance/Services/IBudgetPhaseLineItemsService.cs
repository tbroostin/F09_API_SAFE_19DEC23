//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for BudgetPhaseLineItems services
    /// </summary>
    public interface IBudgetPhaseLineItemsService : IBaseService
    {
        /// <summary>
        /// Gets all budget-phase-line-items
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="budgetPhaseGuid">filter for budgetPhases</param>
        /// <param name="accountingStringComponentValuesGuids">filter for accountingStringComponentValues</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="BudgetPhaseLineItems">budgetPhaseLineItems</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.BudgetPhaseLineItems>, int>> GetBudgetPhaseLineItemsAsync(int offset, int limit, 
            string budgetPhaseGuid,
            List<string> accountingStringComponentValuesGuids,
            bool bypassCache = false);

        /// <summary>
        /// Get a budgetPhaseLineItems by guid.
        /// </summary>
        /// <param name="guid">Guid of the budgetPhaseLineItems in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="BudgetPhaseLineItems">budgetPhaseLineItems</see></returns>
        Task<Ellucian.Colleague.Dtos.BudgetPhaseLineItems> GetBudgetPhaseLineItemsByGuidAsync(string guid, bool bypassCache = true);

    }
}