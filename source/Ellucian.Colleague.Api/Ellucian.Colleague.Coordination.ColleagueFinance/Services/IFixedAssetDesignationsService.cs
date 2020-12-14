//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for FixedAssetDesignations services
    /// </summary>
    public interface IFixedAssetDesignationsService : IBaseService
    {

        /// <summary>
        /// Gets all fixed-asset-designations
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="FixedAssetDesignations">fixedAssetDesignations</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetDesignations>> GetFixedAssetDesignationsAsync(bool bypassCache = false);

        /// <summary>
        /// Get a fixedAssetDesignations by guid.
        /// </summary>
        /// <param name="guid">Guid of the fixedAssetDesignations in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="FixedAssetDesignations">fixedAssetDesignations</see></returns>
        Task<Ellucian.Colleague.Dtos.FixedAssetDesignations> GetFixedAssetDesignationsByGuidAsync(string guid, bool bypassCache = true);


    }
}
