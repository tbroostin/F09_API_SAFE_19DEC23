// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// An e-commerce payment provider's information
    /// </summary>
    public class PaymentProvider
    {
        /// <summary>
        /// URL to payment site
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Error message if payment cannot be processed
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
