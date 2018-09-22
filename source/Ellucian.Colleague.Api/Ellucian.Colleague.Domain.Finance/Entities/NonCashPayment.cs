// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A payment by means other than cash
    /// </summary>
    [Serializable]
    public class NonCashPayment
    {
        private readonly string _paymentMethodCode;
        private readonly decimal _amount;

        /// <summary>
        /// Payment method of the non-cash payment
        /// </summary>
        public string PaymentMethodCode { get { return _paymentMethodCode; } }

        /// <summary>
        /// Amount of the non-cash payment
        /// </summary>
        public decimal Amount { get { return _amount; } }

        /// <summary>
        /// Public constructor for a non-cash payment
        /// </summary>
        /// <param name="paymentMethod">The means by which the non-cash payment was tendered.</param>
        /// <param name="amount">the amount of the non-cash payment.</param>
        public NonCashPayment(string paymentMethod, decimal amount)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                throw new ArgumentNullException("paymentMethod", "Payment method cannot be null.");
            }

            if (amount == 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Amount must not be zero");
            }

            _paymentMethodCode = paymentMethod;
            _amount = amount;
        }
    }
}
