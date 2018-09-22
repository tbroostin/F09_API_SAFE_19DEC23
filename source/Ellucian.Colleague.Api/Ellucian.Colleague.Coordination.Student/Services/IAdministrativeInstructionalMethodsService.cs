//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdministrativeInstructionalMethods services
    /// </summary>
    public interface IAdministrativeInstructionalMethodsService : IBaseService
    {
        /// <summary>
        /// Gets all administrative-instructional-methods
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AdministrativeInstructionalMethods">administrativeInstructionalMethods</see> objects</returns>          
        Task<IEnumerable<Ellucian.Colleague.Dtos.AdministrativeInstructionalMethods>> GetAdministrativeInstructionalMethodsAsync(bool bypassCache = false);

        /// <summary>
        /// Get an administrative-instructional-methods by guid.
        /// </summary>
        /// <param name="guid">Guid of administrativeInstructionalMethods in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="AdministrativeInstructionalMethods">administrativeInstructionalMethods</see></returns>
        Task<Ellucian.Colleague.Dtos.AdministrativeInstructionalMethods> GetAdministrativeInstructionalMethodsByGuidAsync(string guid, bool bypassCache = true);

            
    }
}
