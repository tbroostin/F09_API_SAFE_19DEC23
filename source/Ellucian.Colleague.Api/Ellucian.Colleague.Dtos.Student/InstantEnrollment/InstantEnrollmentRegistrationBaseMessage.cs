// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains a message (not necessarily an error message) from the registration process.
    /// The message may be a general message, or it may apply to a specific section.
    /// </summary>
    public class InstantEnrollmentRegistrationBaseMessage
    {
        /// <summary>
        /// The message returned by the registration process.  Required.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The section the message might apply to.  Not required.
        /// </summary>
        public string MessageSection { get; set; }

        /// <summary>
        /// constructor to populate the message and optionally the section associated with it.
        /// </summary>
        /// <param name="messageSection"></param>
        /// <param name="message">Required.</param>
    }
}
