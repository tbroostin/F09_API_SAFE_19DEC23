// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A payment to be paid on a certain date as part of a payment plan's schedule of payments
    /// </summary>
    [Serializable]
    public class ScheduledPayment
    {
        private string _id;
        private string _planId;
        private readonly decimal _amount;
        private readonly DateTime _dueDate;
        private readonly decimal _amountPaid;
        private readonly DateTime? _lastPaidDate;

        /// <summary>
        /// ID of the scheduled payment
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    throw new InvalidOperationException("ID already defined for scheduled payment.");
                }
                _id = value;
            }
        }

        /// <summary>
        /// ID of the payment plan to which the scheduled payment belongs
        /// </summary>
        public string PlanId
        {
            get { return _planId; }
            set
            {
                if (!string.IsNullOrEmpty(_planId))
                {
                    throw new InvalidOperationException("Plan ID already defined for scheduled payment.");
                }
                _planId = value;
            }
        }

        /// <summary>
        /// Amount to be paid
        /// </summary>
        public decimal Amount { get { return _amount; } }

        /// <summary>
        /// Date on which the scheduled payment is due to be paid
        /// </summary>
        public DateTime DueDate { get { return _dueDate; } }

        /// <summary>
        /// Amount paid against the scheduled payment
        /// </summary>
        public decimal AmountPaid { get { return _amountPaid; } }

        /// <summary>
        /// Date on which last payment was made against scheduled payment
        /// </summary>
        public DateTime? LastPaidDate { get { return _lastPaidDate; } }

        /// <summary>
        /// Determines whether the scheduled payment is overdue
        /// </summary>
        public bool IsPastDue { get {return DueDate.Date < DateTime.Today.Date; } }

        /// <summary>
        /// Constructor for the scheduled payment entity
        /// </summary>
        /// <param name="id">ID of the scheduled payment</param>
        /// <param name="planId">ID of the payment plan to which the scheduled payment belongs</param>
        /// <param name="amount">Amount to be paid for the scheduled payment</param>
        /// <param name="dueDate">Date on which the scheduled payment is due to be paid</param>
        /// <param name="amountPaid">Amount paid against the scheduled payment</param>
        public ScheduledPayment(string id, string planId, decimal amount, DateTime dueDate, decimal amountPaid, DateTime? lastPaidDate)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Scheduled payment must be greater than zero.");
            }

            if (!string.IsNullOrEmpty(id) && string.IsNullOrEmpty(planId))
            {
                throw new ArgumentNullException("planId", "Plan ID cannot be null/empty if ID is supplied.");
            }

            if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(planId))
            {
                throw new ArgumentNullException("id", "ID cannot be null/empty if plan ID is supplied.");
            }

            if (amountPaid < 0)
            {
                throw new ArgumentOutOfRangeException("amountPaid", "Amount paid cannot be negative.");
            }

            _id = id;
            _planId = planId;
            _dueDate = dueDate;
            _amount = amount;
            _amountPaid = amountPaid;
            _lastPaidDate = lastPaidDate;
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
            ScheduledPayment other = obj as ScheduledPayment;
            if (other == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(Id))
            {
                return false;
            }

            return (other.Id.Equals(Id));
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return (Id == null) ? base.GetHashCode() : Id.GetHashCode();
        }
    }
}
