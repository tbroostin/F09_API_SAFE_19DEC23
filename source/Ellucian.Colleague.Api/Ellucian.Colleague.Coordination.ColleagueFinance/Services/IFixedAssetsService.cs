//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for FixedAssets services
    /// </summary>
    public interface IFixedAssetsService : IBaseService
    {
        /// <summary>
        /// Gets all fixed-assets
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="FixedAssets">fixedAssets</see> objects</returns>          
        Task<Tuple<IEnumerable<Dtos.FixedAssets>, int>> GetFixedAssetsAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get a fixedAssets by guid.
        /// </summary>
        /// <param name="guid">Guid of the fixedAssets in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="FixedAssets">fixedAssets</see></returns>
        Task<Dtos.FixedAssets> GetFixedAssetsByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Get Fixed Assets transfer flags
        /// </summary>
        /// <returns>Collection of <see cref="FxaTransferFlag">FixedAssetsFlag</see> objects</returns>
        Task<IEnumerable<Dtos.ColleagueFinance.FixedAssetsFlag>> GetFixedAssetTransferFlagsAsync();
    }
}
