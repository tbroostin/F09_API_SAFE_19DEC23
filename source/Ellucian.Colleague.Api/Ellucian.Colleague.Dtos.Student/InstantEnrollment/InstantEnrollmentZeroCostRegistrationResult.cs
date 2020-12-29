// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains the results of a zero cost registration of given classes for a given individual.
    /// </summary>
    public class InstantEnrollmentZeroCostRegistrationResult
    {
        /// <summary>
        /// This identifies if any errors occurred while registering a person for classes when the total cost is zero.
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
        /// The unique identifier of the person that registered for the class(es) when a successful registration occurs
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// DMI Registry Username of the person that registered for the class(es) when a successful registration occurs. 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// A DTO containing the results of a zero cost registration
        /// </summary>
        public InstantEnrollmentZeroCostRegistrationResult()
        {
            RegisteredSections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>();
            RegistrationMessages = new List<InstantEnrollmentRegistrationBaseMessage>();
        }
    }
}
