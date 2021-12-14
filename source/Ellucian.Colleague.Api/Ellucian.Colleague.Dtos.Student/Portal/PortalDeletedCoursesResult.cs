// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Contains the courses that are applicable for deletion from Portal.
    /// </summary>
    public class PortalDeletedCoursesResult 
    {
        /// <summary>
        /// Total number of courses applicable for deletion from Portal.
        /// </summary>
        public Nullable<int> TotalCourses { get; set; }
        /// <summary>
        /// List of Course Ids that are applicable for deletion from Portal
        /// </summary>
        public List<string> CourseIds { get;  set; }
    }
}
