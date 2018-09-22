//Copyright 2014-2017 Ellucian Company L.P. and its affiliates
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface defines the methods that must be implemented to get StudentAwardYear 
    /// objects from Colleague
    /// </summary>
    public interface IStudentAwardYearRepository
    {
        /// <summary>
        /// This gets all of the student's award years from Colleague
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve award years data</param>
        /// <param name="currentOfficeService">current Office Service</param>
        /// <param name="getActiveYearsOnly">Flag indicating whether to retrieve active award years only</param>
        /// <returns>A list of StudentAwardYear objects</returns>
        Task<IEnumerable<StudentAwardYear>> GetStudentAwardYearsAsync(string studentId, CurrentOfficeService currentOfficeService, bool getActiveYearsOnly = false);

        /// <summary>
        /// Get a single award year
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="awardYearCode">award year code</param>
        /// <param name="currentOfficeService">current office service</param>
        /// <returns>StudentAwardYear entity</returns>
        Task<StudentAwardYear> GetStudentAwardYearAsync(string studentId, string awardYearCode, CurrentOfficeService currentOfficeService);

        /// <summary>
        /// Updates paper copy option flag on the student's FIN.AID record
        /// </summary>
        /// <param name="studentAwardYear">student award year containing info</param>
        /// <returns>student award year entity</returns>
        StudentAwardYear UpdateStudentAwardYear(StudentAwardYear studentAwardYear);

        /// <summary>
        /// Updates paper copy option flag on the student's FIN.AID record
        /// </summary>
        /// <param name="studentAwardYear">student award year containing info</param>
        /// <returns>student award year entity</returns>
        Task<StudentAwardYear> UpdateStudentAwardYearAsync(StudentAwardYear studentAwardYear);

    }
}
