// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Course Status services
    /// </summary>
    public interface ICourseStatusService
    {
        IEnumerable<Ellucian.Colleague.Dtos.CourseStatus> GetCourseStatuses();
    }
}
