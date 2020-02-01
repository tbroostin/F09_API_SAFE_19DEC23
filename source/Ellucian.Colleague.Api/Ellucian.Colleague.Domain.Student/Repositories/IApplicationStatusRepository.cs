using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface to an Applicant Repository
    /// </summary>
    public interface IApplicationStatusRepository : IEthosExtended
    {
        /// <summary>
        /// Get an admission decision by id.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<ApplicationStatus2> GetApplicationStatusByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Get an admission decisions All/Filter.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="applicationId"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<ApplicationStatus2>, int>> GetApplicationStatusesAsync(int offset, int limit, string applicationId,
            string[] filterPersonIds = null, DateTimeOffset? convertedDecidedOn = null, Dictionary<string, string> filterQualifiers = null, bool bypassCache = false);

        /// <summary>
        /// Gets Entity, PrimaryKey & Secondarykey.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<Tuple<string, string, string>> GetApplicationStatusKey(string guid);

        /// <summary>
        /// Creates admission decision.
        /// </summary>
        /// <param name="appStatusEntity"></param>
        /// <returns></returns>
        Task<ApplicationStatus2> UpdateAdmissionDecisionAsync(ApplicationStatus2 appStatusEntity);
    }
}
