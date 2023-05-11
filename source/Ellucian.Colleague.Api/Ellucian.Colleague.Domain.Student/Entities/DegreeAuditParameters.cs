// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class DegreeAuditParameters
    {
        public bool ModifiedDefaultSort { get; set; }

        /// <summary>
        /// Extra course handling method (D - display, I - Ignore, S - Semi-apply, A - Apply Fully
        /// </summary>
        public ExtraCourses ExtraCourseHandling { get; set; }

        /// <summary>
        /// Include failures
        /// </summary>
        public bool UseLowGrade { get; set; }
        /// <summary>
        /// A flag to indicate if related courses should be shown with applied courses on a group.
        /// If the value is Y then it will display related courses.
        /// </summary>
        public bool ShowRelatedCourses { get; set; }
        /// <summary>
        /// This is to exclude completed courses that are marked as "Possible Replace In Progress" for being replaced 
        /// by another In-Progress or Planned course marked as "Possible Replacement"
        /// </summary>
        public bool ExcludeCompletedPossibleReplaceInProgressCoursesFromGPA { get; set; }
        /// <summary>
        /// This is to apply repeated credits over planned course. It means if a course is repeated such as course is in-progress or completed or registered but the same course is also  planned
        /// then completed/in-progress/registered course will take precedence over repeated planned course
        /// </summary>
        public bool ApplyRepeatedCreditsOverPlannedCourse { get; set; }
        /// <summary>
        /// This is to disable lookahead optmization in MyProgress. 
        /// </summary>
        public bool DisableLookAheadOptimization { get; set; }
        /// <summary>
        /// Constructor for DegreeAuditParameters
        /// </summary>
        /// <param name="extraCourses">What method to use for extra courses</param>
        /// <param name="useLowGrade">Should low grades be used</param>
        /// <param name="modifiedDefaultSort">Has default sort specification been modified</param>
        public DegreeAuditParameters(ExtraCourses extraCourses, bool showRelatedCourses=false,  bool useLowGrade = false, bool modifiedDefaultSort = false, bool excludeCoursesInGPA = false, bool applyRepeatedCreditsOverPlannedCourse=false, bool disableLookaheadOptimization = false)
        {
            ExtraCourseHandling = extraCourses;
            UseLowGrade = useLowGrade;
            ModifiedDefaultSort = modifiedDefaultSort;
            ShowRelatedCourses = showRelatedCourses;
            ExcludeCompletedPossibleReplaceInProgressCoursesFromGPA = excludeCoursesInGPA;
            ApplyRepeatedCreditsOverPlannedCourse = applyRepeatedCreditsOverPlannedCourse;
            DisableLookAheadOptimization = disableLookaheadOptimization;
        }
    }
}
