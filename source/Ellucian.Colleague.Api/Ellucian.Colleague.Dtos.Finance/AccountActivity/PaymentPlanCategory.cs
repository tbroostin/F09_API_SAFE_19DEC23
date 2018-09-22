// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Payment plans
    /// </summary>
    public partial class PaymentPlanCategory
    {
        /// <summary>
        /// PaymentPlanCategory constructor
        /// </summary>
        public PaymentPlanCategory()
        {
            PaymentPlans = new List<ActivityPaymentPlanDetailsItem>();
        }

        /// <summary>
        /// List of <see cref="ActivityPaymentPlanDetailsItem">payment plans</see>
        /// </summary>
        public List<ActivityPaymentPlanDetailsItem> PaymentPlans { get; set; }
    }
}