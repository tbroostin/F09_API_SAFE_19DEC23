//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentGradePointAverages services
    /// </summary>
    public interface IStudentGradePointAveragesService : IBaseService
    {
        /// <summary>
        /// Gets student grade point averages.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="criteriaObj"></param>
        /// <param name="gradeDateFilterValue"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Dtos.StudentGradePointAverages>, int>> GetStudentGradePointAveragesAsync(int offset, int limit, StudentGradePointAverages criteriaObj, 
            string gradeDateFilterValue, bool bypassCache);

        /// <summary>
        /// Gets record key for student grade point average based on guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        Task<StudentGradePointAverages> GetStudentGradePointAveragesByGuidAsync(string guid, bool bypassCache = false);
    }
}