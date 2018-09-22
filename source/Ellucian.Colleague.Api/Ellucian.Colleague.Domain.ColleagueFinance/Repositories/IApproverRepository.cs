// Copyright 2018 Ellucian Company L.P. and its affiliates.using System;

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Lists required methods.
    /// </summary>
    public interface IApproverRepository
    {
        /// <summary>
        /// Retrieves an approver validation response.
        /// </summary>
        /// <param name="approverId">Approver ID.</param>
        /// <returns>An approver validation response DTO</returns>
        Task<ApproverValidationResponse> ValidateApproverAsync(string approverId);
    }
}