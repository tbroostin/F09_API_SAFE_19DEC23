// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Student's acceptance of the terms & conditions for signing up for a payment plan
    /// </summary>
    [Serializable]
    public class PaymentPlanTermsAcceptance
    {
        private readonly string _studentId;
        private readonly DateTimeOffset _acknowledgementDateTime;
        private readonly string _studentName;
        private readonly PaymentPlan _proposedPlan;
        private readonly decimal _downPaymentAmount;
        private readonly DateTime? _downPaymentDate;
        private readonly string _approvalUserId;
        private readonly DateTimeOffset _approvalReceived;
        private readonly List<string> _termsText = new List<string>();
        private string _paymentControlId;
        private string _registrationApprovalId;

        /// <summary>
        /// ID of the student for whom the payment plan terms and conditions were accepted
        /// </summary>
        public string StudentId { get { return _studentId; } }

        /// <summary>
        /// Date and time at which the payment plan terms and conditions were displayed to the student
        /// </summary>
        public DateTimeOffset AcknowledgementDateTime { get { return _acknowledgementDateTime; } }

        /// <summary>
        /// Name of the student for whom the payment plan terms and conditions were accepted
        /// </summary>
        public string StudentName { get { return _studentName; } }

        /// <summary>
        /// The proposed payment plan to which the terms and conditions correspond
        /// </summary>
        public PaymentPlan ProposedPlan { get { return _proposedPlan; } }

        /// <summary>
        /// Amount of the down payment for the proposed payment plan
        /// </summary>
        public decimal DownPaymentAmount { get { return _downPaymentAmount; } }

        /// <summary>
        /// Date on which the proposed payment plan down payment is due
        /// </summary>
        public DateTime? DownPaymentDate { get { return _downPaymentDate; } }
        
        /// <summary>
        /// The terms and conditions for the proposed payment plan
        /// </summary>
        public ReadOnlyCollection<string> TermsText { get; private set; }
        
        /// <summary>
        /// ID of the user who approved the payment plan terms and conditions
        /// </summary>
        public string ApprovalUserId { get { return _approvalUserId; } }
        
        /// <summary>
        /// Date and time at which the student accepted the payment plan terms and conditions
        /// </summary>
        public DateTimeOffset ApprovalReceived { get { return _approvalReceived; } }

        /// <summary>
        /// Message to the student regarding the terms and conditions for the proposed payment plan
        /// </summary>
        public List<string> AcknowledgementText { get; set; }

        /// <summary>
        /// ID of the registration payment control record to which the payment plan terms and conditions correspond
        /// </summary>
        public string PaymentControlId 
        { 
            get { return _paymentControlId; }
            set
            {
                if (string.IsNullOrEmpty(_paymentControlId))
                {
                    _paymentControlId = value;
                }
                else
                {
                    throw new InvalidOperationException("Cannot change payment control ID.");
                }
            }
        }

        /// <summary>
        /// ID of the registration approvals record corresponding to this plan acceptance
        /// </summary>
        public string RegistrationApprovalId { get { return _registrationApprovalId; } }

        /// <summary>
        /// Constructor for the payment plan terms acceptance 
        /// </summary>
        /// <param name="studentId">ID of the student for whom the payment plan terms and conditions were accepted</param>
        /// <param name="acknowledgementDateTime">Date and time at which the payment plan terms and conditions were displayed to the student</param>
        /// <param name="studentName">Name of the student for whom the payment plan terms and conditions were accepted</param>
        /// <param name="templateId">ID of the payment plan template used to create the proposed payment plan</param>        
        /// <param name="proposedPlan">The proposed payment plan to which the terms and conditions correspond</param>
        /// <param name="downPaymentAmount">Amount of the down payment for the proposed payment plan</param>
        /// <param name="downPaymentDate">Date on which the proposed payment plan down payment is due</param>
        /// <param name="approvalReceived">Date and time at which the student accepted the payment plan terms and conditions</param>
        /// <param name="approvalUserId">ID of the user who approved the payment plan terms and conditions</param>
        /// <param name="termsText">The terms and conditions for the proposed payment plan</param>
        public PaymentPlanTermsAcceptance(string studentId, DateTimeOffset acknowledgementDateTime, string studentName, PaymentPlan proposedPlan,
            decimal downPaymentAmount, DateTime? downPaymentDate, DateTimeOffset approvalReceived, string approvalUserId, IEnumerable<string> termsText)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID must be provided.");
            }
            if (proposedPlan == null)
            {
                throw new ArgumentNullException("proposedPlan", "Proposed Payment Plan must be provided.");
            }
            if (downPaymentAmount < 0)
            {
                throw new ArgumentOutOfRangeException("downPaymentAmount", "Payment Plan Down Payment cannot be less than zero.");
            }
            if ((downPaymentAmount != 0 && downPaymentDate == null) || (downPaymentAmount == 0 && downPaymentDate != null))
            {
                throw new ArgumentOutOfRangeException("downPaymentDate", "A down payment date and down payment amount must be specified together, or neither may be specified.");
            }
            if (string.IsNullOrEmpty(approvalUserId))
            {
                throw new ArgumentNullException("approvalUserId", "Approval User ID must be provided.");
            }
            if (termsText == null || termsText.Count() < 1)
            {
                throw new ArgumentNullException("termsText", "Terms and conditions text is required");
            }

            _studentId = studentId;
            _acknowledgementDateTime = acknowledgementDateTime;
            _studentName = studentName;
            _proposedPlan = proposedPlan;
            _downPaymentAmount = downPaymentAmount;
            _downPaymentDate = downPaymentDate;
            _termsText = termsText.ToList();
            _approvalReceived = approvalReceived;
            _approvalUserId = approvalUserId;

            TermsText = _termsText.AsReadOnly();
        }

        /// <summary>
        /// Constructor for the payment plan terms acceptance
        /// </summary>
        /// <param name="studentId">ID of the student for whom the payment plan terms and conditions were accepted</param>
        /// <param name="acknowledgementDateTime">Date and time at which the payment plan terms and conditions were displayed to the student</param>
        /// <param name="studentName">Name of the student for whom the payment plan terms and conditions were accepted</param>
        /// <param name="templateId">ID of the payment plan template used to create the proposed payment plan</param>        
        /// <param name="proposedPlan">The proposed payment plan to which the terms and conditions correspond</param>
        /// <param name="downPaymentAmount">Amount of the down payment for the proposed payment plan</param>
        /// <param name="downPaymentDate">Date on which the proposed payment plan down payment is due</param>
        /// <param name="approvalReceived">Date and time at which the student accepted the payment plan terms and conditions</param>
        /// <param name="approvalUserId">ID of the user who approved the payment plan terms and conditions</param>
        /// <param name="termsText">The terms and conditions for the proposed payment plan</param>
        /// <param name="registrationApprovalId">ID of the registration approvals record corresponding to this plan acceptance</param>
        public PaymentPlanTermsAcceptance(string studentId, DateTimeOffset acknowledgementDateTime, string studentName, PaymentPlan proposedPlan,
            decimal downPaymentAmount, DateTime? downPaymentDate, DateTimeOffset approvalReceived, string approvalUserId, IEnumerable<string> termsText, string registrationApprovalId)
            : this(studentId, acknowledgementDateTime, studentName, proposedPlan, downPaymentAmount, downPaymentDate, approvalReceived, approvalUserId, termsText)
        {
            if (string.IsNullOrEmpty(registrationApprovalId))
            {
                throw new ArgumentNullException("registrationApprovalId", "Registration Approvals ID must be provided.");
            }
            _registrationApprovalId = registrationApprovalId;
        }
    }
}
