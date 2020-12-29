// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.using System;

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Get an approver name given an approver ID.
        /// </summary>
        /// <param name="approverId">The approver ID.</param>
        /// <returns>An approver name or empty string.</returns>
        Task<String> GetApproverNameForIdAsync(string approverId);

        /// <summary>
        /// Gets the next approver for search criteria.
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        Task<IEnumerable<NextApprover>> QueryNextApproverByKeywordAsync(string searchCriteria);
    }
}