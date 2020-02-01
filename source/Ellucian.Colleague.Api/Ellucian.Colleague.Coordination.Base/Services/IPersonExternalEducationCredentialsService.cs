//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for PersonExternalEducationCredentials services
    /// </summary>
    public interface IPersonExternalEducationCredentialsService : IBaseService
    {
                /// <summary>
        /// Gets all person-external-education-credentials
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PersonExternalEducationCredentials">personExternalEducationCredentials</see> objects</returns>          
         Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials>, int>> GetPersonExternalEducationCredentialsAsync(int offset, int limit, string personFilter, bool bypassCache = false);
             
        /// <summary>
        /// Get a personExternalEducationCredentials by guid.
        /// </summary>
        /// <param name="guid">Guid of the personExternalEducationCredentials in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PersonExternalEducationCredentials">personExternalEducationCredentials</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials> GetPersonExternalEducationCredentialsByGuidAsync(string guid, bool bypassCache = false);

                /// <summary>
        /// Update a personExternalEducationCredentials.
        /// </summary>
        /// <param name="personExternalEducationCredentials">The <see cref="PersonExternalEducationCredentials">personExternalEducationCredentials</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="PersonExternalEducationCredentials">personExternalEducationCredentials</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials> UpdatePersonExternalEducationCredentialsAsync(Ellucian.Colleague.Dtos.PersonExternalEducationCredentials personExternalEducationCredentials);

        /// <summary>
        /// Create a personExternalEducationCredentials.
        /// </summary>
        /// <param name="personExternalEducationCredentials">The <see cref="PersonExternalEducationCredentials">personExternalEducationCredentials</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PersonExternalEducationCredentials">personExternalEducationCredentials</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials> CreatePersonExternalEducationCredentialsAsync(Ellucian.Colleague.Dtos.PersonExternalEducationCredentials personExternalEducationCredentials);
            
    }
}
