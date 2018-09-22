// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// An item being paid
    /// </summary>
    public class PaymentItem
    {
        /// <summary>
        /// Amount owed
        /// </summary>
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// Description of item
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// AR type of item
        /// </summary>
        public string AccountType { get; set; }

        /// <summary>
        /// Term of item
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// Invoice ID to be paid
        /// </summary>
        public string InvoiceId { get; set; }

        /// <summary>
        /// Payment plan to be paid
        /// </summary>
        public string PaymentPlanId { get; set; }

        /// <summary>
        /// Deposit due to be paid
        /// </summary>
        public string DepositDueId { get; set; }

        /// <summary>
        /// Identifies whether amount owed is overdue
        /// </summary>
        public bool Overdue { get; set; }

        /// <summary>
        /// ID of the associated payment control record
        /// </summary>
        public string PaymentControlId { get; set; }

        /// <summary>
        /// Indicator whether payment being made satisfies IPC requirements
        /// </summary>
        public bool PaymentComplete { get; set; }
    }
}
