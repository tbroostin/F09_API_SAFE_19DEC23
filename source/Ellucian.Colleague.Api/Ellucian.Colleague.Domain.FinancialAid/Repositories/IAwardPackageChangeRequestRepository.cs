/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    public interface IAwardPackageChangeRequestRepository
    {
        /// <summary>
        /// Gets AwardPackageChangeRequest entities
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <returns>AwardPackageChangeRequest</returns>
        Task<IEnumerable<AwardPackageChangeRequest>> GetAwardPackageChangeRequestsAsync(string studentId);
        /// <summary>
        /// Gets AwardPackageChangeRequests with a specific student award in mind
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="studentAward">student award</param>
        /// <returns>AwardPackageChangeRequest</returns>
        Task<IEnumerable<AwardPackageChangeRequest>> GetAwardPackageChangeRequestsAsync(string studentId, StudentAward studentAward);
        /// <summary>
        /// Creates an award package change request
        /// </summary>
        /// <param name="awardPackageChangeRequest">award package change request data</param>
        /// <param name="originalStudentAward"original student award></param>
        /// <returns>AwardPackageChangeRequest</returns>
        Task<AwardPackageChangeRequest> CreateAwardPackageChangeRequestAsync(AwardPackageChangeRequest awardPackageChangeRequest, StudentAward originalStudentAward);
    }
}
