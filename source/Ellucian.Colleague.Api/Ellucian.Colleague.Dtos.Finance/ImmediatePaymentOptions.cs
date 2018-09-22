// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.

using System;
namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Payment options for immediate payment control
    /// </summary>
    public class ImmediatePaymentOptions
    {
        /// <summary>
        /// Specifies whether related charges are associated with any payment plans
        /// </summary>
        public bool ChargesOnPaymentPlan { get; set; }

        /// <summary>
        /// The balance of the registration charges
        /// </summary>
        public decimal RegistrationBalance { get; set; }

        /// <summary>
        /// Minimum payment to be made by the student
        /// </summary>
        public decimal MinimumPayment { get; set; }

        /// <summary>
        /// Percentage of charges that the student can elect to pay at a later date
        /// </summary>
        public decimal DeferralPercentage { get; set; }

        /// <summary>
        /// ID of the payment plan template used in creating a payment plan for the student
        /// </summary>
        public string PaymentPlanTemplateId { get; set; }

        /// <summary>
        /// Date on which first payment plan scheduled payment is due
        /// </summary>
        public DateTime? PaymentPlanFirstDueDate { get; set; }

        /// <summary>
        /// Amount of the payment plan to be created
        /// </summary>
        public decimal PaymentPlanAmount { get; set; }

        /// <summary>
        /// Receivable Type Code of the payment plan to be created
        /// </summary>
        public string PaymentPlanReceivableTypeCode { get; set; }

        /// <summary>
        /// Amount of the payment plan down payment
        /// </summary>
        public decimal DownPaymentAmount { get; set; }

        /// <summary>
        /// Date on which down payment is due
        /// </summary>
        public DateTime? DownPaymentDate { get; set; }
    }
}
