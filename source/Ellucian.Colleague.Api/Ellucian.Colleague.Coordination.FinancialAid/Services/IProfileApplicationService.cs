/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface to a ProfileApplicationService class
    /// </summary>
    public interface IProfileApplicationService
    {
        /// <summary>
        /// Get the ProfileApplications for the given student
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom ProfileApplications are being retrieved.</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of ProfileApplication DTOs for the given student id</returns>
        Task<IEnumerable<ProfileApplication>> GetProfileApplicationsAsync(string studentId, bool getActiveYearsOnly = false);
    }
}
