using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Requirement clause indicating the maximum number of courses that can be taken of a given CourseLevel.
    /// "MAXIMUM 5 100,200 LEVEL COURSES"
    /// </summary>
    public class MaxCoursesAtLevels
    {
        /// <summary>
        /// Maximum number of Courses
        /// </summary>
        public int MaxCourses { get; set; }
        /// <summary>
        /// CourseLevels to which maximum applies
        /// </summary>
        public IEnumerable<string> Levels { get; set; }
    }
}
