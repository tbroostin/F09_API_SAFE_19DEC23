//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentAcademicPeriodStatuses services
    /// </summary>
    public interface IStudentAcademicPeriodStatusesService : IBaseService
    {
        /// <summary>
        /// Gets all student-academic-period-statuses
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentAcademicPeriodStatuses">studentAcademicPeriodStatuses</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodStatuses>> GetStudentAcademicPeriodStatusesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a studentAcademicPeriodStatuses by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentAcademicPeriodStatuses in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentAcademicPeriodStatuses">studentAcademicPeriodStatuses</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentAcademicPeriodStatuses> GetStudentAcademicPeriodStatusesByGuidAsync(string guid, bool bypassCache = true);

    }
}