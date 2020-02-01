// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Collection of registered student IDs for a course section
    /// </summary>
    public class SectionWaitlist
    {
        /// <summary>
        /// Course section ID
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// IDs for students waitlisted in the course section
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
    }
}
