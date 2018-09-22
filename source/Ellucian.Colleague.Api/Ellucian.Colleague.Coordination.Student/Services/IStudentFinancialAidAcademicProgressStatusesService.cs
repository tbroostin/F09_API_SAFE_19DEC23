//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentFinancialAidAcademicProgressStatuses services
    /// </summary>
    public interface IStudentFinancialAidAcademicProgressStatusesService : IBaseService
    {
        /// <summary>
        /// Gets all student-financial-aid-academic-progress-statuses
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentFinancialAidAcademicProgressStatuses">studentFinancialAidAcademicProgressStatuses</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidAcademicProgressStatuses>, int>> GetStudentFinancialAidAcademicProgressStatusesAsync(int offset, int limit, Dtos.StudentFinancialAidAcademicProgressStatuses criteria = null, bool bypassCache = false);

        /// <summary>
        /// Get a studentFinancialAidAcademicProgressStatuses by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentFinancialAidAcademicProgressStatuses in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentFinancialAidAcademicProgressStatuses">studentFinancialAidAcademicProgressStatuses</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentFinancialAidAcademicProgressStatuses> GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(string guid, bool bypassCache = true);
    }
}
