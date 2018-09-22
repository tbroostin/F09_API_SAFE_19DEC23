// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Defines a time period during which a student can setup a payment plan through Immediate Payment Control
    /// </summary>
    [Serializable]
    public class PaymentPlanOption
    {
        private DateTime _effectiveStart;
        private DateTime _effectiveEnd;
        private string _templateId;
        private DateTime _firstPaymentDate;

        /// <summary>
        /// Date on which the payment plan option takes effect
        /// </summary>
        public DateTime EffectiveStart { get { return _effectiveStart; } }

        /// <summary>
        /// Date after which the payment plan option is no longer effective
        /// </summary>
        public DateTime EffectiveEnd { get { return _effectiveEnd; } }

        /// <summary>
        /// ID of the payment plan template for the payment plan option
        /// </summary>
        public string TemplateId { get { return _templateId; } }

        /// <summary>
        /// Date of the first scheduled payment for payment plans created from the payment plan option
        /// </summary>
        public DateTime FirstPaymentDate { get { return _firstPaymentDate; } }

        /// <summary>
        /// Constructor for payment plan option
        /// </summary>
        /// <param name="effectiveStart">Start date of the payment plan option</param>
        /// <param name="effectiveEnd">End date of the payment plan option</param>
        /// <param name="templateId">ID of the payment plan template</param>
        /// <param name="firstPaymentDate">First scheduled payment date for plans created from the payment plan option</param>
        public PaymentPlanOption(DateTime effectiveStart, DateTime effectiveEnd, string templateId, DateTime firstPaymentDate)
        {
            // Template ID cannot be null or empty
            if (string.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException("templateId", "Payment Plan Template ID must be specified.");
            }

            // Start date must be before end date
            if (effectiveStart.Date > effectiveEnd.Date)
            {
                throw new ArgumentOutOfRangeException("effectiveStart", "The effective start date must be on or before the effective end date.");
            }
                        
            _effectiveStart = effectiveStart;
            _effectiveEnd = effectiveEnd;
            _templateId = templateId;
            _firstPaymentDate = firstPaymentDate;
        }
    }
}
