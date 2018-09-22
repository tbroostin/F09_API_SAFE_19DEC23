// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    /// <summary>
    /// Information about a payment plan
    /// </summary>
    [Serializable]
    public class ActivityPaymentPlanDetailsItem : ActivityTermItem
    {
        public ActivityPaymentPlanDetailsItem()
        {
            Type = string.Empty;
            PaymentPlanSchedules = new List<ActivityPaymentPlanScheduleItem>();
        }

        /// <summary>
        /// The Receivable Type of the plan
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The original amount of the plan
        /// </summary>
        public decimal? OriginalAmount { get; set; }

        /// <summary>
        /// The amount still owed on the plan
        /// </summary>
        public decimal? CurrentBalance { get; set; }

        /// <summary>
        /// Payment Plan Approval record
        /// </summary>
        public string PaymentPlanApproval { get; set; }

        /// <summary>
        /// The due dates and due amounts of the payment plan
        /// </summary>
        public List<ActivityPaymentPlanScheduleItem> PaymentPlanSchedules { get; set; }
    }
}
