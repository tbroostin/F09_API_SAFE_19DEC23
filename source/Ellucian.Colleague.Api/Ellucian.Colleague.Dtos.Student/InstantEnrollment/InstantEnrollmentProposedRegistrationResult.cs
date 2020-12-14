// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains the results of a trial registration of given classes for a given individual.
    /// </summary>
    public class InstantEnrollmentProposedRegistrationResult
    {
        /// <summary>
        /// This identifies if any error occurred while working on mock registration.
        /// Those errors will be listed in RegistrationMessages.
        /// </summary>
        public bool ErrorOccurred { get; set; }
        /// <summary>
        /// The list of successfully registered sections and related information.
        /// </summary>
        public List<InstantEnrollmentRegistrationBaseRegisteredSection> RegisteredSections { get; set; }
        /// <summary>
        /// List of sections with any kind of messages that occured during registration.
        /// </summary>
        public List<InstantEnrollmentRegistrationBaseMessage> RegistrationMessages { get; set; }

        /// <summary>
        /// A DTO containing the results of a proposed registration
        /// </summary>
        public InstantEnrollmentProposedRegistrationResult()
        {
            RegisteredSections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>();
            RegistrationMessages = new List<InstantEnrollmentRegistrationBaseMessage>();
        }

        
    }
}
