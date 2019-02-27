// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IGenderIdentityTypeService : IBaseService
    {

        /// <summary>
        /// Get gender identity types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.GenderIdentityType">GenderIdentityType</see> items consisting of code and description</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.GenderIdentityType>> GetBaseGenderIdentityTypesAsync(bool ignoreCache);

        /// <summary>
        /// Gets all gender-identities
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="GenderIdentities">genderIdentities</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.GenderIdentities>> GetGenderIdentitiesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a genderIdentities by guid.
        /// </summary>
        /// <param name="guid">Guid of the genderIdentities in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="GenderIdentities">genderIdentities</see></returns>
        Task<Ellucian.Colleague.Dtos.GenderIdentities> GetGenderIdentitiesByGuidAsync(string guid, bool bypassCache = true);

    }
}
