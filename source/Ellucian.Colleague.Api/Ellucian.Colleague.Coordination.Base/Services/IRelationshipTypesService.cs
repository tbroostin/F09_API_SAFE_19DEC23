//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for RelationshipTypes services
    /// </summary>
    public interface IRelationshipTypesService : IBaseService
    {
        /// <summary>
        /// Gets all relationship-types
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="RelationshipTypes">relationshipTypes</see> objects</returns>          
        Task<IEnumerable<Ellucian.Colleague.Dtos.RelationshipTypes>> GetRelationshipTypesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a relationshipTypes by guid.
        /// </summary>
        /// <param name="guid">Guid of the relationshipTypes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="RelationshipTypes">relationshipTypes</see></returns>
        Task<Ellucian.Colleague.Dtos.RelationshipTypes> GetRelationshipTypesByGuidAsync(string guid, bool bypassCache = true);


    }
}
