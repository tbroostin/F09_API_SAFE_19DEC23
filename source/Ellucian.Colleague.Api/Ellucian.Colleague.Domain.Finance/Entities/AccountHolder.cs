// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// AR Accountholder entity
    /// </summary>
    [Serializable]
    public class AccountHolder : Person
    {
        // Private members
        private readonly List<DepositDue> _depositsDue = new List<DepositDue>();

        /// <summary>
        /// Deposits due for this accountholder
        /// </summary>
        public ReadOnlyCollection<DepositDue> DepositsDue { get; private set; }

        /// <summary>
        /// Constructor for AccountHolder entity
        /// </summary>
        /// <param name="id">Person ID of the accountholder</param>
        /// <param name="lastName">Last name of the accountholder</param>
        /// <param name="privacyStatusCode">Privacy status code</param>
        public AccountHolder(string id, string lastName, string privacyStatusCode = null)
            : base(id, lastName, privacyStatusCode)
        {
            DepositsDue = _depositsDue.AsReadOnly();
        }

        /// <summary>
        /// Add one or more deposits due for this accountholder
        /// </summary>
        /// <param name="depositsDue">List of DepositDue entities</param>
        public void AddDepositsDue(IEnumerable<DepositDue> depositsDue)
        {
            if (depositsDue != null && depositsDue.Count() > 0)
            {
                _depositsDue.AddRange(depositsDue);
            }
        }

        public void AddDepositDue(DepositDue depositDue)
        {
            if (depositDue == null)
            {
                throw new ArgumentNullException("depositDue");
            }
            
            // No duplicates
            if (!_depositsDue.Contains(depositDue))
            {
                _depositsDue.Add(depositDue);
            }
        }
    }
}
