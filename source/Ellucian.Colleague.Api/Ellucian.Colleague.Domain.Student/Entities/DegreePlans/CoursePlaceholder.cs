// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.DegreePlans
{
    /// <summary>
    /// A placeholder for a course on a course block
    /// </summary>
    /// <remarks>A course placeholder might be used to represent complex non-course requirements for a course block</remarks>
    [Serializable]
    public class CoursePlaceholder
    {
        /// <summary>
        /// Unique identifier for the course placeholder
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Title for the course placeholder
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Description of the course placeholder
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Optional date on which the course placeholder becomes applicable; before this date (if set), the course placeholder is not applicable
        /// </summary>
        public DateTime? StartDate { get; private set; }

        /// <summary>
        /// Optional last date on which the course placeholder is applicable; after this date (if set), the course placeholder is not applicable
        /// </summary>
        public DateTime? EndDate { get; private set; }

        /// <summary>
        /// Free-form academic credit information associated with the course placeholder
        /// </summary>
        public string CreditInformation { get; private set; }

        /// <summary>
        /// Optional academic requirement information that is associated with the course placeholder
        /// </summary>
        /// <remarks>Example: If a course placeholder specified that a student must complete an entry-level course OR test out of that course with an equivalent test score
        /// then the course placeholder could be linked with an academic requirement that indicates this information</remarks>
        public AcademicRequirementGroup AcademicRequirement { get; private set; }

        /// <summary>
        /// Creates a new <see cref="CoursePlaceholder"/> object
        /// </summary>
        /// <param name="id">Unique identifier for the course placeholder</param>
        /// <param name="title">Title for the course placeholder</param>
        /// <param name="description">Description of the course placeholder</param>
        /// <param name="startDate">Optional date on which the course placeholder becomes applicable; before this date (if set), the course placeholder is not applicable</param>
        /// <param name="endDate">Optional last date on which the course placeholder is applicable; after this date (if set), the course placeholder is not applicable</param>
        /// <param name="credits">Free-form academic credit information associated with the course placeholder</param>
        /// <param name="academicRequirementGroup">Optional academic requirement information that is associated with the course placeholder</param>
        public CoursePlaceholder(string id, string title, string description, DateTime? startDate, DateTime? endDate, string credits, AcademicRequirementGroup academicRequirementGroup)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A course placeholder must have a unique identifier.");
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title", "A course placeholder must have a title.");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description", "A course placeholder must have a description.");
            }
            if(startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                throw new ArgumentOutOfRangeException("startDate", "A course placeholder's start date may not be later than its end date.");
            }
            Id = id;
            Title = title;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            CreditInformation = credits;
            AcademicRequirement = academicRequirementGroup;
        }
    }
}
