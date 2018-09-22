//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface for a StudentAwardYearService
    /// </summary>
    public interface IStudentAwardYearService
    {
        /// <summary>
        /// Retrieve all of the student's financial aid award years
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve award year data</param>
        /// <returns>A list of StudentAwardYear DTO objects</returns>
        [Obsolete("Obsolete as of API 1.8. Use GetStudentAwardYears2")]
        IEnumerable<StudentAwardYear> GetStudentAwardYears(string studentId);

        /// <summary>
        /// Retrieve all of the student's financial aid award years
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve award year data</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of StudentAwardYear2 DTO objects</returns>
        Task<IEnumerable<StudentAwardYear2>> GetStudentAwardYears2Async(string studentId, bool getActiveYearsOnly = false);

        /// <summary>
        /// Retrieve the specified financial aid award year
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve award year data</param>
        /// <param name="awardYearCode">The award year code for which to retrieve the data</param>
        /// <returns>StudentAwardYear DTO</returns>
        [Obsolete("Obsolete as of API 1.8. Use GetStudentAwardYear2")]
        StudentAwardYear GetStudentAwardYear(string studentId, string awardYearCode);

        /// <summary>
        /// Retrieve the specified financial aid award year
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve award year data</param>
        /// <param name="awardYearCode">The award year code for which to retrieve the data</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>StudentAwardYear2 DTO</returns>
        Task<StudentAwardYear2> GetStudentAwardYear2Async(string studentId, string awardYearCode, bool getActiveYearsOnly = false);

        /// <summary>
        /// Update the paper copy option flag
        /// </summary>
        /// <param name="studentAwardYear">student award year object containing the info</param>
        /// <returns>student award year dto</returns>
        [Obsolete("Obsolete as of API 1.8. Use UpdateStudentAwardYear2")]
        StudentAwardYear UpdateStudentAwardYear(Dtos.FinancialAid.StudentAwardYear studentAwardYear);
        
        /// <summary>
        /// Update the paper copy option flag
        /// </summary>
        /// <param name="studentAwardYear">student award year object containing the info</param>
        /// <returns>StudentAwardYear2 dto</returns>
        Task<StudentAwardYear2> UpdateStudentAwardYear2Async(Dtos.Student.StudentAwardYear2 studentAwardYear);
    }
}
