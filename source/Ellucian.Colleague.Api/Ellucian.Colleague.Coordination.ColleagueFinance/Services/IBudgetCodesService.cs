//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for BudgetCodes services
    /// </summary>
    public interface IBudgetCodesService : IBaseService
    {
        /// <summary>
        /// Gets all budget-codes
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="BudgetCodes">budgetCodes</see> objects</returns>          
        Task<IEnumerable<Ellucian.Colleague.Dtos.BudgetCodes>> GetBudgetCodesAsync(
            bool bypassCache = false);

        /// <summary>
        /// Get a budgetCodes by guid.
        /// </summary>
        /// <param name="guid">Guid of the budgetCodes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="BudgetCodes">budgetCodes</see></returns>
        Task<Ellucian.Colleague.Dtos.BudgetCodes> GetBudgetCodesByGuidAsync(string guid, bool bypassCache = true);
    }
}