//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for FinancialAidAcademicProgressStatuses services
    /// </summary>
    public interface IFinancialAidAcademicProgressStatusesService : IBaseService
    {

        /// <summary>
        /// Gets all financial-aid-academic-progress-statuses
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="FinancialAidAcademicProgressStatuses">financialAidAcademicProgressStatuses</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressStatuses>> GetFinancialAidAcademicProgressStatusesAsync(RestrictedVisibility? restrictedVisibility = null, bool bypassCache = false);

        /// <summary>
        /// Get a financialAidAcademicProgressStatuses by guid.
        /// </summary>
        /// <param name="guid">Guid of the financialAidAcademicProgressStatuses in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="FinancialAidAcademicProgressStatuses">financialAidAcademicProgressStatuses</see></returns>
        Task<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressStatuses> GetFinancialAidAcademicProgressStatusesByGuidAsync(string guid, bool bypassCache = true);
    }
}
