/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface ICollectionConfigurationSettingsRepository : IEthosExtended
    {
        /// <summary>
        /// Get a collection of IntgConfigSettings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgConfigSettings</returns>
        Task<IEnumerable<CollectionConfigurationSettings>> GetCollectionConfigurationSettingsAsync(bool bypassCache);

        Task<CollectionConfigurationSettings> GetCollectionConfigurationSettingsByGuidAsync(string guid, bool bypassCache);

        Task<string> GetCollectionConfigurationSettingsIdFromGuidAsync(string guid);

        Task<CollectionConfigurationSettings> UpdateCollectionConfigurationSettingsAsync(CollectionConfigurationSettings configurationSettings);

        Task<Dictionary<string, string>> GetAllBendedCodesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllRelationTypesCodesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache);
    }
}