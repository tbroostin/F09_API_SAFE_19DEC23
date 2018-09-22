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
    public class ImmediatePaymentOptionsTests
    {
        bool chargesOnPlan = false;
        decimal regBalance = 2000.00m;
        decimal minPayment = 500.00m;
        decimal deferral = 75m;
        string paymentPlanTemplateId = "DEFAULT";
        DateTime? paymentPlanFirstDueDate = DateTime.Today.AddDays(14);
        decimal paymentPlanAmount = 1500.00m;
        string paymentPlanReceivableTypeCode = "01";
        decimal downPaymentAmount = 150.00m;
        DateTime? downPaymentDate = DateTime.Today.AddDays(3);

        [TestMethod]
        public void ImmediatePaymentOptions_Constructor_ChargesOnPaymentPlanTrue()
        {
            var result = new ImmediatePaymentOptions(true, regBalance, minPayment, deferral);
            Assert.AreEqual(true, result.ChargesOnPaymentPlan);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_Constructor_ChargesOnPaymentPlanFalse()
        {
            var result = new ImmediatePaymentOptions(false, regBalance, minPayment, deferral);
            Assert.AreEqual(false, result.ChargesOnPaymentPlan);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImmediatePaymentOptions_Constructor_InvalidRegistrationBalance()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, -1m, minPayment, deferral);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_Constructor_ValidRegistrationBalance()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            Assert.AreEqual(regBalance, result.RegistrationBalance);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImmediatePaymentOptions_Constructor_InvalidMinimumPaymentNegative()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, -1m, deferral);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ImmediatePaymentOptions_Constructor_InvalidMinimumPaymentTooHigh()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, regBalance + 1m, deferral);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_Constructor_ValidMinimumPayment()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            Assert.AreEqual(minPayment, result.MinimumPayment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImmediatePaymentOptions_Constructor_InvalidDeferralPercentageTooLow()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, -1m);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImmediatePaymentOptions_Constructor_InvalidDeferralPercentageTooHigh()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, 100.01m);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_Constructor_ValidDeferralPercentage()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            Assert.AreEqual(deferral, result.DeferralPercentage);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_ValidPaymentPlanTemplateId()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
            Assert.AreEqual(paymentPlanTemplateId, result.PaymentPlanTemplateId);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_NullPaymentPlanTemplateId()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(null, null, 0m, null, 0m, null);
            Assert.AreEqual(null, result.PaymentPlanTemplateId);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_EmptyPaymentPlanTemplateId()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(string.Empty, null, 0m, null, 0m, null);
            Assert.AreEqual(string.Empty, result.PaymentPlanTemplateId);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_ValidPaymentPlanFirstDueDate()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
            Assert.AreEqual(paymentPlanFirstDueDate, result.PaymentPlanFirstDueDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_NullPaymentPlanFirstDueDateWithValidTemplateId()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, null, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_NegativePaymentPlanAmountWithValidTemplateId()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, -1000m, paymentPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_ZeroPaymentPlanAmountWithValidTemplateId()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, 0m, paymentPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_PositivePaymentPlanAmountWithNullTemplateId()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(null, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_PositivePaymentPlanAmountWithEmptyTemplateId()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(string.Empty, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_ValidPaymentPlanAmount()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
            Assert.AreEqual(paymentPlanAmount, result.PaymentPlanAmount);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_ValidPaymentPlanReceivableTypeCode()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
            Assert.AreEqual(paymentPlanReceivableTypeCode, result.PaymentPlanReceivableTypeCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_NullReceivableTypeCodeWithValidPlanAmount()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, null, paymentPlanAmount + 100m, downPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_EmptyReceivableTypeCodeWithValidPlanAmount()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, string.Empty, paymentPlanAmount + 100m, downPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_DownPaymentAmountGreaterThanPlanAmount()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, paymentPlanAmount + 100m, downPaymentDate);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_ValidDownPaymentAmount()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
            Assert.AreEqual(downPaymentAmount, result.DownPaymentAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_NullDownPaymentDateWithDownPayment()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_DownPaymentDateEarlierThanCurrentDate()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, DateTime.Today.AddDays(-7));
        }

        [TestMethod]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_ValidDownPaymentDateInFuture()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, DateTime.Today.AddDays(7));
            Assert.AreEqual(DateTime.Today.AddDays(7), result.DownPaymentDate);
        }

        [TestMethod]
        public void ImmediatePaymentOptions_AddPaymentPlanInformation_ValidDownPaymentDateToday()
        {
            var result = new ImmediatePaymentOptions(chargesOnPlan, regBalance, minPayment, deferral);
            result.AddPaymentPlanInformation(paymentPlanTemplateId, paymentPlanFirstDueDate, paymentPlanAmount, paymentPlanReceivableTypeCode, downPaymentAmount, DateTime.Today);
            Assert.AreEqual(DateTime.Today, result.DownPaymentDate);
        }
    }
}
