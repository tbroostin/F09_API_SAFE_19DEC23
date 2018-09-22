// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountDue
{
    /// <summary>
    /// Account due information broken down by term
    /// </summary>
    public class AccountDue
    {
        /// <summary>
        /// AccountDue constructor
        /// </summary>
        public AccountDue()
        {
            AccountTerms = new List<AccountTerm>();
        }

        /// <summary>
        /// A list of <see cref="AccountTerm">AccountTerm</see> items
        /// </summary>
        public List<AccountTerm> AccountTerms { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Account holder name
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Balance due
        /// </summary>
        public decimal Balance { get; set; }
    }
}
