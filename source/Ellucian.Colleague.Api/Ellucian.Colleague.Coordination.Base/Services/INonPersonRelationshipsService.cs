//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for NonPersonRelationships services
    /// </summary>
    public interface INonPersonRelationshipsService : IBaseService
    {
        /// <summary>
        /// Gets all person-relationships
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="NonPersonRelationships">personRelationships</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.NonPersonRelationships>, int>> GetNonPersonRelationshipsAsync(int offset, int limit, string organization = "", string institution = "", string person = "", string relationType = "", bool bypassCache = false);

        /// <summary>
        /// Get a personRelationships by guid.
        /// </summary>
        /// <param name="guid">Guid of the personRelationships in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="NonPersonRelationships">personRelationships</see></returns>
        Task<Ellucian.Colleague.Dtos.NonPersonRelationships> GetNonPersonRelationshipsByGuidAsync(string guid, bool bypassCache = true);
   }
}
