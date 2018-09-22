//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for LeavePlans services
    /// </summary>
    public interface ILeavePlansService : IBaseService
    {
        /// <summary>
        /// Gets all leave-plans
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="LeavePlans">leavePlans</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.LeavePlans>, int>> GetLeavePlansAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get a leavePlans by guid.
        /// </summary>
        /// <param name="guid">Guid of the leavePlans in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="LeavePlans">leavePlans</see></returns>
        Task<Ellucian.Colleague.Dtos.LeavePlans> GetLeavePlansByGuidAsync(string guid, bool bypassCache = false);


    }
}
