// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class PaymentRequirement
    {
        // Required fields
        private readonly string _id;
        private readonly string _termId;

        // Neither of the following fields are required, but one or the other is required
        private List<PaymentDeferralOption> _deferralOptions = new List<PaymentDeferralOption>();
        private List<PaymentPlanOption> _paymentPlanOptions = new List<PaymentPlanOption>();

        // Protected fields
        // The default requirement will not have a rule, but it can't be changed after it's created
        private readonly string _eligibilityRuleId;
        private readonly int _processingOrder;

        // Public properties
        /// <summary>
        /// ID of the payment requirement
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// ID of the billing term to which the payment requirement applies
        /// </summary>
        public string TermId { get { return _termId; } }

        /// <summary>
        /// ID of the rule for which the payment requirement is applied to a student
        /// </summary>
        public string EligibilityRuleId { get { return _eligibilityRuleId; } }

        /// <summary>
        /// Order in which the payment requirement is applied to the student, relative to other payment requirements
        /// </summary>
        public int ProcessingOrder { get { return _processingOrder; } }

        /// <summary>
        /// Collection of deferral options for the payment requirement
        /// </summary>
        public List<PaymentDeferralOption> DeferralOptions { get { return _deferralOptions; } }

        /// <summary>
        /// Collection of payment plan options for the payment requirement
        /// </summary>
        public List<PaymentPlanOption> PaymentPlanOptions { get { return _paymentPlanOptions; } }

        /// <summary>
        /// Constructor for the payment requirement entity
        /// </summary>
        /// <param name="id">ID of the payment requirement</param>
        /// <param name="termId">ID of the billing term for which the payment requirement is applicable</param>
        /// <param name="eligibilityRuleId">ID of the rule for which the payment requirement is applied to a student</param>
        /// <param name="processingOrder">Order in which the payment requirement is applied to the student</param>
        /// <param name="deferrals">Collection of deferral options for the payment requirement</param>
        /// <param name="plans">Collection of payment plan options for the payment requirement</param>
        public PaymentRequirement(string id, string termId, string eligibilityRuleId, int processingOrder, IEnumerable<PaymentDeferralOption> deferrals, IEnumerable<PaymentPlanOption> plans)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Payment requirement ID must be specified.");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId", "Term ID must be specified.");
            }
            if ((deferrals == null || deferrals.Count() == 0) && (plans == null || plans.Count() == 0))
            {
                throw new ArgumentException("Either a deferral option or a payment plan option is required.");
            }
            if (processingOrder < 0)
            {
                throw new ArgumentOutOfRangeException("processingOrder", "Processing Order cannot be a negative number");
            }

            _id = id;
            _termId = termId;
            _eligibilityRuleId = eligibilityRuleId;
            _processingOrder = processingOrder;
            if (deferrals != null && deferrals.Count() > 0)
            {
                _deferralOptions.AddRange(deferrals);
            }
            if (plans != null && plans.Count() > 0)
            {
                _paymentPlanOptions.AddRange(plans);
            }
        }
    }
}
