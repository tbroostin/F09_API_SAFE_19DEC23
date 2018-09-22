//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdmissionDecisions services
    /// </summary>
    public interface IAdmissionDecisionsService : IBaseService
    {
                /// <summary>
        /// Gets all admission-decisions
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AdmissionDecisions">admissionDecisions</see> objects</returns>          
         Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisions>, int>> GetAdmissionDecisionsAsync(int offset, int limit, string applicationId, bool bypassCache = false);
             
        /// <summary>
        /// Get a admissionDecisions by guid.
        /// </summary>
        /// <param name="guid">Guid of the admissionDecisions in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="AdmissionDecisions">admissionDecisions</see></returns>
        Task<Ellucian.Colleague.Dtos.AdmissionDecisions> GetAdmissionDecisionsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Creates admission decision.
        /// </summary>
        /// <param name="admissionDecisions"></param>
        /// <returns></returns>
        Task<AdmissionDecisions> CreateAdmissionDecisionAsync(AdmissionDecisions admissionDecisions);
    }
}
