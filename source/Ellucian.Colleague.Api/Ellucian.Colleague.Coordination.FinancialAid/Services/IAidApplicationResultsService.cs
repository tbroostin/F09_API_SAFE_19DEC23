// Copyright 2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface to an AidApplicationResultsService
    /// </summary>
    public interface IAidApplicationResultsService : IBaseService
    {
        /// <summary>
        /// Get an aid application results by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="AidApplicationResults">aidApplicationResults</see></returns>
        Task<AidApplicationResults> GetAidApplicationResultsByIdAsync(string id);

        /// <summary>
        /// Gets all aid application results
        /// </summary>        
        /// <returns>Collection of <see cref="AidApplicationResults">aidApplicationResults</see> objects</returns>
        Task<Tuple<IEnumerable<AidApplicationResults>, int>> GetAidApplicationResultsAsync(int offset, int limit, AidApplicationResults criteriaFilter);
        Task<AidApplicationResults> PostAidApplicationResultsAsync(AidApplicationResults aidApplicationResults);
        Task<AidApplicationResults> PutAidApplicationResultsAsync(string id, AidApplicationResults mergedResults);
    }
}
