// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountDue
{
    /// <summary>
    /// A plan payment that is due
    /// </summary>
    public class PaymentPlanDueItem : AccountsReceivableDueItem
    {
        /// <summary>
        /// Payment plan ID
        /// </summary>
        public string PaymentPlanId { get; set; }

        /// <summary>
        /// Indicates whether the plan payment is currently due
        /// </summary>
        public bool PaymentPlanCurrent { get; set; }

        /// <summary>
        /// Amount due on the plan payment
        /// </summary>
        public decimal? UnpaidAmount { get; set; }
    }
}
