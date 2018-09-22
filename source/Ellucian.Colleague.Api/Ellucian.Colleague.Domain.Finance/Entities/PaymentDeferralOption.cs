// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Defines a period during which payment deferrals may take place
    /// </summary>
    [Serializable]
    public class PaymentDeferralOption
    {
        private readonly DateTime _EffectiveStart;
        private readonly DateTime? _EffectiveEnd;
        private decimal _DeferralPercent;

        /// <summary>
        /// The date on which the deferral period starts
        /// </summary>
        public DateTime EffectiveStart { get { return _EffectiveStart; } }
        /// <summary>
        /// The date on which the deferral period ends; a null means it runs indefinitely
        /// </summary>
        public DateTime? EffectiveEnd { get { return _EffectiveEnd; } }
        /// <summary>
        /// The percentage of charges that are deferred during this period
        /// </summary>
        public decimal DeferralPercent { get { return _DeferralPercent; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="effectiveStart">Period starting date</param>
        /// <param name="effectiveEnd">Period ending date</param>
        /// <param name="deferralPercent">Payment deferral percentage</param>
        public PaymentDeferralOption(DateTime effectiveStart, DateTime? effectiveEnd, decimal deferralPercent)
        {
            // Start date must be before end date
            if ((effectiveEnd != null) && (effectiveStart.Date > effectiveEnd.Value.Date))
            {
                throw new ArgumentOutOfRangeException("effectiveStart", "The start date cannot be after the end date.");
            }

            // Deferral Percent is required, and must be between 0 and 100
            if (deferralPercent < 0 || deferralPercent > 100)
            {
                throw new ArgumentOutOfRangeException("deferralPercent", "The deferral percent must be between 0 and 100");
            }
            _EffectiveStart = effectiveStart;
            _EffectiveEnd = effectiveEnd;
            _DeferralPercent = deferralPercent;
        }
    }
}
