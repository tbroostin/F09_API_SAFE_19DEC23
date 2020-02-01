// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// Course/Term approval or denial
    /// </summary>
    public class DegreePlanApproval2
    {
        /// <summary>
        /// Date and time the course was approved or denied
        /// </summary>
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// ID of the advisor who entered this item
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Status of this item (approved or denied) <see cref="DegreePlanApprovalStatus"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DegreePlanApprovalStatus Status { get; set; }

        /// <summary>
        /// Course Id approved
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Term Code approved for this course. Courses can only be approved for a specific term.
        /// </summary>
        public string TermCode { get; set; }
    }
}
