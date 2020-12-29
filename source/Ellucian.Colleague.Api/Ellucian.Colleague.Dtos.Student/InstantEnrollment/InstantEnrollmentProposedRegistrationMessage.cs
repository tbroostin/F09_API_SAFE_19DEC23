// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains a message (not necessarily an error message) from the registration process.
    /// The message may be a general message, or it may apply to a specific section.
    /// </summary>
    public class InstantEnrollmentProposedRegistrationMessage
    {
        /// <summary>
        /// The message returned by the registration process.  
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The section the message might apply to.  
        /// </summary>
        public string MessageSection { get;  set; }

    }
}
