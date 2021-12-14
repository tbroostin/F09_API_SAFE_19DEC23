// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student.Requirements;
using System;

namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// A placeholder for a course on a course block; this might be used to represent complex non-course requirements for a course block.
    /// </summary>
    public class CoursePlaceholder
    {
        /// <summary>
        /// Unique identifier for the course placeholder
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Title for the course placeholder
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the course placeholder
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional date on which the course placeholder becomes applicable; before this date (if set), the course placeholder is not applicable
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional last date on which the course placeholder is applicable; after this date (if set), the course placeholder is not applicable
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Free-form academic credit information associated with the course placeholder
        /// </summary>
        public string CreditInformation { get; set; }

        /// <summary>
        /// Optional academic requirement information that is associated with the course placeholder
        /// </summary>
        public AcademicRequirementGroup AcademicRequirement { get; set; }
    }
}
