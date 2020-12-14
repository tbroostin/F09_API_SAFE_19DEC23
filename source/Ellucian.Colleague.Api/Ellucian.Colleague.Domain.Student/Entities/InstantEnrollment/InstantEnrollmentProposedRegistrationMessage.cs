// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]

    /// <summary>
    /// Contains a message (not necessarily an error message) from the registration process.
    /// The message may be a general message, or it may apply to a specific section.
    /// </summary>
    public class InstantEnrollmentProposedRegistrationMessage
    {
        /// <summary>
        /// The message returned by the registration process.  Required.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The section the message might apply to.  Not required.
        /// </summary>
        public string MessageSection { get; private set; }

       /// <summary>
       /// constructor to populate sectiona and message associtaed with it.
       /// </summary>
       /// <param name="messageSection"></param>
       /// <param name="message"></param>

        public InstantEnrollmentProposedRegistrationMessage(string messageSection, string message)
        {
            MessageSection = messageSection;
            Message = message;
        }
    }
}
