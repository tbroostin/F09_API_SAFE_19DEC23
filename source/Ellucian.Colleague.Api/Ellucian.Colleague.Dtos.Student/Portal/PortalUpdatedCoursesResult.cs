// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Contains courses that are applicable for update from Portal.
    /// </summary>
    public class PortalUpdatedCoursesResult
    {
        /// <summary>
        /// Total number of courses applicable for update from Portal.
        /// </summary>
        public Nullable<int> TotalCourses { get; set; }

        /// <summary>
        /// List of courses applicable for update from Portal.
        /// </summary>
        public List<PortalCourse> Courses { get; set; }

        /// <summary>
        /// Portal Update Course Result constructor
        /// </summary>
        public PortalUpdatedCoursesResult()
        {
            Courses = new List<PortalCourse>();
        }
    }
}
