// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Payment plan information for a person for a billing term and receivable type
    /// </summary>
    [Serializable]
    public class BillingTermPaymentPlanInformation
    {
        private string _personId;
        private string _termId;
        private string _receivableTypeCode;
        private decimal _paymentPlanAmount;

        /// <summary>
        /// ID of the person for whom the payment plan information applies
        /// </summary>
        public string PersonId { get { return _personId; } }

        /// <summary>
        /// ID of the billing term for which the payment plan information applies
        /// </summary>
        public string TermId { get { return _termId; } }

        /// <summary>
        /// Receivable type for charges to which the payment plan information applies
        /// </summary>
        public string ReceivableTypeCode { get { return _receivableTypeCode; } }

        /// <summary>
        /// Amount of the payment plan for the billing term
        /// <remarks>Inbound objects will hold the proposed payment plan amount</remarks>
        /// <remarks>Outbound objects will hold the amount for which a payment plan can be created, which could be different</remarks>
        /// </summary>
        public decimal PaymentPlanAmount { get { return _paymentPlanAmount; } }

        /// <summary>
        /// ID of the payment plan template to be used in creating a payment plan from the associated person, term, receivable type, and amount information.
        /// </summary>
        public string PaymentPlanTemplateId { get; set; }

        /// <summary>
        /// The reasoning for an individual's ineligibility to sign up for a payment plan
        /// </summary>
        public PaymentPlanIneligibilityReason? IneligibilityReason { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="BillingTermPaymentPlanInformation"/> object.
        /// </summary>
        /// <param name="personId">ID of the person for whom the payment plan information applies</param>
        /// <param name="termId">ID of the billing term for which the payment plan information applies</param>
        /// <param name="receivableTypeCode">Receivable type for charges to which the payment plan information applies</param>
        /// <param name="paymentPlanAmount">Amount of the payment plan for the billing term</param>
        public BillingTermPaymentPlanInformation(string personId, string termId, string receivableTypeCode, decimal paymentPlanAmount,
            string paymentPlanTemplateId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A person ID must be specified.");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId", "A term ID must be specified.");
            }
            if (string.IsNullOrEmpty(receivableTypeCode))
            {
                throw new ArgumentNullException("receivableTypeCode", "A receivable type code must be specified.");
            }
            if (paymentPlanAmount <= 0)
            {
                throw new ArgumentException("paymentPlanAmount", "Payment plans may only be created for positive amounts.");
            }

            _personId = personId;
            _termId = termId;
            _receivableTypeCode = receivableTypeCode;
            _paymentPlanAmount = paymentPlanAmount;

            PaymentPlanTemplateId = paymentPlanTemplateId;
        }
    }
}
