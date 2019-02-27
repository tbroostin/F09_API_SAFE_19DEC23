// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentProgramService
    {
        /// <summary>
        /// Retrieves <see cref="Dtos.Student.StudentProgram2">student programs</see> for the specified student IDs
        /// </summary>
        /// <param name="studentIds">List of student IDs</param>
        /// <param name="includeInactivePrograms">Flag indicating whether or not to include inactive programs</param>
        /// <param name="term">Optional code filtering student programs for a specific academic term</param>
        /// <param name="includeHistory">Flag indicating whether or not to include historical data</param>
        /// <returns>List of <see cref="Dtos.Student.StudentProgram2">student programs</see></returns>
        Task<IEnumerable<Dtos.Student.StudentProgram2>> GetStudentProgramsByIdsAsync(IEnumerable<string> studentIds, bool includeInactivePrograms = false, string term = null, bool includeHistory = false);
    }
}
