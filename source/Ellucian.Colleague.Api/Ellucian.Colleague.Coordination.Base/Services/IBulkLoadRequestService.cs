//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for BulkLoadRequest services
    /// </summary>
    public interface IBulkLoadRequestService : IBaseService
    {
        /// <summary>
        /// Create a bulkloadrequest.
        /// </summary>
        /// <param name="bulkLoadRequest">The <see cref="bulkLoadRequest">bulkLoadRequest</see> entity to create in the database.</param>
        /// <param name="permissionCode"></param>
        /// <returns>The newly created <see cref="bulkLoadRequest">bulkLoadRequest</see></returns>
        Task<Ellucian.Colleague.Dtos.BulkLoadRequest> CreateBulkLoadRequestAsync(Ellucian.Colleague.Dtos.BulkLoadRequest bulkLoadRequest, string permissionCode);

        /// <summary>
        /// Get the status for a bulk request
        /// </summary>
        /// <param name="id">id of the bulk request to get the status of</param>
        /// <returns>Bulk request status</returns>
        Task<Dtos.BulkLoadGet> GetBulkLoadRequestStatus(string id);

        /// <summary>
        /// Gets the system status of if bulk loads are supported
        /// </summary>
        /// <returns></returns>
        bool IsBulkLoadSupported();

    }
}