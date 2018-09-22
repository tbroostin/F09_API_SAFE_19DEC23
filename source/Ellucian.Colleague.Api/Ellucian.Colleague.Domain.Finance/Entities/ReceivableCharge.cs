// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A charge against a receivable account
    /// </summary>
    [Serializable]
    public class ReceivableCharge
    {
        private string _id;
        /// <summary>
        /// ID of charge
        /// </summary>
        public string Id { 
            get 
            { 
                return _id; 
            }
            set
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    throw new InvalidOperationException("Id already defined for receivable charge.");
                }
                _id = value;
            }
        }
        
        private string _invoiceId;
        /// <summary>
        /// Invoice ID
        /// </summary>
        public string InvoiceId { 
            get { return _invoiceId; }
            set
            {
                if (!string.IsNullOrEmpty(_invoiceId))
                {
                    throw new InvalidOperationException("InvoiceId already defined for receivable charge.");
                }
                _invoiceId = value;
            }
        }
        
        private readonly List<string> _description = new List<string>();
        /// <summary>
        /// Charge description
        /// </summary>
        public ReadOnlyCollection<string> Description { get; private set; }
        
        private readonly string _code;
        /// <summary>
        /// Charge code
        /// </summary>
        public string Code { get { return _code; } }
        
        private readonly List<string> _allocationIds = new List<string>();
        /// <summary>
        /// Collection of IDs to the Allocation objects that pay on this charge
        /// </summary>
        public ReadOnlyCollection<string> AllocationIds { get; private set; }
        
        private readonly List<string> _paymentPlanIds = new List<string>();
        /// <summary>
        /// Collection of IDs to the payment plans containing this charge
        /// </summary>
        public ReadOnlyCollection<string> PaymentPlanIds { get; private set; }

        private readonly decimal _baseAmount;
        /// <summary>
        /// Base amount of charge (pre-tax amount)
        /// </summary>
        public decimal BaseAmount { get { return _baseAmount; } }
        
        /// <summary>
        /// Amount of tax on this charge
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total amount of the charge, including tax
        /// </summary>
        public decimal Amount { get { return (BaseAmount + TaxAmount); } }

        /// <summary>
        /// Constructor for ReceivableCharge
        /// </summary>
        /// <param name="id">Charge ID</param>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="description">Charge description</param>
        /// <param name="code">Charge code</param>
        /// <param name="amount">Amount of the charge</param>
        public ReceivableCharge(string id, string invoiceId, IEnumerable<string> description, string code, decimal amount)
        {
            if (description == null || !description.Any())
            {
                throw new ArgumentNullException("description", "Description cannot be null");
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "AR Code cannot be null");
            }

            _id = id;
            _invoiceId = invoiceId;
            _code = code;
            _baseAmount = amount;
            _description = description.ToList();

            AllocationIds = _allocationIds.AsReadOnly();
            PaymentPlanIds = _paymentPlanIds.AsReadOnly();
            Description = _description.AsReadOnly();
        }

        /// <summary>
        /// Add an allocation to this charge
        /// </summary>
        /// <param name="allocationId">Allocation ID</param>
        public void AddAllocation(string allocationId)
        {
            if (string.IsNullOrEmpty(allocationId))
            {
                throw new ArgumentNullException("allocationId", "Allocation ID cannot be null");
            }
            if (!_allocationIds.Contains(allocationId))
            {
                _allocationIds.Add(allocationId);
            }
        }

        /// <summary>
        /// Link this charge to a payment plan
        /// </summary>
        /// <param name="paymentPlanId">Payment Plan ID</param>
        public void AddPaymentPlan(string paymentPlanId)
        {
            if (string.IsNullOrEmpty(paymentPlanId))
            {
                throw new ArgumentNullException("paymentPlanId", "Payment Plan ID cannot be null");
            }
            if (!_paymentPlanIds.Contains(paymentPlanId))
            {
                _paymentPlanIds.Add(paymentPlanId);
            }
        }

        /// <summary>
        /// Remove an allocation from this charge
        /// </summary>
        /// <param name="allocationId">Allocation ID</param>
        public void RemoveAllocation(string allocationId)
        {
            if (string.IsNullOrEmpty(allocationId))
            {
                throw new ArgumentNullException("allocationId", "Allocation ID cannot be null");
            }
            _allocationIds.Remove(allocationId);
        }

        /// <summary>
        /// Remove the payment plan linked to this charge
        /// </summary>
        /// <param name="paymentPlanId">Payment plan ID</param>
        public void RemovePaymentPlan(string paymentPlanId)
        {
            if (string.IsNullOrEmpty(paymentPlanId))
            {
                throw new ArgumentNullException("paymentPlanId", "Payment Plan ID cannot be null");
            }
            _paymentPlanIds.Remove(paymentPlanId);
        }

        /// <summary>
        /// Override Equals
        /// </summary>
        /// <param name="obj">Receivable charge object to be compared</param>
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
            var other = obj as ReceivableCharge;
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
