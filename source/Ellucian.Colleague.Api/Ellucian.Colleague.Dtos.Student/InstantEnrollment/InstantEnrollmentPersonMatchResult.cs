// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Result of a person biographic/demographic matching inquiry for Instant Enrollment
    /// </summary>
    public class InstantEnrollmentPersonMatchResult
    {
        /// <summary>
        /// ID of an existing person who matches the submitted biographic and demographic information
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Flag indicating whether or not the submitted biographic and demographic information potentially matches one or more existing persons
        /// </summary>
        public bool HasPotentialMatches { get; set; }

        /// <summary>
        /// Flag indicating that the government ID provided in the person biographic/demographic matching inquiry for Instant Enrollment belongs to an existing user who was not identified as a potential or definite match
        /// </summary>
        public bool DuplicateGovernmentIdFound { get; set; }
    }
}
