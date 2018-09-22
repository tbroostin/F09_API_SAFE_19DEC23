/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface to a LoanRequestService
    /// </summary>
    public interface ILoanRequestService
    {
        /// <summary>
        /// Get a LoanRequest with the given Id
        /// </summary>
        /// <param name="id">Id of the loanRequest object to get</param>
        /// <returns>LoanRequest DTO with the given Id</returns>
        Task<LoanRequest> GetLoanRequestAsync(string id);

        /// <summary>
        /// Create a new LoanRequest
        /// </summary>
        /// <param name="loanRequest">A LoanRequest object containing the new data</param>
        /// <returns>A new LoanRequest object</returns>
        Task<LoanRequest> CreateLoanRequestAsync(LoanRequest loanRequest);
    }
}
