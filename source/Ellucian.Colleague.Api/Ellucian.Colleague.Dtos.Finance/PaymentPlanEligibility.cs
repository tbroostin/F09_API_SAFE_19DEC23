// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// An individual's payment plan eligibility
    /// </summary>
    public class PaymentPlanEligibility
    {
        /// <summary>
        /// Collection of payment plan information for a person for a billing term and receivable type.
        /// </summary>
        public IEnumerable<BillingTermPaymentPlanInformation> EligibleItems { get; set; }

        /// <summary>
        /// The reasoning for an individual's ineligibility to sign up for a payment plan.
        /// </summary>
        public PaymentPlanIneligibilityReason? IneligibilityReason { get; set; }
    }
}
