// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    [Serializable]
    public class PortalUpdatedCoursesResult
    {
        /// <summary>
        /// Total number of courses applicable for update from Portal.
        /// </summary>
        public Nullable<int> TotalCourses { get; private set; }

        /// <summary>
        /// List of courses applicable for update from Portal.
        /// </summary>
        public List<PortalCourse> Courses { get; private set; }

        public PortalUpdatedCoursesResult(Nullable<int> totalCourses, List<PortalCourse> courses)
        {
            TotalCourses = totalCourses;
            Courses = courses;
        }
    }
}
