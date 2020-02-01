//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for ConfigurationSettings services
    /// </summary>
    public interface IConfigurationSettingsService : IBaseService
    {
        /// <summary>
        /// Gets all configuration-settings
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="ConfigurationSettings">configurationSettings</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.ConfigurationSettings>> GetConfigurationSettingsAsync(List<string> resourcesFilter, bool bypassCache = false);

        /// <summary>
        /// Get a configurationSettings by guid.
        /// </summary>
        /// <param name="guid">Guid of the configurationSettings in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="ConfigurationSettings">configurationSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.ConfigurationSettings> GetConfigurationSettingsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Gets all configuration-settings-options
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="ConfigurationSettings">configurationSettings</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.ConfigurationSettingsOptions>> GetConfigurationSettingsOptionsAsync(List<string> resourcesFilter, bool bypassCache = false);

        /// <summary>
        /// Get a configurationSettings options by guid.
        /// </summary>
        /// <param name="guid">Guid of the configurationSettings in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="ConfigurationSettings">configurationSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.ConfigurationSettingsOptions> GetConfigurationSettingsOptionsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Update a configurationSettings.
        /// </summary>
        /// <param name="configurationSettings">The <see cref="ConfigurationSettings">configurationSettings</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="ConfigurationSettings">configurationSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.ConfigurationSettings> UpdateConfigurationSettingsAsync(Ellucian.Colleague.Dtos.ConfigurationSettings configurationSettings);
    }
}
