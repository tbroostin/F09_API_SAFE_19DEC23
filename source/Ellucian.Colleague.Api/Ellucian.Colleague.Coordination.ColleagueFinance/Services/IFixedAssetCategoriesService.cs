//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for FixedAssetCategories services
    /// </summary>
    public interface IFixedAssetCategoriesService : IBaseService
    {
          
        /// <summary>
        /// Gets all fixed-asset-categories
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="FixedAssetCategories">fixedAssetCategories</see> objects</returns>
         Task<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetCategory>> GetFixedAssetCategoriesAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a fixedAssetCategories by guid.
        /// </summary>
        /// <param name="guid">Guid of the fixedAssetCategories in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="FixedAssetCategories">fixedAssetCategories</see></returns>
        Task<Ellucian.Colleague.Dtos.FixedAssetCategory> GetFixedAssetCategoriesByGuidAsync(string guid, bool bypassCache = true);
            
    }
}
