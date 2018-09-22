// Copyright 2016 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// An invoice payment - inherits from <see cref="Invoice">Invoice</see> and also include amount paid
    /// </summary>
    public class InvoicePayment : Invoice
    {
        /// <summary>
        /// Amount of paid on the invoice
        /// </summary>
        public decimal AmountPaid { get; set; }
    }
}
