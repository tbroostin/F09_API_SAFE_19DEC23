// Copyright 2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    public interface IAidApplicationResultsRepository : IEthosExtended
    {
        /// <summary>
        /// Get aid application results by id
        /// </summary>
        /// <param name="id">AidApplicationResults id</param>
        /// <returns>AidApplicationResults</returns>
        Task<AidApplicationResults> GetAidApplicationResultsByIdAsync(string id);

        /// <summary>
        /// Get a collection of aid application results
        /// </summary>
        /// <returns>Collection of AidApplicationResults</returns>
        Task<Tuple<IEnumerable<AidApplicationResults>, int>> GetAidApplicationResultsAsync(int offset, int limit, string appDemoId = "", string personId = "", string aidApplicationType = "", string aidYear = "", int? transactionNumber=null,string applicantAssignedId = "");
        Task<AidApplicationResults> UpdateAidApplicationResultsAsync(AidApplicationResults aidApplicationResults);
        Task<AidApplicationResults> CreateAidApplicationResultsAsync(AidApplicationResults aidApplicationResults);
    }
}
