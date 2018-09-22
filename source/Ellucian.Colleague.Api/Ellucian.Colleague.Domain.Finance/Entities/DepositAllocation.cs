// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class DepositAllocation : ReceivablePayment
    {
        private readonly string _depositId;

        public string DepositId { get { return _depositId; } }

        public DepositAllocation(string id, string referenceNumber, string personId, string receivableType, string termId, DateTime date, decimal amount, string depositId)
            : base(id, referenceNumber, personId, receivableType, termId, date, amount)
        {
            _depositId = depositId;
        }
    }
}
