//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for DefaultSettings services
    /// </summary>
    public interface IDefaultSettingsService : IBaseService
    {
        /// <summary>
        /// Gets all default-settings
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="DefaultSettings">defaultSettings</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettings>> GetDefaultSettingsAsync(List<Dtos.DefaultSettingsEthos> resourcesFilter, bool bypassCache = false);

        /// <summary>
        /// Get a defaultSettings by guid.
        /// </summary>
        /// <param name="guid">Guid of the defaultSettings in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="DefaultSettings">defaultSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.DefaultSettings> GetDefaultSettingsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Gets all default-settings-options
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="DefaultSettings">defaultSettings</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettingsOptions>> GetDefaultSettingsOptionsAsync(List<Dtos.DefaultSettingsEthos> resourcesFilter, bool bypassCache = false);

        /// <summary>
        /// Get a defaultSettings options by guid.
        /// </summary>
        /// <param name="guid">Guid of the defaultSettings in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="DefaultSettings">defaultSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.DefaultSettingsOptions> GetDefaultSettingsOptionsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Gets all default-settings-advanced-search-options
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="DefaultSettingsAdvancedSearchOptions">defaultSettingsAdvancedSearchOptions</see> objects</returns>       
        Task<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettingsAdvancedSearchOptions>> GetDefaultSettingsAdvancedSearchOptionsAsync(Ellucian.Colleague.Dtos.Filters.DefaultSettingsFilter advancedSearchFilter, bool bypassCache = false);
        
        /// <summary>
        /// Update a defaultSettings.
        /// </summary>
        /// <param name="defaultSettings">The <see cref="DefaultSettings">defaultSettings</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="DefaultSettings">defaultSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.DefaultSettings> UpdateDefaultSettingsAsync(Ellucian.Colleague.Dtos.DefaultSettings defaultSettings);
    }
}
