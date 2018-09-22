using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface for the StudentAwardPeriodRepository
    /// </summary>
    public interface IStudentAwardPeriodRepository
    {
        /// <summary>
        /// Get all student award periods by student id
        /// </summary>
        /// <param name="studentId">Student ID to get data for</param>
        /// <returns>List of StudentAwardPeriod objects for the given student id</returns>
        IEnumerable<StudentAwardPeriod> Get(string studentId);

        /*
        /// <summary>
        /// Update the Status of the student award period to an Accepted status.
        /// </summary>
        /// <param name="studentAwardPeriod">The student award period object to update</param>
        /// <returns>A student award period object with an Accepted status</returns>
        StudentAwardPeriod AcceptStudentAwardPeriod(StudentAwardPeriod studentAwardPeriod);

        /// <summary>
        /// Update the Status of the student award period to a Rejected status
        /// </summary>
        /// <param name="studentAwardPeriod">The student award period object to update</param>
        /// <returns>A student award period object with a Rejected status</returns>
        StudentAwardPeriod RejectStudentAwardPeriod(StudentAwardPeriod studentAwardPeriod);
         * */
    }
}
