// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class ReceiptPayment : ReceivablePayment
    {
        private readonly string _receiptId;

        public string ReceiptId { get { return _receiptId; } }

        public ReceiptPayment(string id, string referenceNumber, string personId, string receivableType, string termId, DateTime date, decimal amount, string receiptId)
            : base(id, referenceNumber, personId, receivableType, termId, date, amount)
        {
            if (string.IsNullOrEmpty(receiptId))
            {
                throw new ArgumentNullException("receiptId", "Receipt ID cannot be null.");
            }

            _receiptId = receiptId;
        }
    }
}
