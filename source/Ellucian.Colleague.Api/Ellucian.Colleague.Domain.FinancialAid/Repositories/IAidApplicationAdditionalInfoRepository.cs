// Copyright 2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    public interface IAidApplicationAdditionalInfoRepository : IEthosExtended
    {
        /// <summary>
        /// Get aid application additional info by id
        /// </summary>
        /// <param name="id">AidApplicationAdditionalInfo id</param>
        /// <returns>AidApplicationAdditionalInfo</returns>
        Task<AidApplicationAdditionalInfo> GetAidApplicationAdditionalInfoByIdAsync(string id);

        /// <summary>
        /// Get a collection of aid application additional info
        /// </summary>
        /// <returns>Collection of AidApplicationAdditionalInfo</returns>
        Task<Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>> GetAidApplicationAdditionalInfoAsync(int offset, int limit, string appDemoId = "",string personId = "", string aidApplicationType = "", string aidYear = "", string applicantAssignedId = "");
        Task<AidApplicationAdditionalInfo> CreateAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfo aidApplicationAdditionalInfo);

        Task<AidApplicationAdditionalInfo> UpdateAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfo aidApplicationAdditionalInfoEntity);
    }
}
