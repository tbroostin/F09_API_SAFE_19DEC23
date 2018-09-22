// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// An AR Payment - inherits from <see cref="ReceivableTransaction">ReceivableTransaction</see>
    /// </summary>
    public class ReceivablePayment : ReceivableTransaction
    {
        /// <summary>
        /// Amount of the payment
        /// </summary>
        public decimal Amount { get; set; }
    }
}
