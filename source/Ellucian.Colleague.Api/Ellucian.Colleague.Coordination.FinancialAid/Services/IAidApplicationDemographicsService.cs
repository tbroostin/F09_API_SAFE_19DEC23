using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Interface to an AidApplicationDemographicsService
    /// </summary>
    public interface IAidApplicationDemographicsService : IBaseService
    {
        /// <summary>
        /// Get a aid application demographics by guid.
        /// </summary>
        /// <param name="guid">Guid of the aid application demographics in Colleague.</param>
        /// <returns>The <see cref="AidApplicationDemographics">aidApplicationDemographics</see></returns>
        Task<AidApplicationDemographics> GetAidApplicationDemographicsByIdAsync(string id);

        /// <summary>
        /// Gets all aid application demographics matching the criteria
        /// </summary>
        /// <returns>Collection of <see cref="AidApplicationDemographics">aid application demographics</see> objects</returns>
        Task<Tuple<IEnumerable<AidApplicationDemographics>, int>> GetAidApplicationDemographicsAsync(int offset, int limit, AidApplicationDemographics criteriaFilter);

        Task<AidApplicationDemographics> PostAidApplicationDemographicsAsync(AidApplicationDemographics aidApplicationDemographicsDto);

        Task<AidApplicationDemographics> PutAidApplicationDemographicsAsync(string id, AidApplicationDemographics aidApplicationDemographicsDto);
    }
}
