// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountDue
{
    [Serializable]
    public class PaymentPlanDueItem : AccountsReceivableDueItem
    {
        public string PaymentPlanId { get; set; }

        public bool PaymentPlanCurrent { get; set; }

        public decimal? UnpaidAmount { get; set; }
    }
}
