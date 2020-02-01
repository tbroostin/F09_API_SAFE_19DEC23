// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Registered student ID for a course section, with the rating and status date details
    /// </summary>
    public class SectionWaitlistStudent
    {
        /// <summary>
        /// Course section ID
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// ID of the student waitlisted in the course section
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Rank of the waitlisted student
        /// </summary>
        public int? Rank { get; set; }

        /// <summary>
        /// Rating of the waitlisted student
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// Status date of the waitlisted student
        /// </summary>
        public DateTime? StatusDate { get; set; }
        
        /// <summary>
        /// Status code of the waitlisted student
        /// </summary>
        public String StatusCode { get; set; }

    }
}
