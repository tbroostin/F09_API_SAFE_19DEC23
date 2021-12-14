//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IJobApplicationsRepository
    {
       

        /// <summary>
        /// Get a collection of JobApplication
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of JobApplication</returns>
        Task<Tuple<IEnumerable<JobApplication>, int>> GetJobApplicationsAsync(int offset, int limit, bool bypassCache = false);
        
        /// <summary>
        /// Returns a review for a specified Employment Performance Reviews key.
        /// </summary>
        /// <param name="ids">Key to Employment Performance Reviews to be returned</param>
        /// <returns>JobApplication Objects</returns>
        Task<JobApplication> GetJobApplicationByIdAsync(string id);
    }
}
