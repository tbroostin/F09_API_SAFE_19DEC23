// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Deposits on account
    /// </summary>
    public partial class DepositCategory
    {
        /// <summary>
        /// List of <see cref="ActivityRemainingAmountItem">deposits</see> on account
        /// </summary>
        public List<ActivityRemainingAmountItem> Deposits { get; set; }
    }
}