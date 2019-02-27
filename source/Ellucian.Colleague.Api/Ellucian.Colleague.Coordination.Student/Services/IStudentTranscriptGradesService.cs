//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentTranscriptGrades services
    /// </summary>
    public interface IStudentTranscriptGradesService : IBaseService
    {
        /// <summary>
        /// Gets all student-transcript-grades
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="criteriaFilter">criteria filter</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentTranscriptGrades">studentTranscriptGrades</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentTranscriptGrades>, int>> GetStudentTranscriptGradesAsync(int offset, int limit, Dtos.StudentTranscriptGrades criteriaFilter, bool bypassCache = false);

        /// <summary>
        /// Get a studentTranscriptGrades by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentTranscriptGrades in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentTranscriptGrades">studentTranscriptGrades</see></returns>
        Task<Dtos.StudentTranscriptGrades> GetStudentTranscriptGradesByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Get a studentTranscriptGradesAdjustments by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentTranscriptGrades in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentTranscriptGrades">studentTranscriptGradesAdjustments</see></returns>
        Task<Dtos.StudentTranscriptGradesAdjustments> GetStudentTranscriptGradesAdjustmentsByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Update a studentTranscriptGradesAdjustments.
        /// </summary>
        /// <param name="studentTranscriptGradesAdjustments">The <see cref="StudentTranscriptGradesAdjustments">studentTranscriptGradesAdjustments</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="StudentTranscriptGradesAdjustments">studentTranscriptGradesAdjustments</see></returns>
        Task<Dtos.StudentTranscriptGrades> UpdateStudentTranscriptGradesAdjustmentsAsync(Dtos.StudentTranscriptGradesAdjustments studentTranscriptGradesAdjustments, bool bypassCache = true);
    }
}