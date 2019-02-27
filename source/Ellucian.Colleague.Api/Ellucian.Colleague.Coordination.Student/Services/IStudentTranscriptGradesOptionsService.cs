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
    public interface IStudentTranscriptGradesOptionsService : IBaseService
    {
        /// <summary>
        /// Gets all student-transcript-grades-options
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentTranscriptGrades">studentTranscriptGrades</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentTranscriptGradesOptions>, int>> GetStudentTranscriptGradesOptionsAsync(int offset, int limit, Dtos.Filters.StudentFilter studentFilter, bool bypassCache = false);

        /// <summary>
        /// Get a studentTranscriptGradesOptions by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentTranscriptGrades in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentTranscriptGrades">studentTranscriptGradesOptions</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentTranscriptGradesOptions> GetStudentTranscriptGradesOptionsByGuidAsync(string guid, bool bypassCache = true);

    }
}