// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Represents a section and related information for which a potential student
    /// would  like to enroll.
    /// </summary>
    public class InstantEnrollmentRegistrationBaseSectionToRegister
    {
        /// <summary>
        /// The identifier of the section of interest
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// The number of academic credits for which the student proposes to register
        /// </summary>
        public decimal? AcademicCredits { get; set; }

        /// <summary>
        /// The reason for taking this class.
        /// </summary>
        public string RegistrationReason { get; set; }

        /// <summary>
        /// Indicates how the student found out about this section.
        /// </summary>
        public string MarketingSource { get; set; }
    }
}
