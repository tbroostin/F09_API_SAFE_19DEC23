// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Account activity information for a period
    /// </summary>
    public class DetailedAccountPeriod : AccountPeriod
    {
        /// <summary>
        /// Total amount due
        /// </summary>
        public decimal AmountDue { get; set; }

        /// <summary>
        /// Period charges
        /// </summary>
        public ChargesCategory Charges { get; set; }

        /// <summary>
        /// Period deposits
        /// </summary>
        public DepositCategory Deposits { get; set; }

        /// <summary>
        /// Period due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Period FA awards
        /// </summary>
        public FinancialAidCategory FinancialAid { get; set; }

        /// <summary>
        /// Period payment plans
        /// </summary>
        public PaymentPlanCategory PaymentPlans { get; set; }

        /// <summary>
        /// Refunds in period
        /// </summary>
        public RefundCategory Refunds { get; set; }

        /// <summary>
        /// Period sponsorships payments
        /// </summary>
        public SponsorshipCategory Sponsorships { get; set; }

        /// <summary>
        /// Period payments
        /// </summary>
        public StudentPaymentCategory StudentPayments { get; set; }

    }
}
