//Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface to the repository
    /// </summary>
    public interface IAverageAwardPackageRepository
    {
        /// <summary>
        /// Get a list of AverageAwardPackage records for the given student id
        /// </summary>
        /// <param name="studentId">studentId</param>
        /// <param name="studentAwardYears">list of active student award years</param>
        /// <returns>A list of AverageAwardPackage records</returns>
        Task<IEnumerable<AverageAwardPackage>> GetAverageAwardPackagesAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears);
    }
}
