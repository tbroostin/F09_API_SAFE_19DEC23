//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IGeographicAreasRepository
    {
       
        /// <summary>
        /// Get a collection of GeographicArea
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of GeographicArea</returns>
        Task<Tuple<IEnumerable<GeographicArea>, int>> GetGeographicAreasAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get a single GeographicArea entity
        /// </summary>
        /// <param name="guid">Key to GeographicArea</param>
        /// <returns>GeographicArea Entity</returns>
        Task<GeographicArea> GetGeographicAreaByIdAsync(string guid);
    }
}
