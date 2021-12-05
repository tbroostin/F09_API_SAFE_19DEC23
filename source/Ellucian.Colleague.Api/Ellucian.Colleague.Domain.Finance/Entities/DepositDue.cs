// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// An AR Deposit Due entity
    /// </summary>
    [Serializable]
    public class DepositDue
    {
        // Private members
        private readonly string _id;
        private readonly string _personId;
        private readonly decimal _amount;
        private readonly string _depositType;
        private readonly DateTime _dueDate;
        private readonly List<Deposit> _deposits = new List<Deposit>();

        /// <summary>
        /// ID of the deposit due.
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// ID of the person that the deposit due is for.
        /// </summary>
        public string PersonId { get { return _personId; } }

        /// <summary>
        /// Amount of the deposit due.
        /// </summary>
        public decimal Amount { get { return _amount; } }

        /// <summary>
        /// The type of deposit due.
        /// </summary>
        public string DepositType { get { return _depositType; } }

        /// <summary>
        /// The date the deposit is due.
        /// </summary>
        public DateTime DueDate { get { return _dueDate; } }

        /// <summary>
        /// DueDate offset to timezone defined on CTZS
        /// </summary>
        public DateTimeOffset? DueDateOffsetCTZS { get; set; }

        /// <summary>
        /// The term to which a deposit made would apply.
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// The deposits made against a deposit due
        /// </summary>
        public ReadOnlyCollection<Deposit> Deposits { get; private set; }

        /// <summary>
        /// The balance due for a deposit due
        /// </summary>
        public decimal Balance
        {
            get
            {
                decimal balance = Amount;
                foreach (var deposit in Deposits)
                {
                    balance -= deposit.Amount;
                }
                return balance;
            }
        }

        /// <summary>
        /// Indicates whether the deposit due is overdue
        /// </summary>
        public bool Overdue
        {
            get
            {
                return (Balance > 0) ? DueDate < DateTime.Today : false;
            }
        }

        /// <summary>
        /// The total amount paid towards a deposit due
        /// </summary>
        public decimal AmountPaid
        {
            get
            {
                decimal amountPaid = 0;
                foreach (var deposit in Deposits)
                {
                    amountPaid += deposit.Amount;
                }
                return amountPaid;
            }
        }

        /// <summary>
        /// The description of the type of deposit due.
        /// </summary>
        public string DepositTypeDescription { get; set; }

        /// <summary>
        /// The description of the term to which a deposit made would apply.
        /// </summary>
        public string TermDescription { get; set; }

        /// <summary>
        /// Get the sort order for a deposit due:
        /// By due date, then by deposit type, then by ID
        /// </summary>
        public string SortOrder
        {
            get
            {
                return _dueDate.ToString("s") + _depositType.PadRight(5)+  _id.PadLeft(20, '0'); 
            }
        }

        /// <summary>
        /// Constructor for DepositDue
        /// </summary>
        /// <param name="id">ID of deposit due</param>
        /// <param name="personId">ID of person who has the deposit due</param>
        /// <param name="amount">Amount of deposit due</param>
        /// <param name="depositType">Type of deposit that is due</param>
        /// <param name="dueDate">Date on which the deposit is due</param>
        public DepositDue(string id, string personId, decimal amount, string depositType, DateTime dueDate)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            if (amount < 0)
            {
                throw new ArgumentException("Amount cannot be negative");
            }
            
            if (string.IsNullOrEmpty(depositType))
            {
                throw new ArgumentNullException("depositType");
            }

            _id = id;
            _personId = personId;
            _amount = amount;
            _depositType = depositType;
            _dueDate = dueDate;
            Deposits = _deposits.AsReadOnly();
            DepositTypeDescription = string.Empty;
            TermDescription = string.Empty;
        }

        /// <summary>
        /// Add a deposit for this deposit due
        /// </summary>
        /// <param name="deposit">Deposit made on the deposit due</param>
        public void AddDeposit(Deposit deposit)
        {
            if (deposit == null)
            {
                throw new ArgumentNullException("deposit");
            }
            
            if (!_deposits.Contains(deposit))
            {
                _deposits.Add(deposit);
            }
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

            DepositDue other = obj as DepositDue;
            if (other == null)
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
            return Id.GetHashCode();
        }
    }
}
