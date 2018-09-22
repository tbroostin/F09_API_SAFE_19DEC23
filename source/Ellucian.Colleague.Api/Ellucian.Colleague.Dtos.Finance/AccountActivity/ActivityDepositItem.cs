// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Represents activity on a deposit item.
    /// </summary>
    public class ActivityDepositItem : ActivityDateTermItem
    {
        /// <summary>
        /// Deposit type code - not currently used
        /// </summary>
        public string Type { get; set; }
    }
}
