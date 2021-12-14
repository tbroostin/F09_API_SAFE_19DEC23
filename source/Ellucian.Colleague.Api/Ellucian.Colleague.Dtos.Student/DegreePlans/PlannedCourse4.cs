// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// A course that has been planned. Planned courses are grouped together by term on a degree plan.
    /// </summary>
    public class PlannedCourse4
    {
        /// <summary>
        /// Id of the course that is being planned. Required when course placeholder is not present.
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Id of the section selected.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Term Id for which the course is planned.  If blank this is a "non-term" planned course/section.
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// Number of credits planned.  If there is only a courseId, then this is the number of credits planned for
        /// the course. If there is both a course and a section, then this is effectively the number of credits planned for 
        /// the section. If empty, then the default credits for the course or section is used. Will be present only where
        /// variable credits are allowed.
        /// </summary>
        public decimal? Credits { get; set; }

        /// <summary>
        /// List of validation messages associated with this course/section. May include <see cref="PlannedCourseWarning2">PlannedCourseWarning2</see> messages for corequisites
        /// unmet or for time conflicts between scheduled sections, for example. Warnings will be present only when the degree plan is requested with the Validate option set to 
        /// true (the default setting).
        /// </summary>
        public List<PlannedCourseWarning2> Warnings { get; set; }

        /// <summary>
        /// Grading Type - only applies to the section. Defaults to Graded if there is no section.
        /// <see cref="GradingType"/>
        /// </summary>
        public GradingType GradingType { get; set; }

        /// <summary>
        /// Indicates whether the student is actively on a waitlist for the associated section 
        /// <see cref="WaitlistStatus"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public WaitlistStatus SectionWaitlistStatus { get; set; }

        /// <summary>
        /// System ID for the user who added this course to the plan
        /// </summary>
        public string AddedBy { get; set; }

        /// <summary>
        /// Timestamp when the course was added to the plan.
        /// </summary>
        public DateTimeOffset? AddedOn { get; set; }

        /// <summary>
        /// Boolean which shows whether the planned course is protected. If protected it can only be removed or moved by an authorized advisor or staff person.
        /// </summary>
        public bool IsProtected { get; set; }

        /// <summary>
        /// Id of the course placeholder that is being planned. Required when course is not present.
        /// </summary>
        public string CoursePlaceholderId { get; set; }
    }
}

