// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// All the planned courses for a given term on a students degree plan
    /// </summary>
    public class DegreePlanTerm4
    {
        /// <summary>
        /// The term for which courses are planned
        /// </summary>
        public string TermId { get; set; }
        /// <summary>
        /// The list of planned courses <see cref="PlannedCourse"/>
        /// </summary>
        public List<PlannedCourse4> PlannedCourses { get; set; }
        /// <summary>
        /// Extracts the Ids of the courses planned for this term
        /// </summary>
        /// <returns>A list of course Ids</returns>
        public List<string> GetPlannedCourseIds()
        {
            if (PlannedCourses == null) return new List<string>();
            if (PlannedCourses.Count == 0) return new List<string>();
            return PlannedCourses.Select(pc => pc.CourseId).ToList();
        }
    }
}
