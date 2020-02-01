// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.QuickRegistration
{
    /// <summary>
    /// A student's previously selected course section that may be used in the Colleague Self-Service Quick Registration workflow
    /// </summary>
    [Serializable]
    public class QuickRegistrationSection
    {
        /// <summary>
        /// Course section identifier
        /// </summary>
        public string SectionId { get; private set; }

        /// <summary>
        /// Number of credits selected for the course section
        /// </summary>
        public decimal? Credits { get; private set; }

        /// <summary>
        /// Grading type selected for the course section
        /// </summary>
        public GradingType GradingType { get; private set; }

        /// <summary>
        /// Student's waitlist status for the course section
        /// </summary>
        public DegreePlans.WaitlistStatus WaitlistStatus { get; private set; }

        /// <summary>
        /// Creates a new <see cref="QuickRegistrationSection"/>
        /// </summary>
        /// <param name="sectionId">Course section identifier</param>
        /// <param name="credits">Number of credits selected for the course section</param>
        /// <param name="gradingType">Grading type selected for the course section</param>
        /// <param name="waitlistStatus">Student's waitlist status for the course section</param>
        public QuickRegistrationSection(string sectionId, decimal? credits, GradingType gradingType, DegreePlans.WaitlistStatus waitlistStatus)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "A course section ID is required when building a quick registration section.");
            }
            SectionId = sectionId;
            Credits = credits;
            GradingType = gradingType;
            WaitlistStatus = waitlistStatus;
        }
    }
}
