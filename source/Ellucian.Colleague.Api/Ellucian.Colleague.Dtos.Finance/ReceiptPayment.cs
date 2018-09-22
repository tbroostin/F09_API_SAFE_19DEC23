// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A Cash Receipt Payment - inherits from <see cref="ReceivablePayment">ReceivablePayment</see>
    /// </summary>
    public class ReceiptPayment : ReceivablePayment
    {
        /// <summary>
        /// ID of the cash receipt
        /// </summary>
        public string ReceiptId { get; set; }
    }
}
