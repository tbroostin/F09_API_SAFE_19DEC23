// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentPlanEligibilityTests
    {
        private List<BillingTermPaymentPlanInformation> eligibleItems;
        private PaymentPlanIneligibilityReason? reason;
        private PaymentPlanEligibility entity;

        [TestInitialize]
        public void Initialize()
        {
            eligibleItems = new List<BillingTermPaymentPlanInformation>()
            {
                new BillingTermPaymentPlanInformation("0001234", "2017/SP", "01", 500m, "DEFAULT"),
                new BillingTermPaymentPlanInformation("0001234", "2018/FA", "02", 5000m, "NONDEFAULT")
            };
            reason = PaymentPlanIneligibilityReason.ChargesAreNotEligible;
        }

        [TestCleanup]
        public void Cleanup()
        {
            eligibleItems = null;
            reason = null;
            entity = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void PaymentPlanEligibility_Constructor_PaymentPlanEligibleItems_Null_PaymentPlanIneligibilityReason_Null()
        {
            entity = new PaymentPlanEligibility(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void PaymentPlanEligibility_Constructor_PaymentPlanEligibleItems_Empty_PaymentPlanIneligibilityReason_Null()
        {
            entity = new PaymentPlanEligibility(new List<BillingTermPaymentPlanInformation>(), null);
        }

        [TestMethod]
        public void PaymentPlanEligibility_Constructor_PaymentPlanEligibleItems_NotNullOrEmpty_PaymentPlanIneligibilityReason_NotNull()
        {
            entity = new PaymentPlanEligibility(eligibleItems, reason);
            Assert.AreEqual(eligibleItems.Count, entity.EligibleItems.Count);
            Assert.IsNull(entity.IneligibilityReason);
        }

        [TestMethod]
        public void PaymentPlanEligibility_Constructor_PaymentPlanEligibleItems_Null_PaymentPlanIneligibilityReason_NotNull()
        {
            entity = new PaymentPlanEligibility(null, reason);
            Assert.AreEqual(0, entity.EligibleItems.Count);
            Assert.AreEqual(reason, entity.IneligibilityReason);
        }

        [TestMethod]
        public void PaymentPlanEligibility_Constructor_PaymentPlanEligibleItems_Empty_PaymentPlanIneligibilityReason_NotNull()
        {
            entity = new PaymentPlanEligibility(new List<BillingTermPaymentPlanInformation>(), reason);
            Assert.AreEqual(0, entity.EligibleItems.Count);
            Assert.AreEqual(reason, entity.IneligibilityReason);
        }

        [TestMethod]
        public void PaymentPlanEligibility_Constructor_PaymentPlanEligibleItems_NotNullOrEmpty_PaymentPlanIneligibilityReason_Null()
        {
            entity = new PaymentPlanEligibility(eligibleItems, null);
            Assert.AreEqual(eligibleItems.Count, entity.EligibleItems.Count);
            Assert.IsNull(entity.IneligibilityReason);
        }
    }
}
