// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Definition of methods that must be implemented for a GL cost center service.
    /// </summary>
    public interface ICostCenterService
    {
        /// <summary>
        /// Returns the GL cost center DTOs assigned to the user logged into Self-Service.
        /// These cost centers are only for expense GL accounts.
        /// </summary>
        /// <param name="fiscalYear">General Ledger fiscal year.</param>
        /// <returns>List of GL cost center DTOs for the specified fiscal year.</returns>
        [Obsolete("Obsolete as of API verson 1.29; use the QueryCostCenters endpoint")]
        Task<IEnumerable<CostCenter>> GetAsync(string fiscalYear);

        /// <summary>
        /// Returns the GL cost center DTO selected by the user.
        /// </summary>
        /// <param name="costCenterId">Cost center selected by the user.</param>
        /// <param name="fiscalYear">General Ledger fiscal year.</param>
        /// <returns>Cost center DTO for the specified fiscal year.</returns>
        Task<CostCenter> GetCostCenterAsync(string costCenterId, string fiscalYear);

        /// <summary>
        /// Returns the list of the General Ledger Fiscal Years available.
        /// </summary>
        /// <returns>List of GL fiscal years.</returns>
        Task<IEnumerable<string>> GetFiscalYearsAsync();

        /// <summary>
        /// Returns today's fiscal year based on the General Ledger configuration.
        /// </summary>
        /// <returns>Today's fiscal year</returns>
        Task<string> GetFiscalYearForTodayAsync();

        /// <summary>
        /// Returns the cost center DTOs assigned to the user based on filter criteria.
        /// </summary>
        /// <param name="criteria">Cost center filter criteria</param>
        /// <returns>Returns the cost center DTOs assigned to the user based on filter criteria.</returns>
        Task<IEnumerable<CostCenter>> QueryCostCentersAsync(CostCenterQueryCriteria criteria);
    }
}

