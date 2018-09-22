//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for LedgerActivities services
    /// </summary>
    public interface ILedgerActivityService : IBaseService
    {
                /// <summary>
        /// Gets all ledger-activities
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="LedgerActivities">ledgerActivities</see> objects</returns>          
         Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.LedgerActivity>, int>> GetLedgerActivitiesAsync(int offset, int limit, string fiscalYear, string fiscalPeriod, string reportingSegment, string transactionDate, bool bypassCache = false);

        /// <summary>
        /// Get a ledgerActivities by guid.
        /// </summary>
        /// <param name="guid">Guid of the ledgerActivities in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="LedgerActivity">ledgerActivities</see></returns>
        Task<Ellucian.Colleague.Dtos.LedgerActivity> GetLedgerActivityByGuidAsync(string guid, bool bypassCache = false);
    }
}
