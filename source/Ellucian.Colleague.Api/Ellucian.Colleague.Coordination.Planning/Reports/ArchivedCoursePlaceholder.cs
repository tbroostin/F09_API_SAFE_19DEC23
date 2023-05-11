// Copyright 2021 Ellucian Company L.P. and its affiliates.using System;

namespace Ellucian.Colleague.Coordination.Planning.Reports
{
    public class ArchivedCoursePlaceholder
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
        /// Free-form academic credit information associated with the course placeholder
        /// </summary>
        public string CreditInformation { get; set; }

        /// <summary>
        /// Optional academic requirement code that is associated with the course placeholder
        /// </summary>
        public string AcademicRequirementCode { get; set; }

        /// <summary>
        /// Optional academic requirement subrequirement ID that is associated with the course placeholder
        /// </summary>
        public string AcademicRequirementSubrequirementId { get; set; }

        /// <summary>
        /// Optional academic requirement subrequirement group ID that is associated with the course placeholder
        /// </summary>
        public string AcademicRequirementSubrequirementGroupId { get; set; }

        /// <summary>
        /// Code for the academic term in which the course placeholder is archived
        /// </summary>
        public string TermCode { get; set; }

        public ArchivedCoursePlaceholder()
        {

        }
    }
}
