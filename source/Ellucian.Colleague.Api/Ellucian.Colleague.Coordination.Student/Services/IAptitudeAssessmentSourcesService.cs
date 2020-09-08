//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AptitudeAssessmentSources services
    /// </summary>
    public interface IAptitudeAssessmentSourcesService : IBaseService
    {
          
        /// <summary>
        /// Gets all aptitude-assessment-sources
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AptitudeAssessmentSources">aptitudeAssessmentSources</see> objects</returns>
         Task<IEnumerable<Ellucian.Colleague.Dtos.AptitudeAssessmentSources>> GetAptitudeAssessmentSourcesAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a aptitudeAssessmentSources by guid.
        /// </summary>
        /// <param name="guid">Guid of the aptitudeAssessmentSources in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="AptitudeAssessmentSources">aptitudeAssessmentSources</see></returns>
        Task<Ellucian.Colleague.Dtos.AptitudeAssessmentSources> GetAptitudeAssessmentSourcesByGuidAsync(string guid, bool bypassCache = true);
            
    }
}
