// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A transaction with a date
    /// </summary>
    public class ActivityDateTermItem : ActivityTermItem
    {
        /// <summary>
        /// Transaction date
        /// </summary>
        public DateTime? Date { get; set; }
    }
}
