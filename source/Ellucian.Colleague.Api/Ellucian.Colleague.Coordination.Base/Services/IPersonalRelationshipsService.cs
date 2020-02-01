// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPersonalRelationshipsService : IBaseService
    {
        Task<Tuple<IEnumerable<Dtos.PersonalRelationship>, int>> GetAllPersonalRelationshipsAsync(int offset, int limit, bool bypassCache);
        Task<Dtos.PersonalRelationship> GetPersonalRelationshipByIdAsync(string id);
        Task<Tuple<IEnumerable<Dtos.PersonalRelationship>, int>> GetPersonalRelationshipsByFilterAsync(int offset, int limit, string subjectPerson = "", 
            string relatedPerson = "", string directRelationshipType = "", string directRelationshipDetailId = "");
        Task<Dtos.PersonalRelationship> UpdatePersonalRelationshipAsync(string id, Dtos.PersonalRelationship personalRelationship);
        Task<Dtos.PersonalRelationship> CreatePersonalRelationshipAsync(Dtos.PersonalRelationship personalRelationship);

        /// <summary>
        /// Gets all personal-relationships
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PersonalRelationships2">personalRelationships</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonalRelationships2>, int>> GetPersonalRelationships2Async(int offset, int limit, string person = "", string relationType = "", string personFilterValue = "", bool bypassCache = false);

        /// <summary>
        /// Get a personalRelationships by guid.
        /// </summary>
        /// <param name="guid">Guid of the personRelationships in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PersonalRelationships2">personalRelationships</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonalRelationships2> GetPersonalRelationships2ByGuidAsync(string guid, bool bypassCache = true);

        /// Update a personalRelationships.
        /// </summary>
        /// <param name="personRelationships">The <see cref="PersonalRelationships2">personalRelationships</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="PersonalRelationships2">personRelationships</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonalRelationships2> UpdatePersonalRelationships2Async(Ellucian.Colleague.Dtos.PersonalRelationships2 personRelationships);

        /// <summary>
        /// Create a personalRelationships.
        /// </summary>
        /// <param name="personRelationships">The <see cref="PersonalRelationships2">personalRelationships</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PersonalRelationships2">personRelationships</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonalRelationships2> CreatePersonalRelationships2Async(Ellucian.Colleague.Dtos.PersonalRelationships2 personRelationships);

        /// <summary>
        /// Delete a personalRelationships by guid.
        /// </summary>
        /// <param name="guid">Guid of the personalRelationships in Colleague.</param>
        Task DeletePersonalRelationshipsAsync(string guid);
    }
}
