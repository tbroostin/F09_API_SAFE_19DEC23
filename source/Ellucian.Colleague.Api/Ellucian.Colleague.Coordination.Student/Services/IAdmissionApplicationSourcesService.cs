//Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdmissionApplicationSources services
    /// </summary>
    public interface IAdmissionApplicationSourcesService : IBaseService
    {

        /// <summary>
        /// Gets all admission-application-sources
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AdmissionApplicationSources">admissionApplicationSources</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSources>> GetAdmissionApplicationSourcesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a admissionApplicationSources by guid.
        /// </summary>
        /// <param name="guid">Guid of the admissionApplicationSources in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="AdmissionApplicationSources">admissionApplicationSources</see></returns>
        Task<Ellucian.Colleague.Dtos.AdmissionApplicationSources> GetAdmissionApplicationSourcesByGuidAsync(string guid, bool bypassCache = true);


    }
}
