// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Option for creating a payment plan in the Immediate Payment workflow
    /// </summary>
    public class PaymentPlanOption
    {
        /// <summary>
        /// The date on which this payment plan option takes effect
        /// </summary>
        public DateTime EffectiveStart { get; set; }

        /// <summary>
        /// The date on which this payment plan option is no longer effective
        /// </summary>
        public DateTime? EffectiveEnd { get; set; }

        /// <summary>
        /// ID of the payment plan template associated with this payment plan option
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// The due date assigned to the first scheduled payment for payment plans created using this payment plan option
        /// </summary>
        public DateTime? FirstPaymentDate { get; set; }
    }
}
