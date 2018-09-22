// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountDue
{
    /// <summary>
    /// An invoice due to be paid
    /// </summary>
    public class InvoiceDueItem : AccountsReceivableDueItem
    {
        /// <summary>
        /// Invoice ID number
        /// </summary>
        public string InvoiceId { get; set; }
    }
}
