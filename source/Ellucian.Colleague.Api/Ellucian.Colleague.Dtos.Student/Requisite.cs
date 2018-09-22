using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Requisites related to courses or sections. For example, when taking BIOL-100
    /// you are required to take BIOL-101L concurrently. 
    /// </summary>
    public class Requisite
    {
        /// <summary>
        /// Optional: RequirementCode the defines what must be met to satisfy the requisite.  If the requisite does 
        /// not contain a requirement code, a CorequisiteCourseId must be supplied
        /// </summary>
        public string RequirementCode { get; set; }

        /// <summary>
        /// Indicates whether the requisite is required or just recommended.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Indicates whether the requisite must be met prior to the course or section, or if it must
        /// be met concurrently with the course or section, or whether either timing is acceptable.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RequisiteCompletionOrder CompletionOrder { get; set; }

        /// <summary>
        /// Optional: This property will ONLY be provided if a requirement code cannot be supplied, and only
        /// if the requisite is for a single concurrent course.
        /// </summary>
        public string CorequisiteCourseId { get; set; }

        /// <summary>
        /// Indicates whether the requisite is protected on a course. If it is, it may not be overridden by at the section level. 
        /// </summary>
        public bool IsProtected { get; set; }
    }
}
