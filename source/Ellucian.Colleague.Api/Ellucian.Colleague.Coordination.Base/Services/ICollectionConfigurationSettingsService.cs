//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for CollectionConfigurationSettings services
    /// </summary>
    public interface ICollectionConfigurationSettingsService : IBaseService
    {
        /// <summary>
        /// Gets all configuration-settings
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CollectionConfigurationSettings">configurationSettings</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.CollectionConfigurationSettings>> GetCollectionConfigurationSettingsAsync(List<Dtos.DefaultSettingsEthos> resourcesFilter, bool bypassCache = false);

        /// <summary>
        /// Get a configurationSettings by guid.
        /// </summary>
        /// <param name="guid">Guid of the configurationSettings in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CollectionConfigurationSettings">configurationSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.CollectionConfigurationSettings> GetCollectionConfigurationSettingsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Gets all configuration-settings-options
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CollectionConfigurationSettings">configurationSettings</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.CollectionConfigurationSettingsOptions>> GetCollectionConfigurationSettingsOptionsAsync(List<Dtos.DefaultSettingsEthos> resourcesFilter, bool bypassCache = false);

        /// <summary>
        /// Get a configurationSettings options by guid.
        /// </summary>
        /// <param name="guid">Guid of the configurationSettings in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CollectionConfigurationSettings">configurationSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.CollectionConfigurationSettingsOptions> GetCollectionConfigurationSettingsOptionsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Update a configurationSettings.
        /// </summary>
        /// <param name="configurationSettings">The <see cref="CollectionConfigurationSettings">configurationSettings</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="CollectionConfigurationSettings">configurationSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.CollectionConfigurationSettings> UpdateCollectionConfigurationSettingsAsync(Ellucian.Colleague.Dtos.CollectionConfigurationSettings configurationSettings);
    }
}
