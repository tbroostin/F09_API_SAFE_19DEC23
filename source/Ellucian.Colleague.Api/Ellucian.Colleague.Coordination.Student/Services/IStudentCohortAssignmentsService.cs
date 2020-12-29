//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentCohortAssignments services
    /// </summary>
    public interface IStudentCohortAssignmentsService : IBaseService
    {
        /// <summary>
        /// Gets all student-cohort-assignments
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentCohortAssignments">studentCohortAssignments</see> objects</returns>          
        Task<Tuple<IEnumerable<StudentCohortAssignments>, int>> GetStudentCohortAssignmentsAsync(int offset, int limit, StudentCohortAssignments criteria = null,
            Dictionary<string, string> filterQualifiers = null, bool bypassCache = false);

        /// <summary>
        /// Get a studentCohortAssignments by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentCohortAssignments in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentCohortAssignments">studentCohortAssignments</see></returns>
        Task<StudentCohortAssignments> GetStudentCohortAssignmentsByGuidAsync(string guid, bool bypassCache = true);
    }
}
