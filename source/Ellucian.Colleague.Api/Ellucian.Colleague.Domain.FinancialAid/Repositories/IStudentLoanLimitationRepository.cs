//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{

    /// <summary>
    /// Interface defines the methods that must be implemented to get 
    /// StudentLoanLimitation objects from Colleague
    /// </summary>
    public interface IStudentLoanLimitationRepository
    {
        /// <summary>
        /// This gets all of the student's loan limitation objects from Colleague for all
        /// years that the student has award data
        /// </summary>
        /// <param name="studentId">The id of the student for whom to retrieve the loan limitations</param>
        /// <returns>A list of StudentLoanLimitation objects</returns>
        Task<IEnumerable<StudentLoanLimitation>> GetStudentLoanLimitationsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears);
    }
}
