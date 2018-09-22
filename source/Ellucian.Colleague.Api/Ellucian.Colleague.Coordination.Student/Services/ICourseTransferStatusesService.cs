//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for CourseTransferStatuses services
    /// </summary>
    public interface ICourseTransferStatusesService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.CourseTransferStatuses>> GetCourseTransferStatusesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.CourseTransferStatuses> GetCourseTransferStatusesByGuidAsync(string id);
    }
}
