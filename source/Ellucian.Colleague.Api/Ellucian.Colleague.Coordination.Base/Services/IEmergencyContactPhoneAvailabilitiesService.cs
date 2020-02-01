//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for EmergencyContactPhoneAvailabilities services
    /// </summary>
    public interface IEmergencyContactPhoneAvailabilitiesService : IBaseService
    {

        /// <summary>
        /// Gets all emergency-contact-phone-availabilities
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="EmergencyContactPhoneAvailabilities">emergencyContactPhoneAvailabilities</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.EmergencyContactPhoneAvailabilities>> GetEmergencyContactPhoneAvailabilitiesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a emergencyContactPhoneAvailabilities by guid.
        /// </summary>
        /// <param name="guid">Guid of the emergencyContactPhoneAvailabilities in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="EmergencyContactPhoneAvailabilities">emergencyContactPhoneAvailabilities</see></returns>
        Task<Ellucian.Colleague.Dtos.EmergencyContactPhoneAvailabilities> GetEmergencyContactPhoneAvailabilitiesByGuidAsync(string guid, bool bypassCache = true);


    }
}
