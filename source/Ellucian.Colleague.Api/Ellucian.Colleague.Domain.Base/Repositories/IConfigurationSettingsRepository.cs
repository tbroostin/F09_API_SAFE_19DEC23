/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IConfigurationSettingsRepository : IEthosExtended
    {
        /// <summary>
        /// Get a collection of IntgConfigSettings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgConfigSettings</returns>
        Task<IEnumerable<ConfigurationSettings>> GetConfigurationSettingsAsync(bool bypassCache);

        Task<ConfigurationSettings> GetConfigurationSettingsByGuidAsync(string guid, bool bypassCache);

        Task<string> GetConfigurationSettingsIdFromGuidAsync(string guid);

        Task<ConfigurationSettings> UpdateConfigurationSettingsAsync(ConfigurationSettings configurationSettings);

        Task<Dictionary<string, string>> GetAllDuplicateCriteriaAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllCashierNamesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllRegUsersAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache);
    }
}