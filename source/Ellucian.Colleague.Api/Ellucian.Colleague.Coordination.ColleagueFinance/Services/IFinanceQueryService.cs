// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Definition of methods that must be implemented for a finance query service.
    /// </summary>
    public interface IFinanceQueryService
    {
        /// <summary>
        /// Retrieves the filtered GL account list based finance query filter criteria
        /// </summary>
        /// <param name="criteria">Finance query filter criteria.</param>
        /// <returns>GL accounts that match the filter criteria.</returns>       
        Task<IEnumerable<FinanceQuery>> QueryFinanceQuerySelectionByPostAsync(FinanceQueryCriteria criteria);
    }
}


