//Copyright 2014-2017 Ellucian Company L.P. and its affiliates
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface for the AverageAwardPackageService
    /// </summary>
    public interface IAverageAwardPackageService
    {
        /// <summary>
        /// Get a set of AverageAwardPackage DTO objects for the given student id
        /// for the student award years active on their record
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON Id</param>
        /// <returns>List of AverageAwardPackage objects</returns>
        Task<IEnumerable<AverageAwardPackage>> GetAverageAwardPackagesAsync(string studentId);
    }
}
