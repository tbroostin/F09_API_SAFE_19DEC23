// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains the information common to all Instant Enrollment registration endpoint results, 
    /// as well as the cash receipt that was created.
    /// </summary>
    public class InstantEnrollmentEcheckRegistrationResult
    {
        /// <summary>
        /// Indicates whether an error occurred during the attempted registration.
        /// </summary>
        public bool ErrorOccurred { get; set; }

        /// <summary>
        /// The list of successfully registered sections and related information.
        /// </summary>
        public List<InstantEnrollmentRegistrationBaseRegisteredSection> RegisteredSections { get; set; }
        /// <summary>
        /// List of messages that appeared while doing registration.
        /// </summary>
        public List<InstantEnrollmentRegistrationBaseMessage> RegistrationMessages { get; set; }

        /// <summary>
        /// The unique identifier of the person that registered for the class(es) when a successful registration occurs
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The cash receipt that was created after paying for registration.
        /// </summary>
        public string CashReceipt { get; set; }

        /// <summary>
        /// DMI Registry Username of the person that registered for the class(es) when a successful registration occurs. 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Initilizes a new InstantEnrollmentEcheckRegistrationResult
        /// </summary>
        public InstantEnrollmentEcheckRegistrationResult()
        {
            RegisteredSections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>();
            RegistrationMessages = new List<InstantEnrollmentRegistrationBaseMessage>();
        }
    }
}
