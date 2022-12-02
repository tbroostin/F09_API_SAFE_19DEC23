/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface defines the methods that must be implemented to get AwardYearCredits objects from Colleague
    /// </summary>
    public interface IFinancialAidCreditsRepository
    {
        /// <summary>
        /// This gets all of the student's Financial Aid course credits from Colleague for the FA years provided
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve course credits</param>
        /// <param name="studentAwardYears">The student's award years from which to retrieve course credits</param>
        /// <returns>A list of AwardYearCredits objects</returns>
        Task<List<AwardYearCredits>> GetStudentFinancialAidCreditsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears);
    }
}
