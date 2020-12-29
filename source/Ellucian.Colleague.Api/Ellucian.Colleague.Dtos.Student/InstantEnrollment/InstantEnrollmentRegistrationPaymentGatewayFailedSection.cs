// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains the section and messages for an Instant Enrollment section in which an 
    /// individual was not successfully enrolled
    /// </summary>
    public class InstantEnrollmentRegistrationPaymentGatewayFailedSection
    {
        /// <summary>
        /// The unique id of the section that was not successfully registered.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// The message returned by the registration process for the section that failed registration.
        /// </summary>
        public string Message { get; set; }
    }
}
