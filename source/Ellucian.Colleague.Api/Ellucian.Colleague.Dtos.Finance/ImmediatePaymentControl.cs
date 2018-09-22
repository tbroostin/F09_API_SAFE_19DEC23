// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Parameters controlling immediate payment functionality
    /// </summary>
    public class ImmediatePaymentControl
    {
        /// <summary>
        /// Indicates whether Immediate Payment Control has been enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Document template for the registration acknowledgement
        /// </summary>
        public string RegistrationAcknowledgementDocumentId { get; set; }

        /// <summary>
        /// Document template for the registration terms and conditions
        /// </summary>
        public string TermsAndConditionsDocumentId { get; set; }

        /// <summary>
        /// Document template for display upon payment deferral
        /// </summary>
        public string DeferralAcknowledgementDocumentId { get; set; }
    }
}
