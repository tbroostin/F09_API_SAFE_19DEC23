// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Input to determine account activity for a specific period
    /// </summary>
    public class AccountActivityPeriodArguments
    {
        /// <summary>
        /// List of terms in desired period
        /// </summary>
        public IEnumerable<string> AssociatedPeriods { get; set; }

        /// <summary>
        /// Start date of desired period
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date of desired period
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
