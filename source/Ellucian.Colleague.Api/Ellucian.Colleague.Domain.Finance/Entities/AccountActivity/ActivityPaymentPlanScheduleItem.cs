// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public class ActivityPaymentPlanScheduleItem : ActivityDateTermItem
    {
        public decimal? SetupCharge { get; set; }

        public decimal? LateCharge { get; set; }

        public decimal? AmountPaid { get; set; }

        public decimal? NetAmountDue { get; set; }

        public DateTime? DatePaid { get; set; }
    }
}
