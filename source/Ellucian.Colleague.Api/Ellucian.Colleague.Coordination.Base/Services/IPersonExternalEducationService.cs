// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for PersonExternalEducation services
    /// </summary>
    public interface IPersonExternalEducationService : IBaseService
    {
        /// <summary>
        /// Gets all PersonExternalEducation DTOs
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="personExternalEducationFilter">filter criteria</param>
        /// <param name="personFilterGuid">person Filter - Guid for SAVE.LIST.PARMS which contains a savedlist of person IDs</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PersonExternalEducation">personExternalEducation</see> dto objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PersonExternalEducation>, int>> GetPersonExternalEducationAsync(int offset, int limit,
            Ellucian.Colleague.Dtos.PersonExternalEducation personExternalEducationFilter = null, string personFilterGuid = "", bool bypassCache = false);

        /// <summary>
        /// Get a PersonExternalEducation DTO by guid.
        /// </summary>
        /// <param name="guid">Guid of the externalEducation in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PesonExternalEducation">personExternalEducation</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonExternalEducation> GetPersonExternalEducationByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Create or Update a PersonExternalEducation.
        /// </summary>
        /// <param name="personExternalEducation">The <see cref="PersonExternalEducation">personExternalEducation</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="PersonExternalEducation">personExternalEducation</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonExternalEducation> CreateUpdatePersonExternalEducationAsync(Ellucian.Colleague.Dtos.PersonExternalEducation personExternalEducation, bool isUpdate);

    }
}