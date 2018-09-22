



//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for FiscalYears services
    /// </summary>
    public interface IFiscalYearsService : IBaseService
    {

        /// <summary>
        /// Gets all fiscal-years
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="reportingSegment">reportingSegment filter</param>
        /// <returns>Collection of <see cref="FiscalYears">fiscalYears</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.FiscalYears>> GetFiscalYearsAsync(bool bypassCache = false, string reportingSegment = "");

        /// <summary>
        /// Get a fiscalYears by guid.
        /// </summary>
        /// <param name="guid">Guid of the fiscalYears in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="FiscalYears">fiscalYears</see></returns>
        Task<Ellucian.Colleague.Dtos.FiscalYears> GetFiscalYearsByGuidAsync(string guid, bool bypassCache = false);

    }
}