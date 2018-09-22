//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface for StudentLoanSummaryRepository
    /// </summary>
    public interface IStudentLoanSummaryRepository
    {
        /// <summary>
        /// Get a StudentLoanSummary object for the given student id
        /// </summary>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <returns>StudentLoanSummary object containing data derived from the Colleague database</returns>
        Task<StudentLoanSummary> GetStudentLoanSummaryAsync(string studentId);
    }
}
