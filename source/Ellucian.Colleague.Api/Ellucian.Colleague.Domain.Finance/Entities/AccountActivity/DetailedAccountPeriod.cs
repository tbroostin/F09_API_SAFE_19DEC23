// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public class DetailedAccountPeriod : AccountPeriod
    {
        public decimal AmountDue { get; set; }

        public ChargesCategory Charges { get; set; }

        public DepositCategory Deposits { get; set; }

        public DateTime? DueDate { get; set; }

        public FinancialAidCategory FinancialAid { get; set; }

        public PaymentPlanCategory PaymentPlans { get; set; }

        public PaymentCategory Payments { get; set; }

        public RefundCategory Refunds { get; set; }

        public SponsorshipCategory Sponsorships { get; set; }

        public StudentPaymentCategory StudentPayments { get; set; }
    }
}
