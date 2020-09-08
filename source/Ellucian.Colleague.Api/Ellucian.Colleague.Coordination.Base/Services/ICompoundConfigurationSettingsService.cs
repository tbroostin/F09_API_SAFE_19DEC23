//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for CompoundConfigurationSettings services
    /// </summary>
    public interface ICompoundConfigurationSettingsService : IBaseService
    {

        /// <summary>
        /// Gets all compound-configuration-settings
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CompoundConfigurationSettings">compoundConfigurationSettings</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.CompoundConfigurationSettings>> GetCompoundConfigurationSettingsAsync(List<Dtos.DtoProperties.CompoundConfigurationSettingsEthos> resourcesFilter, bool bypassCache = false);

        /// <summary>
        /// Get a compoundConfigurationSettings by guid.
        /// </summary>
        /// <param name="guid">Guid of the compoundConfigurationSettings in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CompoundConfigurationSettings">compoundConfigurationSettings</see></returns>
        Task<Ellucian.Colleague.Dtos.CompoundConfigurationSettings> GetCompoundConfigurationSettingsByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Update a compoundConfigurationSettings.
        /// </summary>
        /// <param name="compoundConfigurationSettings">The <see cref="CompoundConfigurationSettings">compoundConfigurationSettings</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="CompoundConfigurationSettings">compoundConfigurationSettings</see></returns>
       Task<Ellucian.Colleague.Dtos.CompoundConfigurationSettings> UpdateCompoundConfigurationSettingsAsync(Ellucian.Colleague.Dtos.CompoundConfigurationSettings compoundConfigurationSettings);


        /// <summary>
        /// Gets all compound-configuration-settings-options
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CompoundConfigurationSettingsOptions">compoundConfigurationSettingsOptions</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.CompoundConfigurationSettingsOptions>> GetCompoundConfigurationSettingsOptionsAsync(List<Dtos.DtoProperties.CompoundConfigurationSettingsEthos> resourcesFilter, bool bypassCache = false);

        /// <summary>
        /// Get a compoundConfigurationSettingsOptions by guid.
        /// </summary>
        /// <param name="guid">Guid of the compoundConfigurationSettingsOptions in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CompoundConfigurationSettingsOptions">compoundConfigurationSettingsOptions</see></returns>
        Task<Ellucian.Colleague.Dtos.CompoundConfigurationSettingsOptions> GetCompoundConfigurationSettingsOptionsByGuidAsync(string guid, bool bypassCache = true);

    }
}