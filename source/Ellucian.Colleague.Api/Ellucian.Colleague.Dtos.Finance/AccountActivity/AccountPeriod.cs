// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Account information for a term/period
    /// </summary>
    public class AccountPeriod
    {
        /// <summary>
        /// List of terms in the period
        /// </summary>
        public List<string> AssociatedPeriods { get; set; }

        /// <summary>
        /// Balance for the period
        /// </summary>
        public decimal? Balance { get; set; }

        /// <summary>
        /// Period description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Period ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Period start date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Period end date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// AccountPeriod constructor
        /// </summary>
        public AccountPeriod()
        {
            AssociatedPeriods = new List<string>();
        }
    }
}
