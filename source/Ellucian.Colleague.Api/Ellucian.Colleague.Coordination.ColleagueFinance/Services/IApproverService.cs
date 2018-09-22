// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    public interface IApproverService
    {
        /// <summary>
        /// Retrieves an approver validation response DTO.
        /// </summary>
        /// <param name="approverId">Approver ID.</param>
        /// <returns>Approver validation response DTO.</returns>
        Task<Dtos.ColleagueFinance.NextApproverValidationResponse> ValidateApproverAsync(string approverId);
    }
}