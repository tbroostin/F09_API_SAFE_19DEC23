// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class FinanceConfigurationTests
    {
        FinanceConfiguration config;
        PaymentRequirement termPayPlanOption1;
        PaymentRequirement termPayPlanOption2;
        PaymentRequirement termPayPlanOption3;
        PaymentPlanOption payPlanOption1;

        [TestInitialize]
        public void Initialize() 
        {
            config = new FinanceConfiguration();
            payPlanOption1 = new PaymentPlanOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(7), "TEMPLATE", DateTime.Today.AddDays(14));
            termPayPlanOption1 = new PaymentRequirement("1", "TERM1", "RULE1", 1, null, new List<PaymentPlanOption>() { payPlanOption1 });
            termPayPlanOption2 = new PaymentRequirement("2", "TERM1", "RULE2", 2, null, new List<PaymentPlanOption>() { payPlanOption1 });
            termPayPlanOption3 = new PaymentRequirement("3", "TERM2", "RULE1", 1, null, new List<PaymentPlanOption>() { payPlanOption1 });

        }

        [TestCleanup]
        public void Cleanup() 
        {
            config = null;
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_DisplayedReceivableTypes()
        {
            Assert.AreEqual(0, config.DisplayedReceivableTypes.Count);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_PartialAccountPaymentsAllowed()
        {
            Assert.IsTrue(config.PartialAccountPaymentsAllowed);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_PartialPlanPaymentsAllowed()
        {
            Assert.AreEqual(PartialPlanPayments.Allowed, config.PartialPlanPaymentsAllowed);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_PartialDepositPaymentsAllowed()
        {
            Assert.IsTrue(config.PartialDepositPaymentsAllowed);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_UseGuaranteedChecks()
        {
            Assert.IsFalse(config.UseGuaranteedChecks);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_PaymentMethods()
        {
            Assert.AreEqual(0, config.PaymentMethods.Count);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_IncludeSchedule()
        {
            Assert.IsTrue(config.IncludeSchedule);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_IncludeDetail()
        {
            Assert.IsTrue(config.IncludeDetail);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_IncludeHistory()
        {
            Assert.IsTrue(config.IncludeHistory);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_RemittanceAddress()
        {
            Assert.AreEqual(0, config.RemittanceAddress.Count);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_Periods()
        {
            Assert.AreEqual(0, config.Periods.Count);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_StatementMessage()
        {
            Assert.AreEqual(0, config.StatementMessage.Count);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_Links()
        {
            Assert.AreEqual(0, config.Links.Count);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_UserPaymentPlanCreationEnabled()
        {
            Assert.IsFalse(config.UserPaymentPlanCreationEnabled);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_PaymentPlanEligibilityRuleIds()
        {
            Assert.AreEqual(0, config.PaymentPlanEligibilityRuleIds.Count);
        }

        [TestMethod]
        public void FinanceConfiguration_Constructor_TermPaymentPlanRequirements()
        {
            Assert.AreEqual(0, config.TermPaymentPlanRequirements.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinanceConfiguration_AddDisplayedReceivableType_Null_ReceivableType()
        {
            config.AddDisplayedReceivableType(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void FinanceConfiguration_AddDisplayedReceivableType_Duplicate_ReceivableType()
        {
            config.AddDisplayedReceivableType(new PayableReceivableType("01", true));
            config.AddDisplayedReceivableType(new PayableReceivableType("01", false));
        }

        [TestMethod]
        public void FinanceConfiguration_AddDisplayedReceivableType_Valid()
        {
            config.AddDisplayedReceivableType(new PayableReceivableType("01", true));
            config.AddDisplayedReceivableType(new PayableReceivableType("02", false));
            Assert.AreEqual(2, config.DisplayedReceivableTypes.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinanceConfiguration_AddPaymentPlanEligibilityRuleId_Null_RuleId()
        {
            config.AddPaymentPlanEligibilityRuleId(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void FinanceConfiguration_AddPaymentPlanEligibilityRuleId_Duplicate_RuleId()
        {
            config.AddPaymentPlanEligibilityRuleId("RULE1");
            config.AddPaymentPlanEligibilityRuleId("RULE1");
        }

        [TestMethod]
        public void FinanceConfiguration_AddPaymentPlanEligibilityRuleId_Valid()
        {
            config.AddPaymentPlanEligibilityRuleId("RULE1");
            config.AddPaymentPlanEligibilityRuleId("RULE2"); 
            Assert.AreEqual(2, config.PaymentPlanEligibilityRuleIds.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinanceConfiguration_AddTermPaymentPlanRequirement_Null_RuleId()
        {
            config.AddTermPaymentPlanRequirement(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void FinanceConfiguration_AddTermPaymentPlanRequirement_Duplicate_TermAndRule()
        {
            config.AddTermPaymentPlanRequirement(termPayPlanOption1);
            config.AddTermPaymentPlanRequirement(termPayPlanOption1);
        }

        [TestMethod]
        public void FinanceConfiguration_AddTermPaymentPlanRequirement_Different_RuleIds()
        {
            config.AddTermPaymentPlanRequirement(termPayPlanOption1);
            config.AddTermPaymentPlanRequirement(termPayPlanOption2);
            Assert.AreEqual(2, config.TermPaymentPlanRequirements.Count);
        }

        [TestMethod]
        public void FinanceConfiguration_AddTermPaymentPlanRequirement_Different_TermIds()
        {
            config.AddTermPaymentPlanRequirement(termPayPlanOption1);
            config.AddTermPaymentPlanRequirement(termPayPlanOption3);
            Assert.AreEqual(2, config.TermPaymentPlanRequirements.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinanceConfiguration_IsReceivableTypePayable_Null_ReceivableType()
        {
            var isPayable = config.IsReceivableTypePayable(null);
        }

        [TestMethod]
        public void FinanceConfiguration_IsReceivableTypePayable_No_DisplayedReceivableTypes()
        {
            Assert.IsTrue(config.IsReceivableTypePayable("01"));
        }

        [TestMethod]
        public void FinanceConfiguration_IsReceivableTypePayable_ReceivableType_Not_Payable()
        {
            config.AddDisplayedReceivableType(new PayableReceivableType("01", false));
            Assert.IsFalse(config.IsReceivableTypePayable("01"));
        }

        [TestMethod]
        public void FinanceConfiguration_IsReceivableTypePayable_ReceivableType_Not_Dislayed_or_Payable()
        {
            config.AddDisplayedReceivableType(new PayableReceivableType("02", true));
            Assert.IsFalse(config.IsReceivableTypePayable("01"));
        }

        [TestMethod]
        public void FinanceConfiguration_IsReceivableTypePayable_ReceivableType_Displayed_and_Payable()
        {
            config.AddDisplayedReceivableType(new PayableReceivableType("01", true));
            Assert.IsTrue(config.IsReceivableTypePayable("01"));
        }

        [TestMethod]
        public void FinanceConfiguration_EcommerceProviderLink_IsEmptyOnInitializationTest()
        {
            Assert.IsTrue(string.IsNullOrEmpty(config.EcommerceProviderLink));
        }

        [TestMethod]
        public void FinanceConfiguration_EcommerceProviderLink_SetTest()
        {
            string expected = "www.google.com";
            config.EcommerceProviderLink = expected;
            Assert.AreEqual(expected, config.EcommerceProviderLink);
        }

        /// <summary>
        /// Validate DisplayPotentialD7Amounts is initialized to false
        /// </summary>
        [TestMethod]
        public void FinanceConfiguration_DisplayPotentialD7Amounts_IsFalseOnInitializationTest()
        {
            Assert.IsFalse(config.DisplayPotentialD7Amounts);
        }
    }
}
