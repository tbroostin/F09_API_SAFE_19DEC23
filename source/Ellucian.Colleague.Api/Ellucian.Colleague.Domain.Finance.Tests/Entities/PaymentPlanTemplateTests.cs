// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentPlanTemplateTests
    {
        string id = "DEFAULT";
        string description = "Default Payment Plan";
        bool isActive = true;
        PlanFrequency frequency = PlanFrequency.Weekly;
        int numberOfPayments = 5;
        decimal minimumAmount = 5m;
        decimal? maximumAmount = 10000m;
        string customFrequencySubroutine = "S.SAMPLE.PLAN.FREQ.SUBR";
        List<string> messages = new List<string>();
        PaymentPlanIneligibilityReason? reason;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_Constructor_NullId()
        {
            var result = new PaymentPlanTemplate(null, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_Constructor_EmptyId()
        {
            var result = new PaymentPlanTemplate(string.Empty, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_ValidId()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_Constructor_NullDescription()
        {
            var result = new PaymentPlanTemplate(id, null, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_Constructor_EmptyDescription()
        {
            var result = new PaymentPlanTemplate(id, string.Empty, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_ValidDescription()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            Assert.AreEqual(description, result.Description);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_ValidIsActive()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            Assert.AreEqual(isActive, result.IsActive);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_ValidFrequency()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            Assert.AreEqual(frequency, result.Frequency);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTemplate_Constructor_NumberOfPaymentsLessThan1()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, -1, minimumAmount, maximumAmount, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTemplate_Constructor_NumberOfPaymentsGreaterThan999()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, 1000, minimumAmount, maximumAmount, null);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_ValidNumberOfPayments()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            Assert.AreEqual(numberOfPayments, result.NumberOfPayments);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTemplate_Constructor_NegativeMinimumPlanAmount()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, -1000m, maximumAmount, null);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_ValidMinimumPlanAmount()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            Assert.AreEqual(minimumAmount, result.MinimumPlanAmount);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_NullMaximumPlanAmount()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, null, null); 
            Assert.AreEqual(null, result.MaximumPlanAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTemplate_Constructor_NegativeMaximumPlanAmount()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, 500m, -1, null);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_ValidMaximumPlanAmount()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            Assert.AreEqual(maximumAmount, result.MaximumPlanAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTemplate_Constructor_MaximumPlanAmountLessThanMinimumPlanAmount()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, 500m, 100m, null);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_NonCustomFrequencyAndNullCustomFrequencySubroutine()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            Assert.AreEqual(null, result.CustomFrequencySubroutine);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_NonCustomFrequencyAndEmptyCustomFrequencySubroutine()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, string.Empty);
            Assert.AreEqual(string.Empty, result.CustomFrequencySubroutine);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PaymentPlanTemplate_Constructor_NonCustomFrequencyAndSpecifiedCustomFrequencySubroutine()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, customFrequencySubroutine);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_Constructor_CustomFrequencyAndNullCustomFrequencySubroutine()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, PlanFrequency.Custom, numberOfPayments, minimumAmount, maximumAmount, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_Constructor_CustomFrequencyAndEmptyCustomFrequencySubroutine()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, PlanFrequency.Custom, numberOfPayments, minimumAmount, maximumAmount, string.Empty);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_CustomFrequencyAndSpecifiedCustomFrequencySubroutine()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, PlanFrequency.Custom, numberOfPayments, minimumAmount, maximumAmount, customFrequencySubroutine);
            Assert.AreEqual(customFrequencySubroutine, result.CustomFrequencySubroutine);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_ValidIncludesSetupChargeInFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            Assert.AreEqual(false, result.IncludeSetupChargeInFirstPayment);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_ValidSubtractsAnticipatedFinancialAid()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            Assert.AreEqual(false, result.SubtractAnticipatedFinancialAid);
        }

        [TestMethod]
        public void PaymentPlanTemplate_Constructor_ValidCalculatesPlanAmountAutomatically()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            Assert.AreEqual(false, result.CalculatePlanAmountAutomatically);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_AddAllowedReceivableTypeCode_NullReceivableTypeCode()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddAllowedReceivableTypeCode(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_AddAllowedReceivableTypeCode_EmptyReceivableTypeCode()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddAllowedReceivableTypeCode(string.Empty);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddAllowedReceivableTypeCode_ValidReceivableType()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddAllowedReceivableTypeCode("01");
            Assert.AreEqual("01", result.AllowedReceivableTypeCodes[0]);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddAllowedReceivableTypeCode_ValidReceivableTypeNoDuplicatesInList()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddAllowedReceivableTypeCode("01");
            result.AddAllowedReceivableTypeCode("02");
            Assert.AreEqual("01", result.AllowedReceivableTypeCodes[0]);
            Assert.AreEqual("02", result.AllowedReceivableTypeCodes[1]);
            Assert.AreEqual(2, result.AllowedReceivableTypeCodes.Count);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddAllowedReceivableTypeCode_ValidReceivableTypeCodeWithDuplicatesInList()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddAllowedReceivableTypeCode("01");
            result.AddAllowedReceivableTypeCode("01");
            Assert.AreEqual("01", result.AllowedReceivableTypeCodes[0]);
            Assert.AreEqual(1, result.AllowedReceivableTypeCodes.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_AddInvoiceExclusionRuleId_NullRuleId()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddInvoiceExclusionRuleId(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_AddInvoiceExclusionRuleId_EmptyRuleId()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddInvoiceExclusionRuleId(string.Empty);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddInvoiceExclusionRuleId_ValidRuleId()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddInvoiceExclusionRuleId("RULE1");
            Assert.AreEqual("RULE1", result.InvoiceExclusionRuleIds[0]);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddInvoiceExclusionRuleId_ValidRuleIdNoDuplicatesInList()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddInvoiceExclusionRuleId("RULE1");
            result.AddInvoiceExclusionRuleId("RULE2");
            Assert.AreEqual("RULE1", result.InvoiceExclusionRuleIds[0]);
            Assert.AreEqual("RULE2", result.InvoiceExclusionRuleIds[1]);
            Assert.AreEqual(2, result.InvoiceExclusionRuleIds.Count);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddInvoiceExclusionRuleId_ValidRuleIdWithDuplicatesInList()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddInvoiceExclusionRuleId("RULE1");
            result.AddInvoiceExclusionRuleId("RULE1");
            Assert.AreEqual("RULE1", result.InvoiceExclusionRuleIds[0]);
            Assert.AreEqual(1, result.InvoiceExclusionRuleIds.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_AddIncludedChargeCode_NullChargeCode()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddIncludedChargeCode(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_AddIncludedChargeCode_EmptyChargeCode()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddIncludedChargeCode(string.Empty);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddIncludedChargeCode_ValidChargeCode()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddIncludedChargeCode("MATFE");
            Assert.AreEqual("MATFE", result.IncludedChargeCodes[0]);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddIncludedChargeCode_ValidChargeCodeNoDuplicatesInList()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddIncludedChargeCode("MATFE");
            result.AddIncludedChargeCode("TECFE");
            Assert.AreEqual("MATFE", result.IncludedChargeCodes[0]);
            Assert.AreEqual("TECFE", result.IncludedChargeCodes[1]);
            Assert.AreEqual(2, result.IncludedChargeCodes.Count);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddIncludedChargeCode_ValidChargeCodeWithDuplicatesInList()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddIncludedChargeCode("MATFE");
            result.AddIncludedChargeCode("MATFE");
            Assert.AreEqual("MATFE", result.IncludedChargeCodes[0]);
            Assert.AreEqual(1, result.IncludedChargeCodes.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PaymentPlanTemplate_AddIncludedChargeCode_ValidChargeCodeWithExcludedChargeCodes()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddExcludedChargeCode("MATFE");
            result.AddIncludedChargeCode("TECFE");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_AddExcludedChargeCode_NullChargeCode()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddExcludedChargeCode(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_AddExcludedChargeCode_EmptyChargeCode()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddExcludedChargeCode(string.Empty);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddExcludedChargeCode_ValidChargeCode()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddExcludedChargeCode("MATFE");
            Assert.AreEqual("MATFE", result.ExcludedChargeCodes.ToList()[0]);
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddExcludedChargeCode_ValidChargeCodeNoDuplicatesInList()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddExcludedChargeCode("MATFE");
            result.AddExcludedChargeCode("TECFE");
            Assert.AreEqual("MATFE", result.ExcludedChargeCodes.ToList()[0]);
            Assert.AreEqual("TECFE", result.ExcludedChargeCodes.ToList()[1]);
            Assert.AreEqual(2, result.ExcludedChargeCodes.ToList().Count());
        }

        [TestMethod]
        public void PaymentPlanTemplate_AddExcludedChargeCode_ValidChargeCodeWithDuplicateInList()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddExcludedChargeCode("MATFE");
            result.AddExcludedChargeCode("MATFE");
            Assert.AreEqual("MATFE", result.ExcludedChargeCodes.ToList()[0]);
            Assert.AreEqual(1, result.ExcludedChargeCodes.ToList().Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PaymentPlanTemplate_AddExcludedChargeCode_ValidChargeCodeWithIncludedChargeCodes()
        {
            var result = new PaymentPlanTemplate(id, description, isActive, frequency, numberOfPayments, minimumAmount, maximumAmount, null); 
            result.AddIncludedChargeCode("MATFE");
            result.AddExcludedChargeCode("TECFE");
        }
                
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTemplate_CalculateDownPayment_PlanAmountLessThanZero()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            var downPayment = result.CalculateDownPaymentAmount(-100m);
        }

        [TestMethod] // 10% down payment, $25 setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateSetupCharge_DownPaymentSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateSetupChargeAmount(1000m);
            Assert.AreEqual(25m, amount);
        }

        [TestMethod] // No down payment, $25 setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateSetupCharge_NoDownPaymentSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateSetupChargeAmount(1000m);
            Assert.AreEqual(25m, amount);
        }

        [TestMethod]  // 10% down payment, $25 setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateSetupCharge_DownPaymentSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateSetupChargeAmount(1000m);
            Assert.AreEqual(25m, amount);
        }

        [TestMethod] // No down payment, $25 setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateSetupCharge_NoDownPaymentSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateSetupChargeAmount(1000m);
            Assert.AreEqual(25m, amount);
        }

        [TestMethod] // 10% down payment, no setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateSetupCharge_DownPaymentNoSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateSetupChargeAmount(1000m);
            Assert.AreEqual(0m, amount);
        }

        [TestMethod] // No down payment, no setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateSetupCharge_NoDownPaymentNoSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateSetupChargeAmount(1000m);
            Assert.AreEqual(0m, amount);
        }

        [TestMethod] // 10% down payment, no setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateSetupCharge_DownPaymentNoSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateSetupChargeAmount(1000m);
            Assert.AreEqual(0m, amount);
        }

        [TestMethod] // No down payment, no setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateSetupCharge_NoDownPaymentNoSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateSetupChargeAmount(1000m);
            Assert.AreEqual(0m, amount);
        }

        [TestMethod] // No down payment, no setup charge, setup charge percentage 
        public void PaymentPlanTemplate_CalculateSetupCharge_NoDownPaymentNoSetupCharge_SetupChargePercentage()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = false;
            result.SetupChargePercentage = 10m;
            var amount = result.CalculateSetupChargeAmount(1000m);
            Assert.AreEqual(100m, amount);
        }

        [TestMethod] // 10% down payment, $25 setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateDownPayment_DownPaymentSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateDownPaymentAmount(1000m);
            Assert.AreEqual(125m, amount);
        }

        [TestMethod] // No down payment, $25 setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateDownPayment_NoDownPaymentSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateDownPaymentAmount(1000m);
            Assert.AreEqual(0m, amount);
        }

        [TestMethod]  // 10% down payment, $25 setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateDownPayment_DownPaymentSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateDownPaymentAmount(1000m);
            Assert.AreEqual(105m, amount);
        }

        [TestMethod] // No down payment, $25 setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateDownPayment_NoDownPaymentSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateDownPaymentAmount(1000m);
            Assert.AreEqual(0m, amount);
        }

        [TestMethod] // 10% down payment, no setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateDownPayment_DownPaymentNoSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateDownPaymentAmount(1000m);
            Assert.AreEqual(100m, amount);
        }

        [TestMethod] // No down payment, no setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateDownPayment_NoDownPaymentNoSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateDownPaymentAmount(1000m);
            Assert.AreEqual(0m, amount);
        }

        [TestMethod] // 10% down payment, no setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateDownPayment_DownPaymentNoSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateDownPaymentAmount(1000m);
            Assert.AreEqual(100m, amount);
        }

        [TestMethod] // No down payment, no setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateDownPayment_NoDownPaymentNoSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateDownPaymentAmount(1000m);
            Assert.AreEqual(0m, amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTemplate_CalculateFirstPayment_NoPayments()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            var amount = result.CalculateFirstPaymentAmount(1000m, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTemplate_CalculateSetupChargeAmount_PlanAmountLessThanZero()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            var amount = result.CalculateSetupChargeAmount(-500);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanTemplate_CalculateScheduledPaymentAmount_NoPayments()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            var amount = result.CalculateScheduledPaymentAmount(1000m, 0);
        }

        [TestMethod] // 10% down payment, $25 setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateFirstPayment_DownPaymentSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateFirstPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(180m, amount);
        }

        [TestMethod] // No down payment, $25 setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateFirstPayment_NoDownPaymentSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateFirstPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(225m, amount);
        }

        [TestMethod]  // 10% down payment, $25 setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateFirstPayment_DownPaymentSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateFirstPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(184m, amount);
        }

        [TestMethod] // No down payment, $25 setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateFirstPayment_NoDownPaymentSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateFirstPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(205m, amount);
        }                                                           

        [TestMethod] // 10% down payment, no setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateFirstPayment_DownPaymentNoSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateFirstPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(180m, amount);
        }

        [TestMethod] // No down payment, no setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateFirstPayment_NoDownPaymentNoSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateFirstPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(200m, amount);
        }

        [TestMethod] // 10% down payment, no setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateFirstPayment_DownPaymentNoSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateFirstPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(180m, amount);
        }

        [TestMethod] // No down payment, no setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateFirstPayment_NoDownPaymentNoSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateFirstPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(200m, amount);
        }

        [TestMethod] // 10% down payment, $25 setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateScheduledPayment_DownPaymentSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateScheduledPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(180m, amount);
        }

        [TestMethod] // No down payment, $25 setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateScheduledPayment_NoDownPaymentSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateScheduledPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(200m, amount);
        }

        [TestMethod]  // 10% down payment, $25 setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateScheduledPayment_DownPaymentSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateScheduledPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(184m, amount);
        }

        [TestMethod] // No down payment, $25 setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateScheduledPayment_NoDownPaymentSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 25m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateScheduledPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(205m, amount);
        }

        [TestMethod] // 10% down payment, no setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateScheduledPayment_DownPaymentNoSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateScheduledPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(180m, amount);
        }

        [TestMethod] // No down payment, no setup charge, setup charge on first payment
        public void PaymentPlanTemplate_CalculateScheduledPayment_NoDownPaymentNoSetupChargeOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = true;
            var amount = result.CalculateScheduledPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(200m, amount);
        }

        [TestMethod] // 10% down payment, no setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateScheduledPayment_DownPaymentNoSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateScheduledPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(180m, amount);
        }

        [TestMethod] // No down payment, no setup charge, setup charge not on first payment
        public void PaymentPlanTemplate_CalculateScheduledPayment_NoDownPaymentNoSetupChargeNotOnFirstPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;
            result.SetupChargeAmount = 0m;
            result.IncludeSetupChargeInFirstPayment = false;
            var amount = result.CalculateScheduledPaymentAmount(1000m, numberOfPayments);
            Assert.AreEqual(200m, amount);
        }

        [TestMethod]
        public void PaymentPlanTemplate_CalculateDownPayment_PercentSetupChargeNotInDownPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 15m;
            result.SetupChargeAmount = 0m;
            result.SetupChargePercentage = 10m;
            result.IncludeSetupChargeInFirstPayment = false;
            var downPayment = result.CalculateDownPaymentAmount(1000m);

            Assert.AreEqual(165m, downPayment);
        }

        [TestMethod]
        public void PaymentPlanTemplate_CalculateDownPayment_PercentSetupChargeInDownPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 15m;
            result.SetupChargeAmount = 0m;
            result.SetupChargePercentage = 20m;
            result.IncludeSetupChargeInFirstPayment = true;
            var downPayment = result.CalculateDownPaymentAmount(1000m);

            Assert.AreEqual(350m, downPayment);
        }

        [TestMethod]
        public void PaymentPlanTemplate_CalculateDownPayment_FlatAndPercentSetupChargeNotInDownPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 15m;
            result.SetupChargeAmount = 100m;
            result.SetupChargePercentage = 10m;
            result.IncludeSetupChargeInFirstPayment = false;
            var downPayment = result.CalculateDownPaymentAmount(1000m);

            Assert.AreEqual(180m, downPayment);
        }

        [TestMethod]
        public void PaymentPlanTemplate_CalculateDownPayment_FlatAndPercentSetupChargeInDownPayment()
        {
            var result = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 15m;
            result.SetupChargeAmount = 100m;
            result.SetupChargePercentage = 20m;
            result.IncludeSetupChargeInFirstPayment = true;
            var downPayment = result.CalculateDownPaymentAmount(1000m);

            Assert.AreEqual(450m, downPayment);
        }

        [TestMethod]
        public void PaymentPlanTemplate_DownPaymentDate_NoDownPayment()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 0;

            Assert.IsFalse(result.DownPaymentDate.HasValue);
        }

        [TestMethod]
        public void PaymentPlanTemplate_DownPaymentDate_WithDownPaymentPlusZeroDays()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.DaysUntilDownPaymentIsDue = 0;

            Assert.IsTrue(result.DownPaymentDate.HasValue);
            Assert.AreEqual(DateTime.Today, result.DownPaymentDate);
        }

        [TestMethod]
        public void PaymentPlanTemplate_DownPaymentDate_WithDownPaymentPlusFiveDays()
        {
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 5m;
            result.DaysUntilDownPaymentIsDue = 5;

            Assert.IsTrue(result.DownPaymentDate.HasValue);
            Assert.AreEqual(DateTime.Today.AddDays(5), result.DownPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PaymentPlanTemplate_GetPaymentScheduleDates_CustomFrequency()
        {
            frequency = PlanFrequency.Custom;
            DateTime firstPaymentDate = DateTime.Today.AddMonths(1);
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, "CUSTOM.SUBROUTINE.NAME");
            var schedule = result.GetPaymentScheduleDates(firstPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PaymentPlanTemplate_GetPaymentScheduleDates_InvalidFrequency()
        {
            frequency = (PlanFrequency)7;
            DateTime firstPaymentDate = DateTime.Today.AddMonths(1);
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            var schedule = result.GetPaymentScheduleDates(firstPaymentDate);
        }

        [TestMethod]
        public void PaymentPlanTemplate_GetPaymentScheduleDates_WeeklyFrequency5Payments()
        {
            frequency = PlanFrequency.Weekly;
            numberOfPayments = 5;
            DateTime firstPaymentDate = DateTime.Today.AddMonths(1);
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            var schedule = result.GetPaymentScheduleDates(firstPaymentDate);

            Assert.AreEqual(numberOfPayments, schedule.Count);
            CollectionAssert.AllItemsAreNotNull(schedule);
            CollectionAssert.AllItemsAreInstancesOfType(schedule, typeof(DateTime));
            DateTime nextPaymentDate = firstPaymentDate;
            Assert.AreEqual(nextPaymentDate, schedule[0]);
            for (int i = 1; i < numberOfPayments; i++)
            {
                nextPaymentDate = nextPaymentDate.AddDays(7);
                Assert.AreEqual(nextPaymentDate, schedule[i], "Item {0} in schedule. {1}, is not the expected result: {2}", new[] { i.ToString(), nextPaymentDate.ToString(), schedule[i].ToString() });
            }
        }

        [TestMethod]
        public void PaymentPlanTemplate_GetPaymentScheduleDates_BieeklyFrequency7Payments()
        {
            frequency = PlanFrequency.Biweekly;
            numberOfPayments = 7;
            DateTime firstPaymentDate = DateTime.Today.AddMonths(1);
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            var schedule = result.GetPaymentScheduleDates(firstPaymentDate);

            Assert.AreEqual(numberOfPayments, schedule.Count);
            CollectionAssert.AllItemsAreNotNull(schedule);
            CollectionAssert.AllItemsAreInstancesOfType(schedule, typeof(DateTime));
            DateTime nextPaymentDate = firstPaymentDate;
            Assert.AreEqual(nextPaymentDate, schedule[0]);
            for (int i = 1; i < numberOfPayments; i++)
            {
                nextPaymentDate = nextPaymentDate.AddDays(14);
                Assert.AreEqual(nextPaymentDate, schedule[i], "Item {0} in schedule. {1}, is not the expected result: {2}", new[] { i.ToString(), nextPaymentDate.ToString(), schedule[i].ToString() });
            }
        }

        [Ignore]
        [TestMethod]
        public void PaymentPlanTemplate_GetPaymentScheduleDates_MonthlyFrequency4Payments()
        {
            frequency = PlanFrequency.Monthly;
            numberOfPayments = 4;
            DateTime firstPaymentDate = DateTime.Today.AddMonths(1);
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            var schedule = result.GetPaymentScheduleDates(firstPaymentDate);

            Assert.AreEqual(numberOfPayments, schedule.Count);
            CollectionAssert.AllItemsAreNotNull(schedule);
            CollectionAssert.AllItemsAreInstancesOfType(schedule, typeof(DateTime));
            DateTime nextPaymentDate = firstPaymentDate;
            Assert.AreEqual(nextPaymentDate, schedule[0]);
            for (int i = 1; i < numberOfPayments; i++)
            {
                nextPaymentDate = firstPaymentDate.AddMonths(i);
                Assert.AreEqual(nextPaymentDate, schedule[i], "Item " + i + " in schedule.");
            }
        }

        [TestMethod]
        public void PaymentPlanTemplate_GetPaymentScheduleDates_YearlyFrequency3Payments()
        {
            frequency = PlanFrequency.Yearly;
            numberOfPayments = 3;
            DateTime firstPaymentDate = DateTime.Today.AddMonths(1);
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            var schedule = result.GetPaymentScheduleDates(firstPaymentDate);

            Assert.AreEqual(numberOfPayments, schedule.Count);
            CollectionAssert.AllItemsAreNotNull(schedule);
            CollectionAssert.AllItemsAreInstancesOfType(schedule, typeof(DateTime));
            DateTime nextPaymentDate = firstPaymentDate;
            Assert.AreEqual(nextPaymentDate, schedule[0]);
            for (int i = 1; i < numberOfPayments; i++)
            {
                nextPaymentDate = nextPaymentDate.AddYears(1);
                Assert.AreEqual(nextPaymentDate, schedule[i], "Item {0} in schedule. {1}, is not the expected result: {2}", new[] { i.ToString(), nextPaymentDate.ToString(), schedule[i].ToString() });
            }
        }

        [TestMethod]
        public void PaymentPlanTemplate_GetPaymentScheduleDates_BieeklyFrequency4Payments2BeforeDownPaymentDate()
        {
            frequency = PlanFrequency.Biweekly;
            numberOfPayments = 4;
            DateTime firstPaymentDate = DateTime.Today.AddDays(-21);
            var result = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            result.DownPaymentPercentage = 10m;
            result.DaysUntilDownPaymentIsDue = 0;
            var schedule = result.GetPaymentScheduleDates(firstPaymentDate);

            Assert.AreEqual(numberOfPayments, schedule.Count);
            CollectionAssert.AllItemsAreNotNull(schedule);
            CollectionAssert.AllItemsAreInstancesOfType(schedule, typeof(DateTime));
            Assert.AreEqual(result.DownPaymentDate, DateTime.Today);
            Assert.AreEqual(DateTime.Today, schedule[0]);
            Assert.AreEqual(DateTime.Today, schedule[1]);
            Assert.AreEqual(DateTime.Today.AddDays(7), schedule[2]);
            Assert.AreEqual(DateTime.Today.AddDays(21), schedule[3]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanTemplate_IsValidForUserPaymentPlanCreation_Null_ReceivableCode()
        {
            var template = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            var valid = template.IsValidForUserPaymentPlanCreation(null, 5000m, out messages, out reason);
        }

        [TestMethod]
        public void PaymentPlanTemplate_IsValidForUserPaymentPlanCreation_PaymentPlanAmount_Below_MinimumPlanAmount()
        {
            var template = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            Assert.IsFalse(template.IsValidForUserPaymentPlanCreation("01", minimumAmount - 1, out messages, out reason));
        }

        [TestMethod]
        public void PaymentPlanTemplate_IsValidForUserPaymentPlanCreation_Active_False()
        {
            var template = new PaymentPlanTemplate(id, description, false, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            Assert.IsFalse(template.IsValidForUserPaymentPlanCreation("01", minimumAmount + 1, out messages, out reason));
        }

        [TestMethod]
        public void PaymentPlanTemplate_IsValidForUserPaymentPlanCreation_ModifyPlanAutomatically_False()
        {
            var template = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            template.CalculatePlanAmountAutomatically = true;
            template.ModifyPlanAutomatically = false;
            Assert.IsFalse(template.IsValidForUserPaymentPlanCreation("01", minimumAmount + 1, out messages, out reason));
        }

        [TestMethod]
        public void PaymentPlanTemplate_IsValidForUserPaymentPlanCreation_CalculatePlanAmountAutomatically_False()
        {
            var template = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            template.CalculatePlanAmountAutomatically = false;
            template.ModifyPlanAutomatically = true;
            Assert.IsFalse(template.IsValidForUserPaymentPlanCreation("01", minimumAmount + 1, out messages, out reason));
        }

        [TestMethod]
        public void PaymentPlanTemplate_IsValidForUserPaymentPlanCreation_ReceivableType_NotAllowed()
        {
            var template = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            template.AddAllowedReceivableTypeCode("02");
            template.TermsAndConditionsDocumentId = "PPT&C";
            Assert.IsFalse(template.IsValidForUserPaymentPlanCreation("01", minimumAmount + 1, out messages, out reason));
        }

        [TestMethod]
        public void PaymentPlanTemplate_IsValidForUserPaymentPlanCreation_TermsAndConditionsDocumentId_Null()
        {
            var template = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            template.AddAllowedReceivableTypeCode("01");
            template.TermsAndConditionsDocumentId = null;
            Assert.IsFalse(template.IsValidForUserPaymentPlanCreation("01", minimumAmount + 1, out messages, out reason));
        }

        [TestMethod]
        public void PaymentPlanTemplate_IsValidForUserPaymentPlanCreation_True()
        {
            var template = new PaymentPlanTemplate(id, description, true, frequency, numberOfPayments, minimumAmount, maximumAmount, null);
            template.CalculatePlanAmountAutomatically = true;
            template.ModifyPlanAutomatically = true;
            template.AddAllowedReceivableTypeCode("01");
            template.TermsAndConditionsDocumentId = "PPT&C";
            Assert.IsTrue(template.IsValidForUserPaymentPlanCreation("01", minimumAmount + 1, out messages, out reason));
        }
    }
}
