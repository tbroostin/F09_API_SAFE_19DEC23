// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This is the definition of the methods that must be implemented
    /// for a blanket purchase order service.
    /// </summary>
    public interface IBlanketPurchaseOrderService
    {
        /// <summary>
        /// Returns a specified blanket purchase order.
        /// </summary>
        /// <param name="id">ID of the requested blanket purchase order.</param>
        /// <returns>A blanket purchase order DTO.</returns>
        Task<BlanketPurchaseOrder> GetBlanketPurchaseOrderAsync(string id);
    }
}
