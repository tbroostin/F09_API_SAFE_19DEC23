// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that controls how transcript requests or enrollment requests should be rendered in self-service.
    /// </summary>
    [Serializable]
    public class StudentRequestConfiguration
    {
        /// <summary>
        /// Default Email Type for the student when they are to receive an email confirmation after submitting a transcript request or enrollment verification in self-service
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

        /// <summary>
        /// Constructor for StudentRequestConfiguration
        /// </summary>
        public StudentRequestConfiguration()
        {
            SendTranscriptRequestConfirmation = false;
            SendEnrollmentRequestConfirmation = false;
            TranscriptRequestRequireImmediatePayment = false;
            EnrollmentRequestRequireImmediatePayment = false;
        }
    }
}
