//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for FinancialAidAcademicProgressTypes services
    /// </summary>
    public interface IFinancialAidAcademicProgressTypesService : IBaseService
    {
                /// <summary>
        /// Gets all financial-aid-academic-progress-types
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="FinancialAidAcademicProgressTypes">financialAidAcademicProgressTypes</see> objects</returns>          
         Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressTypes>> GetFinancialAidAcademicProgressTypesAsync(bool bypassCache = false);
             
        /// <summary>
        /// Get a financialAidAcademicProgressTypes by guid.
        /// </summary>
        /// <param name="guid">Guid of the financialAidAcademicProgressTypes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="FinancialAidAcademicProgressTypes">financialAidAcademicProgressTypes</see></returns>
        Task<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressTypes> GetFinancialAidAcademicProgressTypesByGuidAsync(string guid, bool bypassCache = true);            
    }
}
