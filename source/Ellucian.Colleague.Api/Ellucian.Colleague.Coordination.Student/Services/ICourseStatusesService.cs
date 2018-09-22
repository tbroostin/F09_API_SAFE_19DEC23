//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for CourseStatuses services
    /// </summary>
    public interface ICourseStatusesService : IBaseService
    {
          
        /// <summary>
        /// Gets all course-statuses
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="CourseStatuses">courseStatuses</see> objects</returns>
         Task<IEnumerable<Ellucian.Colleague.Dtos.CourseStatuses>> GetCourseStatusesAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a courseStatuses by guid.
        /// </summary>
        /// <param name="guid">Guid of the courseStatuses in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="CourseStatuses">courseStatuses</see></returns>
        Task<Ellucian.Colleague.Dtos.CourseStatuses> GetCourseStatusesByGuidAsync(string guid, bool bypassCache = true);

            
    }
}
