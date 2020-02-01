// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Collections.Generic;
using System;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This is the definition of the methods that must be implemented
    /// for a blanket purchase order service.
    /// </summary>
    public interface IBlanketPurchaseOrderService : IBaseService
    {
        /// <summary>
        /// Returns a specified blanket purchase order.
        /// </summary>
        /// <param name="id">ID of the requested blanket purchase order.</param>
        /// <returns>A blanket purchase order DTO.</returns>
        Task<BlanketPurchaseOrder> GetBlanketPurchaseOrderAsync(string id);

        /// <summary>
        /// EEDM Get all purchase orders by paging V15.1.0
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="criteriaObj"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.BlanketPurchaseOrders>, int>> GetBlanketPurchaseOrdersAsync(int offset, int limit, Dtos.BlanketPurchaseOrders criteriaObj, bool bypassCache = false);

        /// <summary>
        /// EEDM Get Purchase order by GUID V15.1.0
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.BlanketPurchaseOrders> GetBlanketPurchaseOrdersByGuidAsync(string id);

        /// <summary>
        /// EEDM Put purchase orders V15.1.0
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="blanketPurchaseOrders"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.BlanketPurchaseOrders> PutBlanketPurchaseOrdersAsync(string guid, Ellucian.Colleague.Dtos.BlanketPurchaseOrders blanketPurchaseOrders);

        /// <summary>
        /// EEDM Post purchase order V15.1.0
        /// </summary>
        /// <param name="blanketPurchaseOrders"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.BlanketPurchaseOrders> PostBlanketPurchaseOrdersAsync(Ellucian.Colleague.Dtos.BlanketPurchaseOrders blanketPurchaseOrders);
    }
}
