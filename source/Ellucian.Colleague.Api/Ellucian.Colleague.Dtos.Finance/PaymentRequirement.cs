// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Controls the payment options available to the student in the Immediate Payment workflow
    /// </summary>
    public class PaymentRequirement
    {
        /// <summary>
        /// ID of the payment requirement
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the term to which the payment requirement applies
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// ID of the eligibility rule that a student must pass for the payment requirement to be applicable
        /// </summary>
        public string EligibilityRuleId { get; set; }

        /// <summary>
        /// List of payment deferral options for the payment requirement
        /// </summary>
        public IEnumerable<PaymentDeferralOption> DeferralOptions { get; set; }

        /// <summary>
        /// List of payment plan options for the payment requirement
        /// </summary>
        public IEnumerable<PaymentPlanOption> PaymentPlanOptions { get; set; }
    }
}
