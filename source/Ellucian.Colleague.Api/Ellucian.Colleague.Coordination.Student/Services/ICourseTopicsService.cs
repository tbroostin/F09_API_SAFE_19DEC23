//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for CourseTopics services
    /// </summary>
    public interface ICourseTopicsService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.CourseTopics>> GetCourseTopicsAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.CourseTopics> GetCourseTopicsByGuidAsync(string id);
    }
}
