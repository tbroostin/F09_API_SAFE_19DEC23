// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]
    /// <summary>
    /// Contains the information common to all Instant Enrollment registration endpoint results.
    /// Each specific endpoint extends this class as needed.
    /// </summary>
    public abstract class InstantEnrollmentRegistrationBaseResult
    {

        /// <summary>
        /// Indicates whether an error occurred during the attempted registration.
        /// </summary>
        public bool ErrorOccurred { get; private set; }

        /// <summary>
        /// The list of successfully registered sections and related information.
        /// </summary>
        public List<InstantEnrollmentRegistrationBaseRegisteredSection> RegisteredSections { get; private set; }
        /// <summary>
        /// List of messages that appeared while doing registration.
        /// </summary>
        public List<InstantEnrollmentRegistrationBaseMessage> RegistrationMessages { get; private set; }

        public InstantEnrollmentRegistrationBaseResult(
            bool errorOccurred,
            IEnumerable<InstantEnrollmentRegistrationBaseRegisteredSection> sections,
            IEnumerable<InstantEnrollmentRegistrationBaseMessage> messages)
        {
            if (sections == null)
            {
                throw new ArgumentNullException("sections", "proposed registered sections cannot be null ");
            }
            if (messages == null)
            {
                throw new ArgumentNullException("messages", "proposed registration messages cannot be null");
            }
            RegisteredSections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>();
            RegistrationMessages = new List<InstantEnrollmentRegistrationBaseMessage>();

            ErrorOccurred = errorOccurred;
            RegisteredSections.AddRange(sections);
            RegistrationMessages.AddRange(messages);
        }

    }
}
