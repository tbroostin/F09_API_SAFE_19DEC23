// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// The result of adding a course section to student's list of preferred course sections
    /// </summary>
    public class PortalStudentPreferredCourseSectionUpdateResult
    {
        /// <summary>
        /// ID of the course section being added to a student's list of preferred course sections
        /// </summary>
        public string CourseSectionId { get; set; }

        /// <summary>
        /// Status from adding a course section to a student's list of preferred course sections
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PortalStudentPreferredCourseSectionUpdateStatus Status { get; set; }

        /// <summary>
        /// Message associated with adding a course section to a student's list of preferred course sections
        /// </summary>
        public string Message { get; set; }
    }
}
