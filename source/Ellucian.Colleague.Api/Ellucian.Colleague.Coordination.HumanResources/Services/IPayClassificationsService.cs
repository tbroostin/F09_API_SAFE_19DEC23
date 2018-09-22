//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for PayClassifications services
    /// </summary>
    public interface IPayClassificationsService : IBaseService
    {
          
        /// <summary>
        /// Gets all pay-classifications
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PayClassifications">payClassifications</see> objects</returns>
         Task<IEnumerable<Ellucian.Colleague.Dtos.PayClassifications>> GetPayClassificationsAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a payClassifications by guid.
        /// </summary>
        /// <param name="guid">Guid of the payClassifications in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PayClassifications">payClassifications</see></returns>
        Task<Ellucian.Colleague.Dtos.PayClassifications> GetPayClassificationsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Gets all pay-classifications
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PayClassifications2">payClassifications</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.PayClassifications2>> GetPayClassifications2Async(bool bypassCache = false);

        /// <summary>
        /// Get a payClassifications by guid.
        /// </summary>
        /// <param name="guid">Guid of the payClassifications in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PayClassifications2">payClassifications</see></returns>
        Task<Ellucian.Colleague.Dtos.PayClassifications2> GetPayClassificationsByGuid2Async(string guid, bool bypassCache = false);
   
    }
}
