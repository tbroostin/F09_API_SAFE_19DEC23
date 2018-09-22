// Copyright 2015 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A non-cash payment
    /// </summary>
    public class NonCashPayment
    {
        /// <summary>
        /// Payment method of the non-cash payment
        /// </summary>
        public string PaymentMethodCode { get; set; }

        /// <summary>
        /// Amount of the non-cash payment
        /// </summary>
        public decimal Amount { get; set; }
    }
}
