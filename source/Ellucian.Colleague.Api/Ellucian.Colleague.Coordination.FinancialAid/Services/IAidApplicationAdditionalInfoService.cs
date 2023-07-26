using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface to an AidApplicationAdditionalInfoService
    /// </summary>
    public interface IAidApplicationAdditionalInfoService : IBaseService
    {
        /// <summary>
        /// Get an aid application additional info by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="AidApplicationAdditionalInfo">aidApplicationAdditionalInfo</see></returns>
        Task<AidApplicationAdditionalInfo> GetAidApplicationAdditionalInfoByIdAsync(string id);

        /// <summary>
        /// Gets all aid application additional info
        /// </summary>        
        /// <returns>Collection of <see cref="AidApplicationAdditionalInfo">aidApplicationAdditionalInfo</see> objects</returns>
        Task<Tuple<IEnumerable<AidApplicationAdditionalInfo>, int>> GetAidApplicationAdditionalInfoAsync(int offset, int limit, AidApplicationAdditionalInfo criteriaFilter);

        /// <summary>
        /// Gets all aid application additional info
        /// </summary>        
        /// <returns>Collection of <see cref="AidApplicationAdditionalInfo">aidApplicationAdditionalInfo</see> objects</returns>
        Task<AidApplicationAdditionalInfo> PostAidApplicationAdditionalInfoAsync(AidApplicationAdditionalInfo aidApplicationAdditionalInfoDto);
        Task<AidApplicationAdditionalInfo> PutAidApplicationAdditionalInfoAsync(string id, AidApplicationAdditionalInfo aidApplicationAdditionalInfoDto);
    }
}
