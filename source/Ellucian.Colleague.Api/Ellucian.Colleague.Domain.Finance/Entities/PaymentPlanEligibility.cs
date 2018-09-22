// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// An individual's payment plan eligibility
    /// </summary>
    [Serializable]
    public class PaymentPlanEligibility
    {
        private readonly List<BillingTermPaymentPlanInformation> _paymentPlanEligibleItems = new List<BillingTermPaymentPlanInformation>();
        /// <summary>
        /// Collection of <see cref="BillingTermPaymentPlanInformation"/> objects.
        /// </summary>
        public ReadOnlyCollection<BillingTermPaymentPlanInformation> EligibleItems { get; private set; }

        private PaymentPlanIneligibilityReason? _paymentPlanIneligibilityReason;
        /// <summary>
        /// The <see cref="IneligibilityReason"/>.
        /// </summary>
        public PaymentPlanIneligibilityReason? IneligibilityReason { get { return _paymentPlanIneligibilityReason; } }

        /// <summary>
        /// Creates an instance of the <see cref="PaymentPlanEligibility"/> class.
        /// </summary>
        /// <param name="paymentPlanEligibleItems">Collection of <see cref="BillingTermPaymentPlanInformation"/> objects</param>
        /// <param name="paymentPlanIneligibilityReason">A <see cref="IneligibilityReason"/></param>
        public PaymentPlanEligibility(IEnumerable<BillingTermPaymentPlanInformation> paymentPlanEligibleItems, PaymentPlanIneligibilityReason? paymentPlanIneligibilityReason)
        {
            if ((paymentPlanEligibleItems == null || !paymentPlanEligibleItems.Any()) && paymentPlanIneligibilityReason == null)
            {
                throw new ApplicationException("Ineligibility reason must be specified when there are no payment plan-eligible items.");
            }
            _paymentPlanEligibleItems = paymentPlanEligibleItems != null ? paymentPlanEligibleItems.ToList() : new List<BillingTermPaymentPlanInformation>();
            EligibleItems = _paymentPlanEligibleItems.AsReadOnly();

            _paymentPlanIneligibilityReason = EligibleItems.Any() ? null : paymentPlanIneligibilityReason;
        }
    }
}
