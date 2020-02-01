//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for PersonMatchingRequests services
    /// </summary>
    public interface IPersonMatchingRequestsService : IBaseService
    {
        /// <summary>
        /// Gets all prospect-opportunities
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PersonMatchingRequests">personMatchingRequests</see> objects</returns>          
         Task<Tuple<IEnumerable<Dtos.PersonMatchingRequests>, int>> GetPersonMatchingRequestsAsync(int offset, int limit, Dtos.PersonMatchingRequests criteria, 
             string personFilter, bool bypassCache = false);
             
        /// <summary>
        /// Get a personMatchingRequests by guid.
        /// </summary>
        /// <param name="guid">Guid of the personMatchingRequests in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PersonMatchingRequests">personMatchingRequests</see></returns>
        Task<Dtos.PersonMatchingRequests> GetPersonMatchingRequestsByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Create a PersonMatchingRequestsInitiationsProspects.
        /// </summary>
        /// <param name="personMatchingRequestsInitiationsProspects">The <see cref="PersonMatchingRequestsInitiationsProspects">personMatchingRequestsInitiationsProspects</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PersonMatchingRequests">PersonMatchingRequests</see></returns>
        Task<Dtos.PersonMatchingRequests> CreatePersonMatchingRequestsInitiationsProspectsAsync(Dtos.PersonMatchingRequestsInitiationsProspects personMatchingRequestsInitiationsProspects);
    }
}
