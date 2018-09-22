// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A payment allocated to a payable item
    /// </summary>
    [Serializable]
    public class PaymentAllocation
    {
        private readonly string _id;
        /// <summary>
        /// ID of allocation
        /// </summary>
        public string Id { get { return _id; } }

        private readonly string _paymentId;
        /// <summary>
        /// ID of payment for which allocation is made
        /// </summary>
        public string PaymentId { get { return _paymentId; } }

        private readonly PaymentAllocationSource _source;
        /// <summary>
        /// Source of allocation
        /// </summary>
        public PaymentAllocationSource Source { get { return _source; } }

        private readonly decimal _amount;
        /// <summary>
        /// Allocation amount
        /// </summary>
        public decimal Amount { get { return _amount; } }

        /// <summary>
        /// ID of charge to which allocation is made
        /// </summary>
        public string ChargeId { get; set; }

        /// <summary>
        /// Indicator for whether allocation is invoice-specific
        /// </summary>
        public bool IsInvoiceAllocated { get; set; }

        /// <summary>
        /// ID of scheduled payment associated with allocation
        /// </summary>
        public string ScheduledPaymentId { get; set; }

        /// <summary>
        /// Amount that is unallocated
        /// </summary>
        public decimal UnallocatedAmount 
        { 
            get
            {
                return (String.IsNullOrEmpty(ChargeId)) ? Amount : 0;
            }
        }

        /// <summary>
        /// Allocation constructor
        /// </summary>
        /// <param name="id">Allocation ID</param>
        /// <param name="paymentId">Payment ID</param>
        /// <param name="source">Allocation source</param>
        /// <param name="amount">Allocation amount</param>
        public PaymentAllocation(string id, string paymentId, PaymentAllocationSource? source, decimal? amount)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be null");
            }

            if (string.IsNullOrEmpty(paymentId))
            {
                throw new ArgumentNullException("paymentId", "Payment ID cannot be null");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source", "Allocation Source cannot be null");
            }

            if (amount == null)
            {
                throw new ArgumentNullException("amount", "Amount cannot be null");
            }

            _id = id;
            _paymentId = paymentId;
            _source = source.Value;
            _amount = amount.Value;
        }
        /// <summary>
        /// Override Equals
        /// </summary>
        /// <param name="obj">Receivable payment object to be compared</param>
        /// <returns>True or False if objects are considered equal</returns>
        public override bool Equals(object obj)
        {
            if (string.IsNullOrEmpty(_id))
            {
                return false;
            }
            if (obj == null)
            {
                return false;
            }

            var other = obj as PaymentAllocation;
            if (other == null || string.IsNullOrEmpty(other.Id))
            {
                return false;
            }

            return other.Id.Equals(_id);
        }
        /// <summary>
        /// Override Get HashCode
        /// </summary>
        /// <returns>HashCode integer</returns>
        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(Id) ? base.GetHashCode() : Id.GetHashCode();
        }
    }
}
