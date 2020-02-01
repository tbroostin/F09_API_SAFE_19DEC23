/*Copyright 2015-2019 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface for a StudentBudgetComponentService
    /// </summary>
    public interface IStudentBudgetComponentService
    {
        /// <summary>
        /// Get all StudentBudgetComponents for the given studentId for all award years
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get budget components</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of StudentBudgetComponents</returns>
        Task<IEnumerable<StudentBudgetComponent>> GetStudentBudgetComponentsAsync(string studentId, bool getActiveYearsOnly = false);

        /// <summary>
        /// Get all StudentBudgetComponents for the given studentId and award year
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get budget components</param>
        /// <param name="awardYear">award year to get data for</param>
        /// <returns>A list of StudentBudgetComponents</returns>
        Task<IEnumerable<StudentBudgetComponent>> GetStudentBudgetComponentsForYearAsync(string studentId, string awardYear);
    }
}
