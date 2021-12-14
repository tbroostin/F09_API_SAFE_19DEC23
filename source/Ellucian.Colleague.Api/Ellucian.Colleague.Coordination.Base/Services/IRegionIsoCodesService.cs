//Copyright 2021 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Countries services
    /// </summary>
    public interface IRegionIsoCodesService : IBaseService
    {
        /// <summary>
        /// Gets all region-iso-codes
        /// </summary>
        /// <param name="criteriaFilter">criteria filter</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="RegionIsoCodes">regionIsoCodes</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.RegionIsoCodes>> GetRegionIsoCodesAsync(Dtos.RegionIsoCodes criteriaFilter, bool bypassCache = false);

        /// <summary>
        /// Get a regionIsoCodes by guid.
        /// </summary>
        /// <param name="guid">Guid of the regionIsoCodes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="RegionIsoCodes">regionIsoCodes</see></returns>
        Task<Ellucian.Colleague.Dtos.RegionIsoCodes> GetRegionIsoCodesByGuidAsync(string guid, bool bypassCache = true);


    }
}