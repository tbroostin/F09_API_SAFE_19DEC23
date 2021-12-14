// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// An invoice payment - inherits from <see cref="Invoice">Invoice</see> and also include amount paid
    /// </summary>
    [Serializable]
    public class InvoicePayment : Invoice
    {
        /// <summary>
        /// Amount of paid on the invoice
        /// </summary>
        private readonly decimal _amountPaid;
        public decimal AmountPaid { get { return _amountPaid; } }

        private readonly decimal _balanceAmount;
        /// <summary>
        /// Amount of balance remaining to be paid. This balance is adjusted after applying all the adjusted invoices. 
        /// </summary>
        public decimal BalanceAmount { get { return _balanceAmount; } }

        public InvoicePayment(string id, string personId, string receivableType, string term, string referenceNumber, DateTime date, DateTime dueDate, DateTime billingStart, DateTime billingEnd, string description, IEnumerable<Charge> charges, decimal amountPaid, decimal balanceAmount)
            : base(id, personId, receivableType, term, referenceNumber, date, dueDate, billingStart, billingEnd, description, charges)
        {
            _amountPaid = amountPaid;
            _balanceAmount = balanceAmount;
        }

    }
}
