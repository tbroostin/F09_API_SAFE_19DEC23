// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class ReceivablePayment : ReceivableTransaction
    {
        private readonly decimal _amount;
        public decimal Amount { get { return _amount; } }

        private readonly List<PaymentAllocation> _allocations = new List<PaymentAllocation>();
        public ReadOnlyCollection<PaymentAllocation> Allocations { get ; private set;  }

        public ReceivablePayment(string id, string referenceNumber, string personId, string receivableType, string termId, DateTime date, decimal amount)
            : base(id, referenceNumber, personId, receivableType, termId, date)
        {
            _amount = amount;
            Allocations = _allocations.AsReadOnly();
        }

        public void AddAllocation(PaymentAllocation allocation)
        {
            if (allocation == null)
            {
                throw new ArgumentNullException("allocation");
            }

            // Prevent duplicates
            if (!_allocations.Contains(allocation))
            {
                _allocations.Add(allocation);
            }
        }
    }
}
