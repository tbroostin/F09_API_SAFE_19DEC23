// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Record of a student's registration in the Immediate Payment workflow
    /// </summary>
    public class RegistrationPaymentControl
    {
        /// <summary>
        /// ID of the registration payment control record
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the student
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// ID of the term for which the registration was submitted
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// Indicates whether the registration was successfully completed
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RegistrationPaymentStatus PaymentStatus { get; set; }

        /// <summary>
        /// List of sections in which the student was successfully registered
        /// </summary>
        public IEnumerable<string> RegisteredSectionIds { get; set; }

        /// <summary>
        /// List of associated billing records
        /// </summary>
        public IEnumerable<string> InvoiceIds { get; set; }

        /// <summary>
        /// List of associated academic credits
        /// </summary>
        public IEnumerable<string> AcademicCredits { get; set; }

        /// <summary>
        /// List of associated payments
        /// </summary>
        public IEnumerable<string> Payments { get; set; }

        /// <summary>
        /// ID of the student's most recent approval of the Terms &amp; Conditions
        /// </summary>
        public string LastTermsApprovalId { get; set; }

        /// <summary>
        /// ID of the last payment plan approval record
        /// </summary>
        public string LastPlanApprovalId { get; set; }

        /// <summary>
        /// ID of the associated payment plan
        /// </summary>
        public string PaymentPlanId { get; set; }
    }
}
