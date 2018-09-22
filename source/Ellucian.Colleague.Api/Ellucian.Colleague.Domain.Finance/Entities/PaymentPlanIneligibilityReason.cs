// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Reasoning for an individual's ineligibility to sign up for a payment plan
    /// </summary>
    [Serializable]
    public enum PaymentPlanIneligibilityReason
    {
        /// <summary>
        /// System configuration prevents the user from creating a payment plan
        /// </summary>
        /// <remarks>
        /// Reasons may include, but are not limited to:
        /// - Corrupted system files pertaining to configuration
        /// - User payment plan creation disabled
        /// - No default or rule-based payment plan requirements defined for a billing term
        /// - Corrupted payment plan template file(s)
        /// - Payment Plan template compatibility for payment plan creation (must be active, 
        /// Auto Calculate Plan Amount = Yes,
        /// Modify Plan Automatically = Yes, 
        /// Terms and Conditions document specified)
        /// </remarks>
        PreventedBySystemConfiguration,

        /// <summary>
        /// Payment items charges are not eligible for inclusion on a payment plan
        /// </summary>
        /// <remarks>
        /// Reasons may include, but are not limited to:
        /// - No payment items with a payable receivable type
        /// - Payment Plan template compatibility for payment plan creation (payment item amount >= Minimum Plan Amount,
        /// Allowed Receivable Types permits payment item's receivable type)
        /// </remarks>
        ChargesAreNotEligible
    }
}
