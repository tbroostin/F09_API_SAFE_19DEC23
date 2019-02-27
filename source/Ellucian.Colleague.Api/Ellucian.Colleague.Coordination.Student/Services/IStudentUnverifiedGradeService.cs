//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentUnverifiedGrades services
    /// </summary>
    public interface IStudentUnverifiedGradesService : IBaseService
    {
        /// <summary>
        /// Gets all student-unverified-grades
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentUnverifiedGrades">studentUnverifiedGrades</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentUnverifiedGrades>, int>> GetStudentUnverifiedGradesAsync(int offset, int limit,
            string student = "", string sectionRegistration = "", string section = "", bool bypassCache = false);

        /// <summary>
        /// Get a studentUnverifiedGrades by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentUnverifiedGrades in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentUnverifiedGrades">studentUnverifiedGrades</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentUnverifiedGrades> GetStudentUnverifiedGradesByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Get a studentUnverifiedGradesSubmissions by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentUnverifiedGradesSubmissions in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentUnverifiedGradesSubmissions">studentUnverifiedGradesSubmissions</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentUnverifiedGradesSubmissions> GetStudentUnverifiedGradesSubmissionsByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Update a studentUnverifiedGradesSubmissions.
        /// </summary>
        /// <param name="studentUnverifiedGradesSubmissions">The <see cref="StudentUnverifiedGradesSubmissions">studentUnverifiedGradesSubmissions</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="StudentUnverifiedGrades">studentUnverifiedGrades</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentUnverifiedGrades> UpdateStudentUnverifiedGradesSubmissionsAsync(Ellucian.Colleague.Dtos.StudentUnverifiedGradesSubmissions studentUnverifiedGradesSubmissions);

        /// <summary>
        /// Create a studentUnverifiedGradesSubmissions.
        /// </summary>
        /// <param name="studentUnverifiedGradesSubmissions">The <see cref="StudentUnverifiedGradesSubmissions">studentUnverifiedGradesSubmissions</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="StudentUnverifiedGrades">studentUnverifiedGrades</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentUnverifiedGrades> CreateStudentUnverifiedGradesSubmissionsAsync(Ellucian.Colleague.Dtos.StudentUnverifiedGradesSubmissions studentUnverifiedGradesSubmissions);

    }
}
