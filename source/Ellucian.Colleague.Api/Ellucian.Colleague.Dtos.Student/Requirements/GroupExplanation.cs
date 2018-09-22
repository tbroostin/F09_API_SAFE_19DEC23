// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Additional explanations related to a group result created by a program evaluation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GroupExplanation
    {
        /// <summary>
        /// Group was satisfied by applied courses or credits
        /// </summary>
        Satisfied,
        /// <summary>
        /// Group was satisfied by planned courses or credits
        /// </summary>
        PlannedSatisfied,
        /// <summary>
        /// A group that is a "take all of the following courses: course1, course2" type of requirement was not satisfied
        /// </summary>
        Courses,
        /// <summary>
        /// Not enough courses were applied to satisfy this group
        /// </summary>
        MinCourses,
        /// <summary>
        /// Not enough credits were applied to satisfy this group
        /// </summary>
        MinCredits,
        /// <summary>
        /// The credits or courses applied did not cover the group's minimum number of departments requirement
        /// </summary>
        MinDepartments,
        /// <summary>
        /// The credits or courses applied did not cover the group's minimum number of subjects requirement
        /// </summary>
        MinSubjects,
        /// <summary>
        /// Not enough institutional credits were applied to satisfy this group
        /// </summary>
        MinInstCredits,
        /// <summary>
        /// Not enough courses per subject were applied to satisfy this group
        /// </summary>
        MinCoursesPerSubject,
        /// <summary>
        /// Not enough credits per subject were applied to satisfy this group
        /// </summary>
        MinCreditsPerSubject,
        /// <summary>
        /// Not enough courses per department were applied to satisfy this group
        /// </summary>
        MinCoursesPerDepartment,
        /// <summary>
        /// Not enough credits per department were applied to satisfy this group
        /// </summary>
        MinCreditsPerDepartment,
        /// <summary>
        /// The gpa for courses applied to this block overall was too low
        /// </summary>
        MinGpa

    }
}
