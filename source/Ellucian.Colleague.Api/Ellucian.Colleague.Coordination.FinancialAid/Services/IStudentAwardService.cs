//Copyright 2013-2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface used by the injection framework. Defines the methods of a StudentAwardService
    /// </summary>
    public interface IStudentAwardService
    {
        /// <summary>
        /// Get all StudentAwards for all years the student has data
        /// </summary>
        /// <param name="studentId">The PERSON id of the student for whom to retrieve data </param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of StudentAward objects representing all of the student's award data</returns>
        Task<IEnumerable<StudentAward>> GetStudentAwardsAsync(string studentId, bool getActiveYearsOnly = false);

        /// <summary>
        /// Get all StudentAwards for a particular award year
        /// </summary>
        /// <param name="studentId">The PERSON id of the student for whom to retrieve data</param>
        /// <param name="year">The award year for which to retrieve StudentAwards</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of StudentAward objects representing one year's worth of the student's award data</returns>
        Task<IEnumerable<StudentAward>> GetStudentAwardsAsync(string studentId, string year, bool getActiveYearsOnly = false);



        /// <summary>
        /// Get a single StudentAward based on the given year and award id.
        /// </summary>
        /// <param name="studentId">The PERSON id of the student for whom to retrieve data</param>
        /// <param name="year">The award year from which to retrieve StudentAwards</param>
        /// <param name="awardId">The award Id of the StudentAward to return</param>
        /// <returns>The StudentAward object based on the given parameters</returns>
        Task<StudentAward> GetStudentAwardsAsync(string studentId, string year, string awardId);

        /// <summary>
        /// Update Colleague with the data contained in the given StudentAward objects. The 
        /// StudentAward objects must all have the same award year.
        /// </summary>
        /// <param name="studentAwards">A list of StudentAward objects representing one year's worth of award data.</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of StudentAward objects for one year's worth of award data containing updated data from Colleague.</returns>
        Task<IEnumerable<StudentAward>> UpdateStudentAwardsAsync(string studentId, string year, IEnumerable<StudentAward> studentAwards, bool getActiveYearsOnly = false);

        /// <summary>
        /// Update Colleague with the data contained in the given StudentAward object
        /// </summary>
        /// <param name="studentAward">StudentAward object containing the data with which to update Colleague</param>
        /// <returns>A StudentAward object containing updated data from Colleague</returns>
        Task<StudentAward> UpdateStudentAwardsAsync(string studentId, string year, string awardId, StudentAward studentAward);

        /// <summary>
        /// Select award data for multiple students
        /// </summary>
        /// <param name="criteria">Criteria DTO contains Student IDs, Award Year, and Term.</param>
        /// <returns>DTO Collection of Student Awards</returns>
        IEnumerable<StudentAwardSummary> QueryStudentAwardSummary(StudentAwardSummaryQueryCriteria criteria);



    }
}
