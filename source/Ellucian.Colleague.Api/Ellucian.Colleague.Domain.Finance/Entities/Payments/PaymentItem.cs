// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class PaymentItem
    {
        public decimal PaymentAmount { get; set; }

        public string Description { get; set; }

        public string AccountType { get; set; }

        public string Term { get; set; }

        public string InvoiceId { get; set; }

        public string PaymentPlanId { get; set; }

        public string DepositDueId { get; set; }

        public bool Overdue { get; set; }

        public string PaymentControlId { get; set; }

        public bool PaymentComplete { get; set; }
    }
}
