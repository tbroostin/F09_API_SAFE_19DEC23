using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.FinancialAid;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// This interface defines the StudentAwardPeriodService
    /// </summary>
    public interface IStudentAwardPeriodService
    {
        /// <summary>
        /// Get all of a student's award periods across all award years.
        /// </summary>
        /// <param name="studentId">StudentId for which to get award data</param>
        /// <returns>A list of StudentAwardPeriod objects containing all of the student's award data</returns>
        IEnumerable<StudentAwardPeriod> Get(string studentId);

        /*
        StudentAwardPeriod AcceptStudentAwardPeriod(StudentAwardPeriod studentAwardPeriod);

        StudentAwardPeriod RejectStudentAwardPeriod(StudentAwardPeriod studentAwardPeriod);
         * */
    }
}
