// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ActivityPaymentPlanDetailsItemTests
    {
        [TestMethod]
        public void ActivityPaymentPlanDetailsItem_PaymentPlanSchedules()
        {
            var ap = new ActivityPaymentPlanDetailsItem();
            CollectionAssert.AreEqual(new List<ActivityPaymentPlanScheduleItem>(), ap.PaymentPlanSchedules);
        }
    }
}
