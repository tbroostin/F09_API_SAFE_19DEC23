// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A charge on a receivable account
    /// </summary>
    [Serializable]
    public class Charge
    {
        private readonly string _id;
        private readonly string _invoiceId;
        private readonly List<string> _description;
        private readonly string _code;
        private readonly decimal _baseAmount;
        private readonly List<string> _allocationIds = new List<String>();
        private readonly List<string> _paymentPlanIds = new List<String>();

        /// <summary>
        /// Record ID of the charge on the account
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// ID of the invoice this charge is on
        /// </summary>
        public string InvoiceId { get { return _invoiceId; } }

        /// <summary>
        /// Description of the charge
        /// </summary>
        public ReadOnlyCollection<string> Description { get; private set; }

        /// <summary>
        /// Charge code
        /// </summary>
        public string Code { get { return _code; } }

        /// <summary>
        /// Base amount of the charge (without taxes)
        /// </summary>
        public decimal BaseAmount { get { return _baseAmount; } }

        /// <summary>
        /// List of IDs to the allocations toward this charge
        /// </summary>
        public ReadOnlyCollection<string> AllocationIds { get; private set; }

        /// <summary>
        /// List of payment plans this charge is on
        /// </summary>
        public ReadOnlyCollection<string> PaymentPlanIds { get; private set; }

        /// <summary>
        /// Amount of tax on this charge
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total amount of the charge (including taxes)
        /// </summary>
        public decimal Amount { get { return (BaseAmount + TaxAmount); } }

        /// <summary>
        /// Charge constructor
        /// </summary>
        /// <param name="id">Charge ID</param>
        /// <param name="invoiceId">ID of the invoice the charge is on</param>
        /// <param name="description">Charge description</param>
        /// <param name="code">Charge code</param>
        /// <param name="amount">Base amount of the charge</param>
        public Charge(string id, string invoiceId, IEnumerable<string> description, string code, decimal amount)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Invoice Item ID cannot be null");
            }
            if (string.IsNullOrEmpty(invoiceId))
            {
                throw new ArgumentNullException("invoiceId", "Invoice ID cannot be null");
            }
            if (description == null || description.Count() == 0)
            {
                throw new ArgumentNullException("description", "Description cannot be null");
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "AR Code cannot be null");
            }

            _id = id;
            _invoiceId = invoiceId;
            _description = description.ToList();
            _code = code;
            _baseAmount = amount;

            Description = _description.AsReadOnly();
            AllocationIds = _allocationIds.AsReadOnly();
            PaymentPlanIds = _paymentPlanIds.AsReadOnly();
        }

        /// <summary>
        /// Add an allocation ID to this charge
        /// </summary>
        /// <param name="id">ID of the allocation</param>
        public void AddAllocation(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Allocation ID cannot be null");
            }
            _allocationIds.Add(id);
        }

        /// <summary>
        /// Link this charge to a payment plan
        /// </summary>
        /// <param name="id">Payment plan ID</param>
        public void AddPaymentPlan(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Payment Plan ID cannot be null");
            }
            _paymentPlanIds.Add(id);
        }

        /// <summary>
        /// Remove an allocation to this charge
        /// </summary>
        /// <param name="id">ID of the allocation to remove</param>
        public void RemoveAllocation(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Allocation ID cannot be null");
            }
            _allocationIds.Remove(id);
        }

        /// <summary>
        /// Remove the link to a payment plan
        /// </summary>
        /// <param name="id">Payment plan ID</param>
        public void RemovePaymentPlan(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Payment Plan ID cannot be null");
            }
            _paymentPlanIds.Remove(id);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Charge other = obj as Charge;
            if (other == null)
            {
                return false;
            }

            return other.Id.Equals(Id);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
