// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Collections.Generic;
using System;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for Purchase Orders
    /// </summary>
    public interface IPurchaseOrderService : IBaseService
    {
        /// <summary>
        /// Returns the purchase order selected by the user
        /// </summary>
        /// <param name="purchaseOrderId">The requested purchase order ID</param>
        /// <returns>Purchase Order DTO</returns>
        Task<PurchaseOrder> GetPurchaseOrderAsync(string purchaseOrderId);
        
        /// <summary>
        /// EEDM Get all purchase orders by paging V11
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PurchaseOrders2>, int>> GetPurchaseOrdersAsync(int offset, int limit, Dtos.PurchaseOrders2 criteriaObject, bool bypassCache = false);

        /// <summary>
        /// EEDM Get Purchase order by GUID V11
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.PurchaseOrders2> GetPurchaseOrdersByGuidAsync(string id);

        /// <summary>
        /// EEDM Put purchase orders V11
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="purchaseOrders"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.PurchaseOrders2> PutPurchaseOrdersAsync(string guid, Ellucian.Colleague.Dtos.PurchaseOrders2 purchaseOrders);

        /// <summary>
        /// EEDM Post purchase order V11
        /// </summary>
        /// <param name="purchaseOrders"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.PurchaseOrders2> PostPurchaseOrdersAsync(Ellucian.Colleague.Dtos.PurchaseOrders2 purchaseOrders);

        /// <summary>
        /// Returns the list of Purchase Order summary object for the user
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>Purchase Order Summary DTOs</returns>
       Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderSummary>> GetPurchaseOrderSummaryByPersonIdAsync(string personId);
        
        /// <summary>
        /// Create/Update a purchase order.
        /// </summary>
        /// <param name="purchaseOrderCreateUpdateRequest">The purchase order create update request DTO.</param>        
        /// <returns>The purchase order create update response DTO.</returns>
        Task<Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse> CreateUpdatePurchaseOrderAsync(Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest purchaseOrderCreateUpdateRequest);

        /// <summary>
        /// Void a purchase order.
        /// </summary>
        /// <param name="purchaseOrderVoidRequest">The purchase order void request DTO.</param>        
        /// <returns>The purchase order void response DTO.</returns>
        Task<Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidResponse> VoidPurchaseOrderAsync(Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest purchaseOrderVoidRequest);

    }
}
