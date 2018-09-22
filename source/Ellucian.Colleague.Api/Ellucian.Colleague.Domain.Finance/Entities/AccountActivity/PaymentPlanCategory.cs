// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public partial class PaymentPlanCategory
    {
        public PaymentPlanCategory()
        {
            PaymentPlans = new List<ActivityPaymentPlanDetailsItem>();
        }


        public List<ActivityPaymentPlanDetailsItem> PaymentPlans { get; set; }
    }
}