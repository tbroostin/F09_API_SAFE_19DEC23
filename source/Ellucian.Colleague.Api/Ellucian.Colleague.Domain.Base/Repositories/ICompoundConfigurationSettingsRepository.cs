using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface ICompoundConfigurationSettingsRepository : IEthosExtended
    {

        /// <summary>
        /// Get a collection of Compound Configuration Settings
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of CompoundConfigurationSettings</returns>
        Task<IEnumerable<CompoundConfigurationSettings>> GetCompoundConfigurationSettingsAsync(bool bypassCache);

        /// <summary>
        /// Get a single Compound Configuration Settings domain entity from an compound configuration settings guid.
        /// </summary>
        /// <param name="guid">Guid for Lookup</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns></returns>
        Task<CompoundConfigurationSettings> GetCompoundConfigurationSettingsByGuidAsync(string guid, bool bypassCache = false);


        /// <summary>
        /// Update Compound Configuration Settings 
        /// </summary>
        /// <param name="compoundSettingss">Compound Configuration Settings to update</param>     
        /// <returns>Updated Compound Configuration Settings</returns>
        Task<CompoundConfigurationSettings> UpdateCompoundConfigurationSettingsAsync(CompoundConfigurationSettings compoundSettingss);

        /// <summary>
        /// Get the record key from a compound configuration settings GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        Task<string> GetCompoundConfigurationSettingsIdFromGuidAsync(string guid);
        
        /// <summary>
        /// Get a collection of Compount Configuration Settings Options
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of Compound Configuration Settings Options</returns>
        Task<IEnumerable<CompoundConfigurationSettingsOptions>> GetCompoundConfigurationSettingsOptionsAsync(bool bypassCache);

        /// <summary>
        /// Get a single Compound Configuration Settings Options domain entity from a compound configuration settings options guid.
        /// </summary>
        /// <param name="guid">Guid for Lookup</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns></returns>
        Task<CompoundConfigurationSettingsOptions> GetCompoundConfigurationSettingsOptionsByGuidAsync(string guid, bool bypassCache);

        /// <summary>
        /// Get the record key from a compount configuration settings options GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        Task<string> GetCompoundConfigurationSettingsOptionsIdFromGuidAsync(string guid);
    }
}
