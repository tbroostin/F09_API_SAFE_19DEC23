//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for FixedAssetTypes services
    /// </summary>
    public interface IFixedAssetTypesService : IBaseService
    {          
        /// <summary>
        /// Gets all fixed-asset-types
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="FixedAssetType">fixedAssetTypes</see> objects</returns>
         Task<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetType>> GetFixedAssetTypesAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a fixed asset type by guid.
        /// </summary>
        /// <param name="guid">Guid of the fixedAssetTypes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="FixedAssetType">fixedAssetTypes</see></returns>
        Task<Ellucian.Colleague.Dtos.FixedAssetType> GetFixedAssetTypesByGuidAsync(string guid, bool bypassCache = true);            
    }
}
