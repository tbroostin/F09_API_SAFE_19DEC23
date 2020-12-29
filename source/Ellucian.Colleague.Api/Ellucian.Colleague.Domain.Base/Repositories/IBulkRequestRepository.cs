// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IBulkRequestRepository
    {
        /// <summary>
        /// Create the Bulk Request
        /// </summary>
        /// <returns>created bulk request</returns>
        Task<BulkRequest> CreateBulkLoadRequestAsync(BulkRequest bulkRequest, IEnumerable<string> dataPrivacyList);

        /// <summary>
        /// Get the bulk request details
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        Task<BulkRequestDetails> GetBulkRequestDetails(string resourceName, string id);

        /// <summary>
        /// Check if bulk load is supported
        /// </summary>
        /// <returns>True if bulk load is supported, false if not</returns>
        bool IsBulkLoadSupported();
    }
}
