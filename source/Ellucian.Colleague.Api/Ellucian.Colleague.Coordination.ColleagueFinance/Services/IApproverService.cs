// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Collections.Generic;
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

        /// <summary>
        /// Retrieves an next approvers from keyword search and responses DTO.
        /// </summary>
        /// <param name="queryKeyword"></param>
        /// <returns>list of next approver</returns>
        Task<IEnumerable<Dtos.ColleagueFinance.NextApprover>> QueryNextApproverByKeywordAsync(string queryKeyword);
    }
}