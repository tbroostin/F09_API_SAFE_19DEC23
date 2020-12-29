//Copyright 2019 Ellucian Company L.P. and its affiliates.EmergencyContactPhoneAvailabilities

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for EmergencyContactTypes services
    /// </summary>
    public interface IEmergencyContactTypesService : IBaseService
    {

        /// <summary>
        /// Gets all emergency-contact-types
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="EmergencyContactTypes">emergencyContactTypes</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.EmergencyContactTypes>> GetEmergencyContactTypesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a emergencyContactTypes by guid.
        /// </summary>
        /// <param name="guid">Guid of the emergencyContactTypes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="EmergencyContactTypes">emergencyContactTypes</see></returns>
        Task<Ellucian.Colleague.Dtos.EmergencyContactTypes> GetEmergencyContactTypesByGuidAsync(string guid, bool bypassCache = true);


    }
}
