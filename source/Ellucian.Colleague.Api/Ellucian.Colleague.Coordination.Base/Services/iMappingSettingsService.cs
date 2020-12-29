//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for MappingSettings services
    /// </summary>
    public interface IMappingSettingsService : IBaseService
    {
        /// <summary>
        /// Gets all mapping-settings
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="MappingSettings">mappingSettings</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.MappingSettings>, int>> GetMappingSettingsAsync(int offset, int limit, Dtos.MappingSettings criteriaFilter, bool bypassCache = false);

        /// <summary>
        /// Get a mappingSettings by guid.
        /// </summary>
        /// <param name="guid">Guid of the mappingSettings in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="MappingSettings">mappingSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.MappingSettings> GetMappingSettingsByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Update a mappingSettings.
        /// </summary>
        /// <param name="mappingSettings">The <see cref="MappingSettings">mappingSettings</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="MappingSettings">mappingSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.MappingSettings> UpdateMappingSettingsAsync(Ellucian.Colleague.Dtos.MappingSettings mappingSettings);

        /// <summary>
        /// Get a mappingSettings options by guid.
        /// </summary>
        /// <param name="guid">Guid of the mappingSettings in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="MappingSettings">mappingSettingsOptions</see></returns>
        Task<Ellucian.Colleague.Dtos.MappingSettingsOptions> GetMappingSettingsOptionsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Gets all mapping-settings-options
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="MappingSettingsOptions">mappingSettingsOptions</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.MappingSettingsOptions>, int>> GetMappingSettingsOptionsAsync(int offset, int limit, Dtos.MappingSettingsOptions criteriaFilter, bool bypassCache = false);

    }
}
