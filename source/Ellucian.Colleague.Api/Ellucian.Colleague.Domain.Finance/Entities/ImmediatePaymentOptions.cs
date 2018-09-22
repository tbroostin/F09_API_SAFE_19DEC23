// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class ImmediatePaymentOptions
    {
        // Private fields
        private readonly bool _chargesOnPaymentPlan;
        private readonly decimal _registrationBalance;
        private readonly decimal _minimumPayment;
        private readonly decimal _deferralPercentage;
        private string _paymentPlanTemplateId;
        private DateTime? _paymentPlanFirstDueDate;
        private decimal _paymentPlanAmount;
        private string _paymentPlanReceivableTypeCode;
        private decimal _downPaymentAmount;
        private DateTime? _downPaymentDate;

        /// <summary>
        /// Specifies whether IPC-related charges are associated with any payment plans
        /// </summary>
        public bool ChargesOnPaymentPlan { get { return _chargesOnPaymentPlan; } }

        /// <summary>
        /// The balance of the IPC-related charges; sum of all IPC invoices, less any payments made against them
        /// </summary>
        public decimal RegistrationBalance { get { return _registrationBalance; } }

        /// <summary>
        /// Minimum payment to be made by the student via IPC;
        /// </summary>
        public decimal MinimumPayment { get { return _minimumPayment; } }

        /// <summary>
        /// Percentage of charges that the student can elect to pay at a later date
        /// </summary>
        public decimal DeferralPercentage { get { return _deferralPercentage; } }

        /// <summary>
        /// ID of the payment plan template used in creating a payment plan for the student
        /// </summary>
        public string PaymentPlanTemplateId { get { return _paymentPlanTemplateId; } }

        /// <summary>
        /// Date on which first payment plan scheduled payment is due
        /// </summary>
        public DateTime? PaymentPlanFirstDueDate { get { return _paymentPlanFirstDueDate; } }

        /// <summary>
        /// Amount of the payment plan to be created
        /// </summary>
        public decimal PaymentPlanAmount { get { return _paymentPlanAmount; } }

        /// <summary>
        /// Receivable Type Code of the payment plan to be created
        /// </summary>
        public string PaymentPlanReceivableTypeCode { get { return _paymentPlanReceivableTypeCode; } }

        /// <summary>
        /// Amount of the payment plan down payment
        /// </summary>
        public decimal DownPaymentAmount { get { return _downPaymentAmount; } }

        /// <summary>
        /// Date on which the down payment is due
        /// </summary>
        public DateTime? DownPaymentDate { get { return _downPaymentDate; } }

        /// <summary>
        /// Constructor for immediate payment options entity
        /// </summary>
        /// <param name="chargesOnPaymentPlan">Are registration charges on a payment plan?</param>
        /// <param name="registrationBalance">Registration balance amount</param>
        /// <param name="minimumPayment">Minimum payment amount</param>
        /// <param name="deferralPercentage">Deferral percentage (0 to 100)</param>
        public ImmediatePaymentOptions(bool chargesOnPaymentPlan, decimal registrationBalance, decimal minimumPayment, decimal deferralPercentage)
        {
            if (registrationBalance < 0)
            {
                throw new ArgumentOutOfRangeException("registrationBalance", "Registration balance cannot be negative.");
            }
            if (minimumPayment < 0)
            {
                throw new ArgumentOutOfRangeException("minimumPayment", "Minimum payment cannot be negative.");
            }
            if (minimumPayment > registrationBalance)
            {
                throw new ArgumentException("Minimum payment cannot be more than the registration balance.");
            }
            if (deferralPercentage < 0 || deferralPercentage > 100)
            {
                throw new ArgumentOutOfRangeException("deferralPercentage", "Deferral percentage must be between 0 and 100, inclusive.");
            }

            _chargesOnPaymentPlan = chargesOnPaymentPlan;
            _registrationBalance = registrationBalance;
            _minimumPayment = minimumPayment;
            _deferralPercentage = deferralPercentage;
        }

        /// <summary>
        /// Add payment plan information to payment options
        /// </summary>
        /// <param name="paymentPlanTemplateId">Payment Plan Template ID</param>
        /// <param name="paymentPlanFirstDueDate">Date on which the first payment plan scheduled payment is due</param>
        /// <param name="paymentPlanAmount">Amount of the payment plan to be created</param>
        /// <param name="paymentPlanReceivableTypeCode">Receivable Type Code of the payment plan to be created</param>
        /// <param name="downPaymentAmount">Amount of the payment plan down payment</param>
        /// <param name="downPaymentDate">Date on which down payment is due</param>
        public void AddPaymentPlanInformation(string paymentPlanTemplateId, DateTime? paymentPlanFirstDueDate, decimal paymentPlanAmount,
            string paymentPlanReceivableTypeCode, decimal downPaymentAmount, DateTime? downPaymentDate)
        {
            if (!string.IsNullOrEmpty(paymentPlanTemplateId) && !paymentPlanFirstDueDate.HasValue)
            {
                throw new ArgumentNullException("paymentPlanFirstDueDate", "First Due Date cannot be null when a template ID is provided.");
            }
            if (!string.IsNullOrEmpty(paymentPlanTemplateId) && paymentPlanAmount <= 0)
            {
                throw new ArgumentOutOfRangeException("paymentPlanAmount", "Payment Plan Amount must be greater than zero when a template ID is provided.");
            }
            if (string.IsNullOrEmpty(paymentPlanTemplateId) && paymentPlanAmount > 0)
            {
                throw new ArgumentOutOfRangeException("paymentPlanAmount", "Payment Plan Amount cannot be greater than zero when a template ID is not provided.");
            }
            if (string.IsNullOrEmpty(paymentPlanReceivableTypeCode) && paymentPlanAmount > 0)
            {
                throw new ArgumentNullException("paymentPlanAmount", "Payment Plan Amount cannot be greater than zero when a receivable type code is not provided.");
            }
            if (downPaymentAmount > paymentPlanAmount)
            {
                throw new ArgumentOutOfRangeException("downPaymentAmount", "Down Payment Amount cannot be greater than the Payment Plan Amount.");
            }
            if (downPaymentAmount > 0 && !downPaymentDate.HasValue)
            {
                throw new ArgumentNullException("downPaymentDate", "Down Payment Date is required if there is a down payment.");
            }
            if (downPaymentDate < DateTime.Today)
            {
                throw new ArgumentOutOfRangeException("downPaymentDate", "Down Payment Date cannot be earlier than today's date.");
            }
        
            _paymentPlanTemplateId = paymentPlanTemplateId;
            _paymentPlanFirstDueDate = paymentPlanFirstDueDate;
            _paymentPlanAmount = paymentPlanAmount;
            _paymentPlanReceivableTypeCode = paymentPlanReceivableTypeCode;
            _downPaymentAmount = downPaymentAmount;
            _downPaymentDate = downPaymentDate;
        }
    }
}
