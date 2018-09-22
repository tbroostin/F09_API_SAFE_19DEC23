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
    public class PaymentPlanCategoryTests
    {
        [TestMethod]
        public void PaymentPlanCategory_PaymentPlans()
        {
            var ppc = new PaymentPlanCategory();
            CollectionAssert.AreEqual(new List<ActivityPaymentPlanDetailsItem>(), ppc.PaymentPlans);
        }
    }
}
