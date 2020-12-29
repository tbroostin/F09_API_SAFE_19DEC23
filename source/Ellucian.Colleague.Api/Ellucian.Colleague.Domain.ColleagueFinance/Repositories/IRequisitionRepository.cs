// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a requisition repository
    /// </summary>
    public interface IRequisitionRepository : IEthosExtended
    {
        /// <summary>
        /// Get a single requisition
        /// </summary>
        /// <param name="requisitionId">The requisition ID to retrieve</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <param name="expenseAccounts">Set of GL Accounts to which the user has access.</param>
        /// <returns>A requisition domain entity</returns>
        Task<Requisition> GetRequisitionAsync(string requisitionId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts);
        Task<Tuple<IEnumerable<Requisition>, int>> GetRequisitionsAsync(int offset, int limit);
        Task<Requisition> GetRequisitionsByGuidAsync(string guid);
        Task<Requisition> UpdateRequisitionAsync(Requisition requisition);
        Task<Requisition> CreateRequisitionAsync(Requisition requisition);
        Task<Requisition> DeleteRequisitionAsync(string guid);
        Task<string> GetRequisitionsIdFromGuidAsync(string id);
        Task<IDictionary<string, string>> GetProjectReferenceIds(string[] projectIds);
        Task<IDictionary<string, string>> GetProjectIdsFromReferenceNo(string[] projectRefNo);
        Task<IEnumerable<RequisitionSummary>> GetRequisitionsSummaryByPersonIdAsync(string personId);
        Task<RequisitionCreateUpdateResponse> CreateRequisitionsAsync(RequisitionCreateUpdateRequest createUpdateRequest);
        Task<RequisitionCreateUpdateResponse> UpdateRequisitionsAsync(RequisitionCreateUpdateRequest createUpdateRequest, Requisition originalRequisition);
        Task<RequisitionDeleteResponse> DeleteRequisitionsAsync(RequisitionDeleteRequest deleteRequest);

    }
}
