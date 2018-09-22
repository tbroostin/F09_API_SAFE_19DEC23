//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for BudgetPhases services
    /// </summary>
    public interface IBudgetPhasesService : IBaseService
    {
        /// <summary>
        /// Gets all budget-phases
        /// </summary>
        /// <param name="budgetCode">Filter for budgetCode</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="BudgetPhases">budgetPhases</see> objects</returns>          
        Task<IEnumerable<Ellucian.Colleague.Dtos.BudgetPhases>> GetBudgetPhasesAsync(string budgetCode, 
            bool bypassCache = false);

        /// <summary>
        /// Get a budgetPhases by guid.
        /// </summary>
        /// <param name="guid">Guid of the budgetPhases in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="BudgetPhases">budgetPhases</see></returns>
        Task<Ellucian.Colleague.Dtos.BudgetPhases> GetBudgetPhasesByGuidAsync(string guid, bool bypassCache = true);


    }
}