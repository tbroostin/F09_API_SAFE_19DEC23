//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for ProcurementReceipts services
    /// </summary>
    public interface IProcurementReceiptsService : IBaseService
    {
        /// <summary>
        /// Gets all procurement-receipts
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="filters">Filters</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="ProcurementReceipts">procurementReceipts</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.ProcurementReceipts>, int>> GetProcurementReceiptsAsync(int offset, int limit, Dtos.ProcurementReceipts filters, bool bypassCache = false);

        /// <summary>
        /// Get a procurementReceipts by guid.
        /// </summary>
        /// <param name="guid">Guid of the procurementReceipts in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="ProcurementReceipts">procurementReceipts</see></returns>
        Task<Ellucian.Colleague.Dtos.ProcurementReceipts> GetProcurementReceiptsByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Create a procurementReceipts.
        /// </summary>
        /// <param name="procurementReceipts">The <see cref="ProcurementReceipts">procurementReceipts</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="ProcurementReceipts">procurementReceipts</see></returns>
        Task<Ellucian.Colleague.Dtos.ProcurementReceipts> CreateProcurementReceiptsAsync(Ellucian.Colleague.Dtos.ProcurementReceipts procurementReceipts);
    }
}