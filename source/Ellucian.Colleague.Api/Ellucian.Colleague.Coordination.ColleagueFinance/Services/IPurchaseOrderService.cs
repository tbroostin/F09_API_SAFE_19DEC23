// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

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
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PurchaseOrders2>, int>> GetPurchaseOrdersAsync2(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// EEDM Get Purchase order by GUID V11
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.PurchaseOrders2> GetPurchaseOrdersByGuidAsync2(string id);

        /// <summary>
        /// EEDM Put purchase orders V11
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="purchaseOrders"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.PurchaseOrders2> PutPurchaseOrdersAsync2(string guid, Ellucian.Colleague.Dtos.PurchaseOrders2 purchaseOrders);

        /// <summary>
        /// EEDM Post purchase order V11
        /// </summary>
        /// <param name="purchaseOrders"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.PurchaseOrders2> PostPurchaseOrdersAsync2(Ellucian.Colleague.Dtos.PurchaseOrders2 purchaseOrders);

        /// <summary>
        /// Returns the list of Purchase Order summary object for the user
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>Purchase Order Summary DTOs</returns>
       Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderSummary>> GetPurchaseOrderSummaryByPersonIdAsync(string personId);

    }
}
