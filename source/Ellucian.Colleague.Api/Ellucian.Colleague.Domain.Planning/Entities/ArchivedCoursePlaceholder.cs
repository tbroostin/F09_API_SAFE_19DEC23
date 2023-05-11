// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using System;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    /// <summary>
    /// An archived course placeholder
    /// </summary>
    /// <remarks>A course placeholder might be used to represent complex non-course requirements for a course block</remarks>
    [Serializable]
    public class ArchivedCoursePlaceholder
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
        /// Code for the academic term in which the course placeholder is archived
        /// </summary>
        public string TermCode { get; private set; }

        /// <summary>
        /// ID of the user who added the course placeholder to the degree plan
        /// </summary>
        public string AddedBy { get; set; }

        /// <summary>
        /// Date/time course placeholder was added to the plan
        /// </summary>
        public DateTimeOffset? AddedOn { get; set; }

        /// <summary>
        /// Creates a new <see cref="ArchivedCoursePlaceholder"/> object
        /// </summary>
        /// <param name="id">Unique identifier for the course placeholder</param>
        /// <param name="title">Title for the course placeholder</param>
        /// <param name="description">Description of the course placeholder</param>
        /// <param name="credits">Free-form academic credit information associated with the course placeholder</param>
        /// <param name="academicRequirementGroup">Optional academic requirement information that is associated with the course placeholder</param>
        /// <param name="termCode">Code for the academic term in which the course placeholder is archived</param>
        public ArchivedCoursePlaceholder(string id, string title, string credits, AcademicRequirementGroup academicRequirementGroup, string termCode)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "An archived course placeholder must have a unique identifier.");
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title", "An archived course placeholder must have a title.");
            }
            Id = id;
            Title = title;
            CreditInformation = credits;
            AcademicRequirement = academicRequirementGroup;
            TermCode = termCode;
        }
    }
}
