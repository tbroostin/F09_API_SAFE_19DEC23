// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    [Serializable]

    /// <summary>
    /// Contains the courses that  are applicable for deletion from portal.
    /// </summary>
    public class PortalDeletedCoursesResult 
    {
        /// <summary>
        /// Total number of courses applicable for deletion from Portal.
        /// </summary>
        public Nullable<int> TotalCourses { get; private set; }
        /// <summary>
        /// List of Course Ids that are applicable for deletion from Portal.
        /// </summary>
        public List<string> CourseIds { get; private set; }

        public PortalDeletedCoursesResult(Nullable<int> totalCourses, List<string> courseIds )
        {
            TotalCourses = totalCourses;
            CourseIds = courseIds;
        }
       
    }
}
