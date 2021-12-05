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
        /// <summary>
        /// Amount of balance remaining to be paid. This balance is adjusted after applying all the adjusted invoices. 
        /// </summary>
        public decimal BalanceAmount { get; set; }
    }
}
