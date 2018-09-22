// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountDue
{
    /// <summary>
    /// Account due information broken down by period
    /// </summary>
    public class AccountDuePeriod
    {
        /// <summary>
        /// Past period due items
        /// </summary>
        public AccountDue Past { get; set; }

        /// <summary>
        /// Current period due items
        /// </summary>
        public AccountDue Current { get; set; }

        /// <summary>
        /// Future period due items
        /// </summary>
        public AccountDue Future { get; set; }

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
