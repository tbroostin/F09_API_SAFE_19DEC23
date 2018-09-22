// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// An AR deposit entity
    /// </summary>
    [Serializable]
    public class Deposit
    {
        private string _id;
        /// <summary>
        /// ID of the deposit.
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                // if null, set to value.  if not null, cannot set to a different value
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    if (!_id.Equals(value))
                    {
                        throw new InvalidOperationException("Receipt identifier already defined for deposit.");
                    }
                }
            }
        }

        private readonly string _personId;
        /// <summary>
        /// ID of the person the deposit is for.
        /// </summary>
        public string PersonId { get { return _personId; } }

        private readonly DateTime _date;
        /// <summary>
        /// Date the deposit was paid.
        /// </summary>
        public DateTime Date { get { return _date; } }

        private readonly string _depositType;
        /// <summary>
        /// The type of deposit.
        /// </summary>
        public string DepositType { get { return _depositType; } }

        private readonly decimal _amount;
        /// <summary>
        /// The amount of the deposit.
        /// </summary>
        public decimal Amount { get { return _amount; } }

        /// <summary>
        /// The term of the deposit.
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// External System
        /// </summary>
        public string ExternalSystem { get; private set; }
        /// <summary>
        /// External System ID
        /// </summary>
        public string ExternalIdentifier { get; private set; }

        private string _receiptId;
        /// <summary>
        /// Receipt ID
        /// </summary>
        public string ReceiptId
        { 
            get {return _receiptId;}
            set
            {
                // if null, set to value.  if not null, cannot set to a different value
                if (string.IsNullOrEmpty(_receiptId))
                {
                    _receiptId = value;
                }
                else
                {
                    if (!_receiptId.Equals(value))
                    {
                        throw new InvalidOperationException("Receipt identifier already defined for deposit.");
                    }
                }
            }
        }

        /// <summary>
        /// Constructor for a Deposit
        /// </summary>
        /// <param name="id">Deposit ID</param>
        /// <param name="personId">Person for whom the deposit was paid</param>
        /// <param name="date">Date the deposit is paid</param>
        /// <param name="depositType">Type of deposit</param>
        /// <param name="amount">Amount of deposit</param>
        public Deposit(string id, string personId, DateTime date, string depositType, decimal amount)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            if (date == default(DateTime))
            {
                throw new ArgumentNullException("date", "date must have a value");
            }

            if (string.IsNullOrEmpty(depositType))
            {
                throw new ArgumentNullException("depositType");
            }

            if (amount == 0)
            {
                throw new ArgumentException("Amount must not be zero");
            }

            _id = id;
            _personId = personId;
            _date = date;
            _depositType = depositType;
            _amount = amount;
        }

        /// <summary>
        /// Add an external system and external system ID to the Receipt
        /// </summary>
        /// <param name="externalSystem">External system</param>
        /// <param name="externalIdentifier">Identifier of the external financial item linked to this receipt</param>
        public void AddExternalSystemAndId(string externalSystem, string externalIdentifier)
        {
            if (string.IsNullOrEmpty(externalSystem) && string.IsNullOrEmpty(externalIdentifier))
            {
                throw new ArgumentNullException("externalSystem", "External System and ID must both be specified at the same time.");
            }
            if (string.IsNullOrEmpty(externalSystem) && !string.IsNullOrEmpty(externalIdentifier))
            {
                throw new ArgumentNullException("externalSystem", "External System cannot be null when External System ID is specified");
            }
            if (string.IsNullOrEmpty(externalIdentifier) && !string.IsNullOrEmpty(externalSystem))
            {
                throw new ArgumentNullException("externalIdentifier", "External Identifier cannot be null when External System is specified");
            }
            ExternalIdentifier = externalIdentifier;
            ExternalSystem = externalSystem;
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
            if (string.IsNullOrEmpty(_id))
            {
                return false;
            }

            if (obj == null)
            {
                return false;
            }

            var other = obj as Deposit;
            if (other == null || string.IsNullOrEmpty(other.Id))
            {
                return false;
            }

            return other.Id.Equals(_id);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(Id) ? base.GetHashCode() : Id.GetHashCode();
        }
    }
}
