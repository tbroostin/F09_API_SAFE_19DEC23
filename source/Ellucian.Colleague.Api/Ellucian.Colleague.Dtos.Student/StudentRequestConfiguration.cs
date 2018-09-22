// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains information that controls how student requests should be rendered for a user in self-service.
    /// </summary>
    public class StudentRequestConfiguration
    {
        /// <summary>
        /// Default Email Type for the student when they are to receive an email confirmation for any type of student request (transript request or enrollment verification)
        /// </summary>
        public string DefaultWebEmailType { get; set; }

        /// <summary>
        ///Indicates whether student will receive an email with a confirmation on entering a transcript request
        /// </summary>
        public bool SendTranscriptRequestConfirmation { get; set; }

        /// <summary>
        ///Indicates whether student will receive an email with a confirmation on entering an enrollment request
        /// </summary>
        public bool SendEnrollmentRequestConfirmation { get; set; }

        /// <summary>
        /// Indicates whether transcript request requires immediate payment
        /// </summary>
        public bool TranscriptRequestRequireImmediatePayment { get; set; }

        /// <summary>
        /// Indicates whether enrollment request requires immediate payment
        /// </summary>
        public bool EnrollmentRequestRequireImmediatePayment { get; set; }

    }
}
