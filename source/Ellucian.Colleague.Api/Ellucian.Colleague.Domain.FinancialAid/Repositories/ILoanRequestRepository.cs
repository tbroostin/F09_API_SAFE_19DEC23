/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Interface to a LoanRequestRepository to access a database and create domain entity objects
    /// </summary>
    public interface ILoanRequestRepository
    {
        /// <summary>
        /// Get a LoanRequest with the given id
        /// </summary>
        /// <param name="id">Id of the LoanRequest to get</param>
        /// <returns>LoanRequest object with the given id.</returns>
        Task<LoanRequest> GetLoanRequestAsync(string id);

        /// <summary>
        /// Create a LoanRequest record in the database. 
        /// </summary>
        /// <param name="loanRequest">The LoanRequest object containing the data with which to update the database</param>
        /// <returns>A new LoanRequest object successfully created from the new loan request database record</returns>
        Task<LoanRequest> CreateLoanRequestAsync(LoanRequest loanRequest, StudentAwardYear studentAwardYear);
    }
}
