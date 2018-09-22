//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for PersonRelationships services
    /// </summary>
    public interface IPersonRelationshipsService : IBaseService
    {
        /// <summary>
        /// Gets all person-relationships
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PersonRelationships">personRelationships</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonRelationships>, int>> GetPersonRelationshipsAsync(int offset, int limit, string person = "", string relationType = "", bool bypassCache = false);

        /// <summary>
        /// Get a personRelationships by guid.
        /// </summary>
        /// <param name="guid">Guid of the personRelationships in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PersonRelationships">personRelationships</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonRelationships> GetPersonRelationshipsByGuidAsync(string guid, bool bypassCache = true);

        /// Update a personRelationships.
        /// </summary>
        /// <param name="personRelationships">The <see cref="PersonRelationships">personRelationships</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="PersonRelationships">personRelationships</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonRelationships> UpdatePersonRelationshipsAsync(Ellucian.Colleague.Dtos.PersonRelationships personRelationships);

        /// <summary>
        /// Create a personRelationships.
        /// </summary>
        /// <param name="personRelationships">The <see cref="PersonRelationships">personRelationships</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PersonRelationships">personRelationships</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonRelationships> CreatePersonRelationshipsAsync(Ellucian.Colleague.Dtos.PersonRelationships personRelationships);

        /// <summary>
        /// Delete a personRelationships by guid.
        /// </summary>
        /// <param name="guid">Guid of the personRelationships in Colleague.</param>
        Task DeletePersonRelationshipsAsync(string guid);

    }
}
