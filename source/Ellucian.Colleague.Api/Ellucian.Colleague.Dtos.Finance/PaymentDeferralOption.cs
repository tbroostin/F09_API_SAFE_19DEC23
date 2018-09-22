// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Option for deferring a percentage of charges in the Immediate Payment workflow
    /// </summary>
    public class PaymentDeferralOption
    {
        /// <summary>
        /// The date on which this deferral option takes effect
        /// </summary>
        public DateTime EffectiveStart { get; set; }

        /// <summary>
        /// The date on which this deferral option is no longer effective
        /// </summary>
        public DateTime? EffectiveEnd { get; set; }

        /// <summary>
        /// The percentage of charges that are deferrable for this deferral option
        /// </summary>
        public decimal DeferralPercent { get; set; }
    }
}
