//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentAcademicCredentials services
    /// </summary>
    public interface IStudentAcademicCredentialsService : IBaseService
    {
        /// <summary>
        /// Gets all student-academic-credentials
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentAcademicCredentials">studentAcademicCredentials</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicCredentials>, int>> GetStudentAcademicCredentialsAsync(int offset, int limit,
            Dtos.StudentAcademicCredentials criteria = null, Dtos.Filters.PersonFilterFilter2 personFilter = null, Dtos.Filters.AcademicProgramsFilter academicProgramFilter = null,
            Dictionary<string, string> filterQualifiers = null, bool bypassCache = false);

        /// <summary>
        /// Get a studentAcademicCredentials by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentAcademicCredentials in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentAcademicCredentials">studentAcademicCredentials</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentAcademicCredentials> GetStudentAcademicCredentialsByGuidAsync(string guid, bool bypassCache = true);
    }
}