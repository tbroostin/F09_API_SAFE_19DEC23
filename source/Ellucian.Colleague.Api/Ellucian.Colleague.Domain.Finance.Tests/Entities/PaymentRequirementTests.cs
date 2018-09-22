// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentRequirementTests
    {
        string id = "123";
        string termId = "2013/SP";
        string ruleId = "SOMERULE";
        int processingOrder = 5;
        List<PaymentDeferralOption> deferrals = new List<PaymentDeferralOption>() { new PaymentDeferralOption(DateTime.Today, DateTime.Today.AddDays(60), 50) };
        List<PaymentPlanOption> plans = new List<PaymentPlanOption>() { new PaymentPlanOption(DateTime.Today, DateTime.Today.AddDays(90), "DEFAULT", DateTime.Today.AddDays(100)) };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentRequirement_Constructor_NullId()
        {
            var result = new PaymentRequirement(null, termId, ruleId, processingOrder, deferrals, plans);
        }

        public void PaymentRequirement_Constructor_EmptyId()
        {
            var result = new PaymentRequirement(String.Empty, termId, ruleId, processingOrder, deferrals, plans);
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_ValidId()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, deferrals, plans);

            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentRequirement_Constructor_NullTermId()
        {
            var result = new PaymentRequirement(id, null, ruleId, processingOrder, deferrals, plans);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentRequirement_Constructor_EmptyTermId()
        {
            var result = new PaymentRequirement(id, String.Empty, ruleId, processingOrder, deferrals, plans);
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_ValidTermId()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, deferrals, plans);
            Assert.AreEqual(termId, result.TermId);
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_NullRuleId()
        {
            var result = new PaymentRequirement(id, termId, null, processingOrder, deferrals, plans);
            Assert.AreEqual(null, result.EligibilityRuleId);
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_EmptyRuleId()
        {
            var result = new PaymentRequirement(id, termId, String.Empty, processingOrder, deferrals, plans);
            Assert.AreEqual(String.Empty, result.EligibilityRuleId);
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_ValidRuleId()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, deferrals, plans);
            Assert.AreEqual(ruleId, result.EligibilityRuleId);
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_ValidProcessingOrder()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, deferrals, plans);
            Assert.AreEqual(processingOrder, result.ProcessingOrder);
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_ZeroProcessingOrder()
        {
            var result = new PaymentRequirement(id, termId, ruleId, 0, deferrals, plans);
            Assert.AreEqual(0, result.ProcessingOrder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentRequirement_Constructor_NegativeProcessingOrder()
        {
            var result = new PaymentRequirement(id, termId, ruleId, -1, deferrals, plans);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PaymentRequirement_Constructor_NullDeferralNullPlans()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PaymentRequirement_Constructor_NullDeferralNoPlans()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, null, new List<PaymentPlanOption>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PaymentRequirement_Constructor_NoDeferralNullPlans()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, new List<PaymentDeferralOption>(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PaymentRequirement_Constructor_NoDeferralNoPlans()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, new List<PaymentDeferralOption>(), new List<PaymentPlanOption>());
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_ValidDeferralNullPlans()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, deferrals, null);

            Assert.AreEqual(deferrals[0], result.DeferralOptions.ElementAt(0));
            Assert.AreEqual(0, result.PaymentPlanOptions.Count());
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_ValidDeferralNoPlans()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, deferrals, new List<PaymentPlanOption>());

            Assert.AreEqual(deferrals[0], result.DeferralOptions.ElementAt(0));
            Assert.AreEqual(0, result.PaymentPlanOptions.Count());
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_NullDeferralValidPlans()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, null, plans);

            Assert.AreEqual(plans[0], result.PaymentPlanOptions.ElementAt(0));
            Assert.AreEqual(0, result.DeferralOptions.Count());
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_NoDeferralValidPlans()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, new List<PaymentDeferralOption>(), plans);

            Assert.AreEqual(plans[0], result.PaymentPlanOptions.ElementAt(0));
            Assert.AreEqual(0, result.DeferralOptions.Count());
        }

        [TestMethod]
        public void PaymentRequirement_Constructor_ValidDeferralValidPlans()
        {
            var result = new PaymentRequirement(id, termId, ruleId, processingOrder, deferrals, plans);

            Assert.AreEqual(deferrals[0], result.DeferralOptions.ElementAt(0));
            Assert.AreEqual(plans[0], result.PaymentPlanOptions.ElementAt(0));
        }
    }
}
