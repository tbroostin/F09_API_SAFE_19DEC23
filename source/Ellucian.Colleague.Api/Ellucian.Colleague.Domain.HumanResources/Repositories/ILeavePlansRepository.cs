/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface ILeavePlansRepository
    {
        /// <summary>
        /// Get LeavePlans objects for all LeavePlans bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        
        /// <returns>Tuple of LeavePlan Entity objects <see cref="LeavePlan"/> and a count for paging.</returns>
        Task<Tuple<IEnumerable<LeavePlan>, int>> GetLeavePlansAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get LeavePlans objects for a specific Id.
        /// </summary>   
        /// <param name="id">guid of the LeavePlans record.</param>
        /// <returns>LeavePlan Entity <see cref="LeavePlan"./></returns>
        Task<LeavePlan> GetLeavePlansByIdAsync(string id);

        /// <summary>
        /// Get LeavePlans entities for all leaveplans
        /// </summary>   
        /// <param name="id">guid of the LeavePlans record.</param>
        /// <returns>LeavePlan Entities <see cref="LeavePlan"./></returns>
        Task<IEnumerable<LeavePlan>> GetLeavePlansAsync(bool bypassCache);

        /// <summary>
        /// This method gets the same leave plans as the original version, but this method has a try/catch around each
        /// BuildLeavePlan step, so that we catch individual problems and still return the rest.
        /// 
        /// Also the cache key is different
        /// </summary>
        /// <param name="bypassCache">If you want to bypass the cache</param>
        /// <returns></returns>
        Task<IEnumerable<LeavePlan>> GetLeavePlansV2Async(bool bypassCache);


    }
}
