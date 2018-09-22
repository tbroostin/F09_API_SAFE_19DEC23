// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class TransferInvoice : ReceivablePayment
    {
        private readonly string _invoiceId;

        public string InvoiceId { get { return _invoiceId; } }

        public TransferInvoice(string id, string referenceNumber, string personId, string receivableType, string termId, DateTime date, decimal amount, string invoiceId)
            : base(id, referenceNumber, personId, receivableType, termId, date, amount)
        {
            _invoiceId = invoiceId;
        }
    }
}
