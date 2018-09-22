/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    public interface IAwardPackageChangeRequestService
    {
        /// <summary>
        /// Get a list of AwardPackageChangeRequests that were submitted by the given student
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        Task<IEnumerable<AwardPackageChangeRequest>> GetAwardPackageChangeRequestsAsync(string studentId);

        /// <summary>
        /// Get a single AwardPackageChangeRequest submitted by the given student with the given awardPackageChangeRequestId
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardPackageChangeRequestId"></param>
        /// <returns></returns>
        Task<AwardPackageChangeRequest> GetAwardPackageChangeRequestAsync(string studentId, string awardPackageChangeRequestId);

        /// <summary>
        /// Create an AwardPackageChangeRequest for the given student using the data in the newAwardPackageChangeRequest
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="newAwardPackageChangeRequest"></param>
        /// <returns></returns>
        Task<AwardPackageChangeRequest> CreateAwardPackageChangeRequestAsync(string studentId, AwardPackageChangeRequest newAwardPackageChangeRequest);
    }
}
