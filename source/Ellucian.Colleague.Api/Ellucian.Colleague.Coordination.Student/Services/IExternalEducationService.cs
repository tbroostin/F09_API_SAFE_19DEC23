// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IExternalEducationService : IBaseService
    {
        /// <summary>
        /// Gets all external-education
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="personGuid">personGuid</param>
        /// <returns>Collection of <see cref="ExternalEducation">externalEducation</see> objects</returns> 
        Task<Tuple<IEnumerable<Dtos.ExternalEducation>, int>> GetExternalEducationsAsync(int offset, int limit, bool bypassCache = false, string personGuid = "");

        /// <summary>
        /// Get a externalEducation by guid.
        /// </summary>
        /// <param name="guid">Guid of the externalEducation in Colleague.</param>
        /// <returns>The <see cref="ExternalEducation">externalEducation</see></returns>
        Task<Dtos.ExternalEducation> GetExternalEducationByGuidAsync(string guid);

    }
}