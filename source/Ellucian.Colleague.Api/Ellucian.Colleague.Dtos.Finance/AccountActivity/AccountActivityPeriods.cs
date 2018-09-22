// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Account activity
    /// </summary>
    public class AccountActivityPeriods
    {
        /// <summary>
        /// Non-term account activity
        /// </summary>
        public AccountPeriod NonTermActivity { get; set; }

        /// <summary>
        /// List of term-based account activity
        /// </summary>
        public IList<AccountPeriod> Periods { get; set; }

        /// <summary>
        /// AccountActivityPeriods constructor
        /// </summary>
        public AccountActivityPeriods()
        {
            Periods = new List<AccountPeriod>();
        }
    }
}
