// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance.AccountDue
{
    /// <summary>
    /// An item due on an AR account
    /// </summary>
    public class AccountsReceivableDueItem
    {
        /// <summary>
        /// Amount due
        /// </summary>
        public decimal? AmountDue { get; set; }

        /// <summary>
        /// Date due
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Date due offset to CTZS timezone setting
        /// </summary>
        public DateTimeOffset? DueDateOffset { get; set; }

        /// <summary>
        /// Item description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether the item is overdue
        /// </summary>
        public bool Overdue { get; set; }

        /// <summary>
        /// Code of term for which item is due
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// Term description
        /// </summary>
        public string TermDescription { get; set; }

        /// <summary>
        /// Code of period for which item is due
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Period description
        /// </summary>
        public string PeriodDescription { get; set; }

        /// <summary>
        /// AR type code
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// AR type description
        /// </summary>
        public string AccountDescription { get; set; }

        /// <summary>
        /// Distribution against which the item will be paid
        /// </summary>
        public string Distribution { get; set; }
    }
}
