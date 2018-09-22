// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Student's acceptance of the terms &amp; conditions for signing up for a payment plan
    /// </summary>
    public class PaymentPlanTermsAcceptance
    {
        /// <summary>
        /// ID of the student for whom the payment plan terms and conditions were accepted
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// ID of the registration payment control record to which the payment plan terms and conditions correspond
        /// </summary>
        public string PaymentControlId { get; set; }

        /// <summary>
        /// Date and time at which the payment plan terms and conditions were displayed to the student
        /// </summary>
        public DateTimeOffset AcknowledgementDateTime { get; set; }

        /// <summary>
        /// Name of the student for whom the payment plan terms and conditions were accepted
        /// </summary>
        public string StudentName { get; set; }

        /// <summary>
        /// The proposed payment plan to which the terms and conditions correspond
        /// </summary>
        public PaymentPlan ProposedPlan { get; set; }

        /// <summary>
        /// Amount of the down payment for the proposed payment plan
        /// </summary>
        public decimal DownPaymentAmount { get; set; }

        /// <summary>
        /// Date on which the proposed payment plan down payment is due
        /// </summary>
        public DateTime? DownPaymentDate { get; set; }

        /// <summary>
        /// The terms and conditions for the proposed payment plan
        /// </summary>
        public List<string> TermsText { get; set; }

        /// <summary>
        /// ID of the user who approved the payment plan terms and conditions
        /// </summary>
        public string ApprovalUserId { get; set; }

        /// <summary>
        /// Date and time at which the student accepted the payment plan terms and conditions
        /// </summary>
        public DateTimeOffset ApprovalReceived { get; set; }

        /// <summary>
        /// Message to the student regarding the terms and conditions for the proposed payment plan
        /// </summary>
        public List<string> AcknowledgementText { get; set; }

        /// <summary>
        /// ID of the registration approvals record corresponding to this plan acceptance
        /// </summary>
        public string RegistrationApprovalId { get; set; }
    }
}
