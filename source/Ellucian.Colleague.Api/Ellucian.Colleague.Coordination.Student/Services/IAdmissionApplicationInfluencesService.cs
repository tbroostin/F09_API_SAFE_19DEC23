//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdmissionApplicationInfluences services
    /// </summary>
    public interface IAdmissionApplicationInfluencesService : IBaseService
    {

        /// <summary>
        /// Gets all admission-application-influences
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AdmissionApplicationInfluences">admissionApplicationInfluences</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationInfluences>> GetAdmissionApplicationInfluencesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a admissionApplicationInfluences by guid.
        /// </summary>
        /// <param name="guid">Guid of the admissionApplicationInfluences in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="AdmissionApplicationInfluences">admissionApplicationInfluences</see></returns>
        Task<Ellucian.Colleague.Dtos.AdmissionApplicationInfluences> GetAdmissionApplicationInfluencesByGuidAsync(string guid, bool bypassCache = true);


    }
}