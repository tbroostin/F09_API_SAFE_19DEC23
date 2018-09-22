/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface for a ProfileApplicationRepository class
    /// </summary>
    public interface IProfileApplicationRepository
    {
        /// <summary>
        /// Get all Profile Applications for the given student
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id for whom to get profile applications</param>
        /// <param name="studentAwardYears">The list of studentAwardYears for which to get profile applications for the student</param>
        /// <returns>A list of profile applications for the given student</returns>
        Task<IEnumerable<ProfileApplication>> GetProfileApplicationsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears);
    }
}
