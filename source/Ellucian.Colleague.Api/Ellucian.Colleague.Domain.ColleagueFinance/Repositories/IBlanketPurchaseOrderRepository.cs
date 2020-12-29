// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a blanket purchase order repository
    /// </summary>
    public interface IBlanketPurchaseOrderRepository : IEthosExtended
    {
        /// <summary>
        /// Get a single blanket purchase order
        /// </summary>
        /// <param name="blanketPurchaseOrderId">The blanket purchase order to retrieve</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <param name="expenseAccounts">Set of GL Accounts to which the user has access.</param>
        /// <returns>Blanket purchase order domain entity</returns>
        Task<BlanketPurchaseOrder> GetBlanketPurchaseOrderAsync(string blanketPurchaseOrderId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts);

        /// <summary>
        /// Get the purchase order requested
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <param name="glAccessLevel">access level of current user.</param>
        /// <param name="expenseAccounts">List of expense accounts.</param>
        /// <returns>Tuple of PurchaseOrder entity objects <see cref="BlanketPurchaseOrder"/> and a count for paging.</returns>
        Task<Tuple<IEnumerable<BlanketPurchaseOrder>, int>> GetBlanketPurchaseOrdersAsync(int offset, int limit, string number);

        /// <summary>
        /// Get PurchaseOrder by GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>PurchaseOrder entity object <see cref="BlanketPurchaseOrder"/></returns>
        Task<BlanketPurchaseOrder> GetBlanketPurchaseOrdersByGuidAsync(string guid);

        /// <summary>
        /// Returns a BPO key from a GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Key to Blanket Purchase Order</returns>
        Task<string> GetBlanketPurchaseOrdersIdFromGuidAsync(string guid);

        /// <summary>
        /// Get a list of all project references within a blanket purchase order.
        /// </summary>
        /// <param name="projectIds">Project Keys.</param>
        /// <returns></returns>
        Task<IDictionary<string, string>> GetProjectReferenceIds(string[] projectIds);

        /// <summary>
        /// Gets project ref no's.
        /// </summary>
        /// <param name="projectIds"></param>
        /// <returns></returns>
        Task<IDictionary<string, string>> GetProjectIdsFromReferenceNo(string[] projectRefNo);

        /// <summary>
        /// Get the GUID for a buyer using its ID
        /// </summary>
        /// <param name="id">The Entity ID we are looking for</param>
        /// <param name="entity">Any entity found in Colleague</param>
        /// <returns>Buyer GUID</returns>
        Task<string> GetGuidFromIdAsync(string id, string entity);

        Task<BlanketPurchaseOrder> UpdateBlanketPurchaseOrdersAsync(BlanketPurchaseOrder purchaseOrdersEntity);

        Task<BlanketPurchaseOrder> CreateBlanketPurchaseOrdersAsync(BlanketPurchaseOrder purchaseOrdersEntity);
    }
}
