// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class PaymentRefund : ReceivablePayment
    {
        private readonly string _reason;

        public string Reason { get { return _reason; } }

        public string CheckNumber { get; set; }

        public string PaymentMethod { get; set; }

        public PaymentRefund(string id,  string referenceNumber, string personId, string receivableType, string termId,DateTime date, decimal amount, string reason)
            : base(id, referenceNumber, personId, receivableType, termId, date, amount)
        {
            _reason = reason;
        }
    }
}
