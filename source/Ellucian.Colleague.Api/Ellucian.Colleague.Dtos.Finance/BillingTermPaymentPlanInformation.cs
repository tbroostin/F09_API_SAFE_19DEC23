// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Payment plan information for a person for a billing term and receivable type
    /// </summary>
    public class BillingTermPaymentPlanInformation
    {
        /// <summary>
        /// ID of the person for whom the payment plan information applies
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// ID of the billing term for which the payment plan information applies
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// Receivable type for charges to which the payment plan information applies
        /// </summary>
        public string ReceivableTypeCode { get; set; }

        /// <summary>
        /// Amount of the payment plan for the billing term
        /// <remarks>Inbound objects will hold the proposed payment plan amount</remarks>
        /// <remarks>Outbound objects will hold the amount for which a payment plan can be created, which could be different</remarks>
        /// </summary>
        public decimal PaymentPlanAmount { get; set; }

        /// <summary>
        /// ID of the payment plan template to be used in creating a payment plan from the associated person, term, receivable type, and amount information.
        /// </summary>
        public string PaymentPlanTemplateId { get; set; }

        /// <summary>
        /// The reasoning for an individual's ineligibility to sign up for a payment plan
        /// </summary>
        public PaymentPlanIneligibilityReason? IneligibilityReason { get; set; }
    }
}
