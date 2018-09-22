// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Point-in-time snapshot when the user approves a payment plan
    /// </summary>
    [Serializable]
    public class PaymentPlanApproval
    {
        private string _id;
        private readonly string _studentId;
        private readonly string _studentName;
        private readonly DateTimeOffset _timestamp;
        private readonly string _paymentPlanId;
        private readonly string _termsResponseId;
        private readonly string _templateId;
        private readonly decimal _planAmount;
        private readonly List<PlanSchedule> _planSchedules = new List<PlanSchedule>();
        private string _paymentControlId;

        /// <summary>
        /// ID of the payment plan approval
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// ID of the student for the payment plan approval
        /// </summary>
        public string StudentId { get { return _studentId; } }

        /// <summary>
        /// Name of the student for the payment plan approval
        /// </summary>
        public string StudentName { get { return _studentName; } }

        /// <summary>
        /// The date and time the approval was given
        /// </summary>
        public DateTimeOffset Timestamp { get { return _timestamp; } }

        /// <summary>
        /// ID of payment plan to which terms and conditions approval corresponds
        /// </summary>
        public string PaymentPlanId { get { return _paymentPlanId; } }

        /// <summary>
        /// ID of terms and conditions response
        /// </summary>
        public string TermsResponseId { get { return _termsResponseId; } }

        /// <summary>
        /// Template used to create original payment plan
        /// </summary>
        public string TemplateId { get { return _templateId; } }

        /// <summary>
        /// Original amount of the plan
        /// </summary>
        public decimal PlanAmount { get { return _planAmount; } }

        /// <summary>
        /// ID of acknowledgement document
        /// </summary>
        public string AcknowledgementDocumentId { get; set; }

        /// <summary>
        /// ID of the registration payment control record to which the payment plan terms approval corresponds
        /// </summary>
        public string PaymentControlId
        {
            get
            {
                return _paymentControlId;
            }
            set
            {
                if (string.IsNullOrEmpty(_paymentControlId))
                {
                    _paymentControlId = value;
                }
                else
                {
                    throw new InvalidOperationException("Payment Plan Approval ID cannot be changed.");
                }
            }
        }

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
        public ReadOnlyCollection<PlanSchedule> PlanSchedules { get; private set; }

        /// <summary>
        /// Constructor for the payment plan terms approval
        /// </summary>
        /// <param name="id">ID of the registration approval</param>
        /// <param name="studentId">Student ID number</param>
        /// <param name="studentName">Student name when the approval was made</param>
        /// <param name="timestamp">Date and time the approval was made</param>
        /// <param name="templateId">ID of payment plan template used</param>
        /// <param name="paymentPlanId">Payment Plan to which terms and conditions approval corresponds</param>
        /// <param name="termsResponseId">Terms and conditions response</param>
        /// <param name="planAmount">Amount of the plan being created</param>
        /// <param name="planSchedules">List of scheduled dates/payments on the plan</param>
        public PaymentPlanApproval(string id, string studentId, string studentName, DateTimeOffset timestamp, string templateId, 
            string paymentPlanId, string termsResponseId, decimal planAmount, IEnumerable<PlanSchedule> planSchedules)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID must be provided.");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID must be provided.");
            }
            if (string.IsNullOrEmpty(studentName))
            {
                throw new ArgumentNullException("studentName", "Student Name must be provided.");
            }
            if (string.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException("templateId", "Template ID must be provided.");
            }
            if (string.IsNullOrEmpty(paymentPlanId))
            {
                throw new ArgumentNullException("paymentPlanId", "Payment plan ID must be provided.");
            }
            if (string.IsNullOrEmpty(termsResponseId))
            {
                throw new ArgumentNullException("termsResponseId", "The terms approval response must be provided.");
            }
            if (planSchedules == null || planSchedules.Count() == 0)
            {
                throw new ArgumentNullException("planSchedules", "The payment plan schedule must be provided.");
            }

            _id = id;
            _studentId = studentId;
            _studentName = studentName;
            _timestamp = timestamp;
            _paymentPlanId = paymentPlanId;
            _termsResponseId = termsResponseId;
            _templateId = templateId;
            _planAmount = planAmount;
            _planSchedules.AddRange(planSchedules);

            PlanSchedules = _planSchedules.AsReadOnly();
        }
    }
}
