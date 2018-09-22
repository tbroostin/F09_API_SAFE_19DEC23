//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for Grants services
    /// </summary>
    public interface IGrantsService : IBaseService
    {
                /// <summary>
        /// Gets all grants
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="Grant">grant</see> objects</returns>          
         Task<Tuple<IEnumerable<Dtos.Grant>, int>> GetGrantsAsync(int offset, int limit, string reportingSegment = "", string fiscalYearId = "", bool bypassCache = false);
             
        /// <summary>
        /// Get a grant by guid.
        /// </summary>
        /// <param name="guid">Guid of the grants in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="Grant">grant</see></returns>
        Task<Dtos.Grant> GetGrantsByGuidAsync(string guid, bool bypassCache = true);            
    }
}
