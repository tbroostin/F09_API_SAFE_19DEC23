// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// A warning that us associated with a planned course when conflicts are found, such as unsatisfied
    /// requisites or time conflicts between planned course sections
    /// </summary>
    public class PlannedCourseWarning
    {
        /// <summary>
        /// Indicates type of warning, such as corequisite or time conflict
        /// <see cref="DegreePlanWarningType"/>
        /// </summary>        
        public PlannedCourseWarningType Type { get; set; }

        /// <summary>
        /// If a course section is relevant to the warning, the section Id will be included here
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// If the type is an UnmetRequisite, then this is the information about the requisite
        /// </summary>
        public Requisite Requisite { get; set; }

        /// <summary>
        /// If the type is an UnmetRequisite, then this is the information about the section requisite
        /// </summary>
        public SectionRequisite SectionRequisite { get; set; }
    }

}