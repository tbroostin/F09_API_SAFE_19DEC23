// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class StudentStatementSummaryTests
    {
        static List<ActivityTermItem> chargeInformation = new List<ActivityTermItem>()
        {
            new ActivityTermItem() { Description = "Tuition by Total", Amount = 100m },
            new ActivityTermItem() { Description = "Tuition by Section", Amount = 150m},
            new ActivityTermItem() { Description = "Room and Board", Amount = 200m},
            new ActivityTermItem() { Description = "Fees", Amount = 250m},
            new ActivityTermItem() { Description = "Miscellaneous", Amount = 300m}
        };
        static List<ActivityTermItem> nonChargeInformation = new List<ActivityTermItem>()
        {
            new ActivityTermItem() { Description = "Student Payments", Amount = 100m},
            new ActivityTermItem() { Description = "Financial Aid", Amount = 150m},
            new ActivityTermItem() { Description = "Sponsor Billing", Amount = 200m},
            new ActivityTermItem() { Description = "Deposits", Amount = 250m},
            new ActivityTermItem() { Description = "Refunds", Amount = 300m}
        };
        StudentStatementSummary accountSummary;

        [TestInitialize]
        public void Initialize()
        {
            accountSummary = new StudentStatementSummary(chargeInformation, nonChargeInformation, 500m, 600m);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentStatementSummary_Constructor_NullChargeInformation()
        {
            accountSummary = new StudentStatementSummary(null, nonChargeInformation, 500m, 600m);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentStatementSummary_Constructor_NullNonChargeInformation()
        {
            accountSummary = new StudentStatementSummary(chargeInformation, null, 500m, 600m);
        }

        [TestMethod]
        public void StudentStatementSummary_Constructor_Valid()
        {
            var expectedList = new List<ActivityTermItem>(chargeInformation);
            var actualList = new List<ActivityTermItem>(accountSummary.ChargeInformation).ToList();
            Assert.AreEqual(expectedList.Count, actualList.Count);
            for (int i = 0; i < expectedList.Count; i++)
            {
                Assert.AreEqual(expectedList[i].Description, actualList[i].Description);
                Assert.AreEqual(expectedList[i].Amount, actualList[i].Amount);
            }
            expectedList = new List<ActivityTermItem>(nonChargeInformation).ToList();
            actualList = new List<ActivityTermItem>(accountSummary.NonChargeInformation).ToList();
            Assert.AreEqual(expectedList.Count, actualList.Count);
            for (int i = 0; i < expectedList.Count; i++)
            {
                Assert.AreEqual(expectedList[i].Description, actualList[i].Description);
                Assert.AreEqual(expectedList[i].Amount, actualList[i].Amount);
            }
            Assert.AreEqual(500m, accountSummary.PaymentPlanAdjustmentsAmount);
            Assert.AreEqual(600m, accountSummary.CurrentDepositsDueAmount);
        }
    }
}
