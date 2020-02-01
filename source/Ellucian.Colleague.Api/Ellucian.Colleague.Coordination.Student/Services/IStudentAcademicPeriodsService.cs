//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using System;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentAcademicPeriods services
    /// </summary>
    public interface IStudentAcademicPeriodsService : IBaseService
    {

        /// <summary>
        /// Gets all student-academic-periods
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentAcademicPeriods">studentAcademicPeriods</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriods>, int>> GetStudentAcademicPeriodsAsync(int offset, int limit,
            string personFilterValue, Dtos.StudentAcademicPeriods filters, bool bypassCache = false);

        /// <summary>
        /// Get a studentAcademicPeriods by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentAcademicPeriods in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentAcademicPeriods">studentAcademicPeriods</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentAcademicPeriods> GetStudentAcademicPeriodsByGuidAsync(string guid, bool bypassCache = true);
    }
}