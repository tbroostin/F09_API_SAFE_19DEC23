// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class PayrollDeduction : ReceivablePayment
    {
        private readonly string _glReferenceNumber;

        public string GLReferenceNumber { get { return _glReferenceNumber; } }

        public PayrollDeduction(string id, string referenceNumber, string personId, string receivableType, string termId, DateTime date, decimal amount, string glReferenceNumber)
            : base(id, referenceNumber, personId, receivableType, termId, date, amount)
        {
            _glReferenceNumber = glReferenceNumber;
        }
    }
}
