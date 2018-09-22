//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface for a StudentLoanSummaryService.
    /// </summary>
    public interface IStudentLoanSummaryService
    {
        /// <summary>
        /// Get a StudentLoanSummary DTO object for the given student Id
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <returns>StudentLoanSummary object containing data derived from the Colleague database</returns>
        Task<StudentLoanSummary> GetStudentLoanSummaryAsync(string studentId);
    }
}
