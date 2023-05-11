// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// All information related to an transcript course import
    /// </summary>
    public class TranscriptCourse
    {
        /// <summary>
        /// ERP prospect ID (ID in PERSON)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")] 
        public string ErpProspectId { get; set; }

        /// <summary>
        /// ERP institutions ID
        /// </summary>
        public string ErpInstitutionId { get; set; }

        /// <summary>
        /// Course title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Course ID
        /// </summary>
        public string Course { get; set; }

        /// <summary>
        /// Academic term
        /// </summary>
        public string Term { get; set; }
        
        /// <summary>
        /// Start Date
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// End Date
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Grade
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// Interim grade
        /// </summary>
        public string InterimGradeFlag { get; set; }

        /// <summary>
        /// Number of credits
        /// </summary>
        public string Credits { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Created on
        /// </summary>
        public string CreatedOn { get; set; }

        /// <summary>
        /// Source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Custom fields
        /// </summary>
        public IEnumerable<CustomField> CustomFields { get; set; }

        /// <summary>
        /// CRM organization name
        /// </summary>
        public string RecruiterOrganizationName { get; set; }

        /// <summary>
        /// CRM organization GUID
        /// </summary>
        public string RecruiterOrganizationId { get; set; }
    }
}
