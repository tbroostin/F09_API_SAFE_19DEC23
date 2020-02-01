//Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for PersonSources services
    /// </summary>
    public interface IPersonSourcesService : IBaseService
    {

        /// <summary>
        /// Gets all person-sources
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PersonSources">personSources</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.PersonSources>> GetPersonSourcesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a personSources by guid.
        /// </summary>
        /// <param name="guid">Guid of the personSources in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PersonSources">personSources</see></returns>
        Task<Ellucian.Colleague.Dtos.PersonSources> GetPersonSourcesByGuidAsync(string guid, bool bypassCache = true);


    }
}
