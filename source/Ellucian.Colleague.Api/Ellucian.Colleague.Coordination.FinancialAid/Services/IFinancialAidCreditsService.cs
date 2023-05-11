//Copyright 2022 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface used by the injection framework. Defines the methods of a FinancialAidCreditsService
    /// </summary>
    public interface IFinancialAidCreditsService
    {
        /// <summary>
        /// Get all Financial Aid Credits for all years the student has data
        /// </summary>
        /// <param name="studentId">The PERSON id of the student for whom to retrieve data </param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only, defaults to true</param>
        /// <returns>A list of AwardYearCredits objects representing all of the student's award data</returns>
        Task<List<Domain.FinancialAid.Entities.AwardYearCredits>> GetFinancialAidCreditsAsync(string studentId, bool getActiveYearsOnly = true);

    }
}
