// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a purchase order repository
    /// </summary>
    public interface IPurchaseOrderRepository : IEthosExtended
    {
        /// <summary>
        /// Get a single purchase order
        /// </summary>
        /// <param name="purchaseOrderId">The purchase order to retrieve</param>
        /// <param name="personId">The user ID</param>
        /// <param name="glAccessLevel">The user GL account security level</param>
        /// <param name="expenseAccounts">Set of GL Accounts to which the user has access.</param>
        /// <returns>Purchase Order domain entity</returns>
        Task<PurchaseOrder> GetPurchaseOrderAsync(string purchaseOrderId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts);


        Task<Tuple<IEnumerable<PurchaseOrder>, int>> GetPurchaseOrdersAsync(int offset, int limit, string orderNumber);

        Task<PurchaseOrder> GetPurchaseOrdersByGuidAsync(string guid);
        Task<PurchaseOrder> UpdatePurchaseOrdersAsync(PurchaseOrder purchaseOrdersEntity);
        Task<PurchaseOrder> CreatePurchaseOrdersAsync(PurchaseOrder purchaseOrdersEntity);
        Task<string> GetPurchaseOrdersIdFromGuidAsync(string guid);

        Task<string> GetGuidFromIdAsync(string id, string entity);

        Task<IDictionary<string, string>> GetProjectReferenceIds(string[] projectIds);
        Task<IDictionary<string, string>> GetProjectIdsFromReferenceNo(string[] projectRefNo);
        Task<IEnumerable<PurchaseOrderSummary>> GetPurchaseOrderSummaryByPersonIdAsync(string personId);

        Task<PurchaseOrderCreateUpdateResponse> CreatePurchaseOrderAsync(PurchaseOrderCreateUpdateRequest createUpdateRequest);

        /// <summary>
        /// Update a Purchase Order
        /// </summary>
        /// <param name="voidRequest"></param>
        /// <returns></returns>
        Task<PurchaseOrderCreateUpdateResponse> UpdatePurchaseOrderAsync(PurchaseOrderCreateUpdateRequest createUpdateRequest, PurchaseOrder originalPurchaseOrder);

        /// <summary>
        /// Void a Purchase Order
        /// </summary>
        /// <param name="voidRequest"></param>
        /// <returns></returns>
        Task<PurchaseOrderVoidResponse> VoidPurchaseOrderAsync(PurchaseOrderVoidRequest voidRequest);

    }
}
