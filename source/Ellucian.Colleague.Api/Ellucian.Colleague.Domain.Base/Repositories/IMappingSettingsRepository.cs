/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IMappingSettingsRepository : IEthosExtended
    {
        /// <summary>
        /// Get a collection of IntgMappingSettings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgMappingSettings</returns>
        Task<Tuple<IEnumerable<MappingSettings>, int>> GetMappingSettingsAsync(int offset, int limit, List<string> resources, List<string> propertyNames, bool bypassCache);

        Task<MappingSettings> GetMappingSettingsByGuidAsync(string guid, bool bypassCache);

        Task<string> GetMappingSettingsIdFromGuidAsync(string guid);

        Task<MappingSettings> UpdateMappingSettingsAsync(MappingSettings mappingSettings);

        Task<Tuple<IEnumerable<MappingSettingsOptions>, int>> GetMappingSettingsOptionsAsync(int offset, int limit, List<string> resources, List<string> propertyNames, bool bypassCache);

        Task<MappingSettingsOptions> GetMappingSettingsOptionsByGuidAsync(string guid, bool bypassCache);

        Task<string> GetMappingSettingsOptionsIdFromGuidAsync(string guid);


    }
}