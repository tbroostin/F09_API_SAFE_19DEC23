// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a ProcurementReceiptsRepository
    /// </summary>
    public interface IPurchaseOrderReceiptRepository
    {
        Task<Tuple<IEnumerable<PurchaseOrderReceipt>, int>> GetPurchaseOrderReceiptsAsync(int offset, int limit, string purchaseOrderID="");
        Task<PurchaseOrderReceipt> GetPurchaseOrderReceiptByGuidAsync(string guid);
        Task<PurchaseOrderReceipt> CreatePurchaseOrderReceiptAsync(PurchaseOrderReceipt purchaseOrdersEntity);
        Task<string> GetPurchaseOrderReceiptIdFromGuidAsync(string guid);
    }
}