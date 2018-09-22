// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A payment plan summary
    /// </summary>
    public class ActivityPaymentPlanDetailsItem : ActivityTermItem
    {
        /// <summary>
        /// ActivityPaymentPlanDetailsItem constructor
        /// </summary>
        public ActivityPaymentPlanDetailsItem()
        {
            Type = string.Empty;
            PaymentPlanSchedules = new List<ActivityPaymentPlanScheduleItem>();
        }

        /// <summary>
        /// Plan type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Original plan amount
        /// </summary>
        public decimal? OriginalAmount { get; set; }

        /// <summary>
        /// Current plan balance
        /// </summary>
        public decimal? CurrentBalance { get; set; }

        /// <summary>
        /// Payment Plan Approval record
        /// </summary>
        public string PaymentPlanApproval { get; set; }

        /// <summary>
        /// List of <see cref="ActivityPaymentPlanScheduleItem">scheduled payments</see> on a plan
        /// </summary>
        public List<ActivityPaymentPlanScheduleItem> PaymentPlanSchedules { get; set; }
    }
}
