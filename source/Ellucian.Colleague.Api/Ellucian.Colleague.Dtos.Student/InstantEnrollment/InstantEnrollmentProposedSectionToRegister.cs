// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Represents a section and related information for which a potential student
    /// would  like to enroll.
    /// </summary>
    public class InstantEnrollmentProposedSectionToRegister
    {

        /// <summary>
        /// The identifier of the section of interest.
        /// </summary>
        public string SectionId { get;  set; }

        /// <summary>
        /// The number of academic credits for which the student proposes to register.
        /// </summary>
        public double? AcademicCredits { get; set; }

    }
}
