/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface defines the methods that must be implemented to get and update
    /// StudentAward objects from Colleague
    /// </summary>
    public interface IStudentAwardRepository
    {
        /// <summary>
        /// This gets all of the student's awards from Colleague for all years the student has award data.
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve award data</param>
        /// <param name="studentAwardYears">The student's award years from which to retrieve award data</param>
        /// <param name="allAwards">A collection of all Award objects from Colleague</param>
        /// <param name="allAwardStatuses">A collection of all AwardStatus objects from Colleague</param>
        /// <returns>A list of StudentAward objects</returns>
        Task<IEnumerable<StudentAward>> GetAllStudentAwardsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses);

        /// <summary>
        /// Gets a list of StudentAward objects for a given award year.
        /// </summary>
        /// <param name="studentId">The StudentId</param>
        /// <param name="awardYear">The AwardYear</param>
        /// <param name="allAwards">A collection of all Award objects from Colleague</param>
        /// <param name="allAwardStatuses">A collection of all AwardStatus objects from Colleague</param>
        /// <returns>A list of StudentAward objects for the given awardYear parameter</returns>
        IEnumerable<StudentAward> GetStudentAwardSummaryForYear(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses);

        /// <summary>
        /// Gets a list of StudentAward objects for a given award year.
        /// </summary>
        /// <param name="studentId">The StudentId</param>
        /// <param name="awardYear">The AwardYear</param>
        /// <param name="allAwards">A collection of all Award objects from Colleague</param>
        /// <param name="allAwardStatuses">A collection of all AwardStatus objects from Colleague</param>
        /// <returns>A list of StudentAward objects for the given awardYear parameter</returns>
        Task<IEnumerable<StudentAward>> GetStudentAwardsForYearAsync(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses);

        /// <summary>
        /// Gets a single student award
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="studentAwardYear">student award year</param>
        /// <param name="awardCode">award code</param>
        /// <param name="allAwards">reference awards</param>
        /// <param name="allAwardStatuses">reference award statuses</param>
        /// <returns>StudentAward entity</returns>
        Task<StudentAward> GetStudentAwardAsync(string studentId, StudentAwardYear studentAwardYear, string awardCode, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardYear"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetCFPVersionAsync(string studentId, string awardYear);

        /// <summary>
        /// Updates student awards with received data
        /// </summary>
        /// <param name="studentAwardYear">student award year awards are associated with</param>
        /// <param name="studentAwards">student awards</param>
        /// <param name="allAwards">reference awards</param>
        /// <param name="allAwardStatuses">reference award statuses</param>
        /// <returns>List of StudentAward entities</returns>
        Task<IEnumerable<StudentAward>> UpdateStudentAwardsAsync(StudentAwardYear studentAwardYear, IEnumerable<StudentAward> studentAwards, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses);

    }
}
