//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface for a StudentLoanLimitationService
    /// </summary>
    public interface IStudentLoanLimitationService
    {
        /// <summary>
        /// This gets all of the student's loan limitation objects from Colleague for all
        /// years that the student has award data
        /// </summary>
        /// <param name="studentId">The id of the student for whom to retrieve the loan limitations</param>
        /// <returns>A list of StudentLoanLimitation DTO objects</returns>
        Task<IEnumerable<StudentLoanLimitation>> GetStudentLoanLimitationsAsync(string studentId);
        
        //IEnumerable<StudentLoanLimitation> GetAllLimitations(string studentId, StudentAwardYear studentAwardYear);

        Task<StudentLoanLimitation> GetLimitationAsync(string studentId, string year);


    }
}
