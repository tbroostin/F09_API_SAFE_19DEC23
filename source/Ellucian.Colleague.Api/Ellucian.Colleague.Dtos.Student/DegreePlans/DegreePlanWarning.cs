// Copyright 2019 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// A warning that us associated with a planned course when conflicts are found, such as unsatisfied
    /// requisites or time conflicts between planned course sections
    /// </summary>
    public class DegreePlanWarning
    {
        /// <summary>
        /// Indicates type of warning, such as corequisite or time conflict
        /// <see cref="DegreePlanWarningType"/>
        /// </summary>        
        public DegreePlanWarningType Type { get; set; }

        /// <summary>
        /// If a course is relevant to the warning, the course Id will be included here
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// If a course section is relevant to the warning, the section Id will be included here
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// If a term is relevant to the warning, the term code will be included here
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// The ID of the Requirement to display with the warning
        /// </summary>
        public string RequirementCode { get; set; }
    }

}