// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]
    /// <summary>
    /// Represents a section and related information for which a potential student
    /// would  like to enroll.
    /// </summary>
    public  class InstantEnrollmentRegistrationBaseSectionToRegister
    {

        /// <summary>
        /// The identifier of the section of interest
        /// </summary>
        public string SectionId { get; private set; }

        /// <summary>
        /// The number of academic credits for which the student proposes to register
        /// </summary>
        public decimal? AcademicCredits { get ; private set; }

        /// <summary>
        /// The reason for taking this class.
        /// </summary>
        public string RegistrationReason { get; set; }

        /// <summary>
        /// Indicates how the student found out about this section.
        /// </summary>
        public string MarketingSource { get; set; }

        public InstantEnrollmentRegistrationBaseSectionToRegister(string sectionId, decimal? credits) 
        {
            if (String.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Section Id cannot be null on proposed registration");
            }

            SectionId = sectionId;
            AcademicCredits = credits;
        }
    }
}
