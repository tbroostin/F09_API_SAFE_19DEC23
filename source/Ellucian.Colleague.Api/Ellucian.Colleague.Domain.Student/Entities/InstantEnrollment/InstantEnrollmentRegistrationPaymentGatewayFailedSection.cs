// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Contains the section and messages for an Instant Enrollment section in which an 
    /// individual was not successfully enrolled
    /// </summary>
    [Serializable]
    public class InstantEnrollmentRegistrationPaymentGatewayFailedSection
    {
        /// <summary>
        /// The unique id of the section that was not successfully registered.
        /// </summary>
        public string SectionId { get; private set; }

        /// <summary>
        /// The messages associated with the failed registration which were returned by the registration process.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The constructor to populate the section that was not successfully registered.
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="message"></param>
        public InstantEnrollmentRegistrationPaymentGatewayFailedSection(string sectionId, string message)
        {
            SectionId = sectionId;
            Message = message;
        }
    }
}
