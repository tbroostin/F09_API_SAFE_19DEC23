// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Definition of methods that must be implemented for a GL object code service.
    /// </summary>
    public interface IGlObjectCodeService
    {
        /// <summary>
        /// Retrieves the filtered GL object codes. Uses the filter criteria selected in
        /// the cost centers view because the filter has to persist on the object view
        /// </summary>
        /// <param name="criteria">Cost center filter criteria</param>
        /// <returns>GL object codes DTOs that match the filter criteria.</returns>
        Task<IEnumerable<GlObjectCode>> QueryGlObjectCodesAsync(CostCenterQueryCriteria criteria);

        /// <summary>
        /// Returns the list of the General Ledger Fiscal Years available.
        /// </summary>
        /// <returns>List of GL fiscal years.</returns>
        Task<IEnumerable<string>> GetFiscalYearsAsync();
    }
}

