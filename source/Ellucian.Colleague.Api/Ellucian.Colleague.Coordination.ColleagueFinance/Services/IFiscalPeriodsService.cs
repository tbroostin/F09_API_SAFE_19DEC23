//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for FiscalPeriods services
    /// </summary>
    public interface IFiscalPeriodsService : IBaseService
    {

        /// <summary>
        /// Gets all fiscal-periods
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="fiscalYear">fiscalYear filter</param>
        /// <returns>Collection of <see cref="fiscalYear">fiscalYear</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.FiscalPeriods>> GetFiscalPeriodsAsync(bool bypassCache = false, string fiscalYear = "");

        /// <summary>
        /// Get a fiscalPeriods by guid.
        /// </summary>
        /// <param name="guid">Guid of the fiscalPeriods in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="FiscalPeriods">fiscalPeriods</see></returns>
        Task<Ellucian.Colleague.Dtos.FiscalPeriods> GetFiscalPeriodsByGuidAsync(string guid, bool bypassCache = false);

    }
}