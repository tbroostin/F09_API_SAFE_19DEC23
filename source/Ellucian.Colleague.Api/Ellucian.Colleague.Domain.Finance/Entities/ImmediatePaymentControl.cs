// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Immediate payment control parameters
    /// </summary>
    [Serializable]
    public class ImmediatePaymentControl
    {
        private readonly bool _IsEnabled;

        /// <summary>
        /// Boolean identifying whether IPC is enabled
        /// </summary>
        public bool IsEnabled { get { return _IsEnabled; } }

        /// <summary>
        /// Document ID of registration acknowledgement
        /// </summary>
        public string RegistrationAcknowledgementDocumentId { get; set; }

        /// <summary>
        /// Document ID of terms and conditions
        /// </summary>
        public string TermsAndConditionsDocumentId { get; set; }

        /// <summary>
        /// ID of deferral document
        /// </summary>
        public string DeferralAcknowledgementDocumentId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="enabled">IPC enabled indicator</param>
        public ImmediatePaymentControl(bool enabled)
        {
            _IsEnabled = enabled;
        }
    }
}
