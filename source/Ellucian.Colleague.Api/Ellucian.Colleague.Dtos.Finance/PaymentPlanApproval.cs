// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Point-in-time snapshot when the user approves a payment plan
    /// </summary>
    public class PaymentPlanApproval
    {
        /// <summary>
        /// ID of the payment plan approval
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the student for the payment plan approval
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Name of the student for the payment plan approval
        /// </summary>
        public string StudentName { get; set; }

        /// <summary>
        /// Date and time the approval was made
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// ID of the registration payment control record to which the payment plan approval corresponds
        /// </summary>
        public string PaymentControlId { get; set; }

        /// <summary>
        /// ID of payment plan to which approval corresponds
        /// </summary>
        public string PaymentPlanId { get; set; }

        /// <summary>
        /// ID of terms and conditions response
        /// </summary>
        public string TermsResponseId { get; set; }

        /// <summary>
        /// Template used to create original payment plan
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// ID of Acknowledgement text document
        /// </summary>
        public string AcknowledgementDocumentId { get; set; }

        /// <summary>
        /// Original amount of the plan
        /// </summary>
        public decimal PlanAmount { get; set; }

        /// <summary>
        /// The plan's down payment amount
        /// </summary>
        public decimal DownPaymentAmount { get; set; }

        /// <summary>
        /// The due date for the plan down payment
        /// </summary>
        public DateTime DownPaymentDate { get; set; }

        /// <summary>
        /// The amount of the plan's setup charge
        /// </summary>
        public decimal SetupChargeAmount { get; set; }

        /// <summary>
        /// The frequency of the plan's payments
        /// </summary>
        public PlanFrequency Frequency { get; set; }

        /// <summary>
        /// The number of payments on the plan, not including any down payment
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// The number of grace days before which a payment is considered past due
        /// </summary>
        public int GraceDays { get; set; }

        /// <summary>
        /// The flat amount charged when a payment is late
        /// </summary>
        public decimal LateChargeAmount { get; set; }

        /// <summary>
        /// The percentage charged as a late fee for an overdue payment
        /// </summary>
        public decimal LateChargePercentage { get; set; }

        /// <summary>
        /// The list of scheduled payments on the plan
        /// </summary>
        public IEnumerable<PlanSchedule> PlanSchedules { get; set; }
    }
}
