// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// This class tests the RecurringVoucher class. The recurring voucher class is an extension of the
    /// AccountsPayablePurchasingDocument just like the voucher class, and the voucher test class tests the 
    /// the abstract classes it extends.
    /// </summary>
    [TestClass]
    public class RecurringVoucherTests
    {
        #region Initialize and Cleanup
        private TestRecurringVoucherRepository recurringVoucherRepository;
        private RecurringVoucherBuilder rcVoucherBuilder;
        private RecurringVoucherScheduleBuilder scheduleBuilder;

        [TestInitialize]
        public void Initialize()
        {
            recurringVoucherRepository = new TestRecurringVoucherRepository();
            rcVoucherBuilder = new RecurringVoucherBuilder();
            scheduleBuilder = new RecurringVoucherScheduleBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            recurringVoucherRepository = null;
            rcVoucherBuilder = null;
            scheduleBuilder = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        public void RecurringVoucher_ConstructorInitialization()
        {
            var recurringVoucher = rcVoucherBuilder.Build();

            var schedDate = new DateTime(2015, 04, 01);
            var schedAmount = 50m;
            var schedule = new RecurringVoucherSchedule(schedDate, schedAmount);
            recurringVoucher.AddSchedule(schedule);

            var approvalId = "GTT";
            var approval = new Approver(approvalId);
            approval.SetApprovalName("Gary Thorne");
            approval.ApprovalDate = new DateTime(2015, 04, 01);
            recurringVoucher.AddApprover(approval);

            Assert.AreEqual(rcVoucherBuilder.RecurringVoucher.Status, recurringVoucher.Status, "Status should be initialized.");
            Assert.AreEqual(rcVoucherBuilder.RecurringVoucher.StatusDate, recurringVoucher.StatusDate, "Status date should be initialized.");
            Assert.AreEqual(rcVoucherBuilder.RecurringVoucher.InvoiceNumber, recurringVoucher.InvoiceNumber, "Invoice number should be initialized.");
            Assert.AreEqual(rcVoucherBuilder.RecurringVoucher.InvoiceDate, recurringVoucher.InvoiceDate, "Invoice date should be initialized.");
            Assert.AreEqual(1, recurringVoucher.Approvers.Count);
            Assert.AreEqual(1, recurringVoucher.Schedules.Count);
            foreach (var sch in recurringVoucher.Schedules)
            {
                Assert.AreEqual(schedAmount, sch.Amount, "Schedule amounts should be the same.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RecurringVoucher_NullInvoiceNumber()
        {
            rcVoucherBuilder.WithInvoiceNumber(null).Build();
        }
        #endregion

        #region TotalScheduleAmountInLocalCurrency tests
        [TestMethod]
        public void TotalScheduleAmountInLocalCurrency_NoCurrencyCode()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = null;

            Assert.AreEqual(null, recurringVoucher.TotalScheduleAmountInLocalCurrency, "Should be null if no currency code exists.");
        }

        [TestMethod]
        public void TotalScheduleAmountInLocalCurrency_ReturnsNullWhenExchangeRateIsNull()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = null;

            // Add schedules
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());

            // Check the value
            Assert.AreEqual(null, recurringVoucher.TotalScheduleAmountInLocalCurrency, "Local total should be null.");
        }

        [TestMethod]
        public void TotalScheduleAmountInLocalCurrency_ReturnsNullWhenExchangeRateIsZero()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = 0m;

            // Add schedules
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());

            // Check the value
            Assert.AreEqual(null, recurringVoucher.TotalScheduleAmountInLocalCurrency, "Local total should be null.");
        }

        [TestMethod]
        public void TotalScheduleAmountInLocalCurrency_ReturnsNullWhenExchangeRateIsNegative()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = -1.4m;

            // Add schedules
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());

            // Check the value
            Assert.AreEqual(null, recurringVoucher.TotalScheduleAmountInLocalCurrency, "Local total should be null.");
        }

        [TestMethod]
        public void TotalScheduleAmountInLocalCurrency_ExchangeRateGreaterThan1()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "EUR";
            recurringVoucher.ExchangeRate = 1.3m;

            // Add schedules
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());

            // Check the value
            var expectedAmount = Math.Round(recurringVoucher.Schedules.Sum(x => x.Amount)/recurringVoucher.ExchangeRate.Value, 2);
            Assert.AreEqual(expectedAmount, recurringVoucher.TotalScheduleAmountInLocalCurrency, "Local total should equal the sum of the schedule amounts divided by the exchange rate.");
            Assert.IsTrue(recurringVoucher.TotalScheduleAmountInLocalCurrency < recurringVoucher.Schedules.Sum(x => x.Amount), "The local total should be less than the foreign amount.");
        }

        [TestMethod]
        public void TotalScheduleAmountInLocalCurrency_ExchangeRateLessThan1()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = 0.8m;

            // Add schedules
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());

            // Check the value
            var expectedAmount = Math.Round(recurringVoucher.Schedules.Sum(x => x.Amount) / recurringVoucher.ExchangeRate.Value, 2);
            Assert.AreEqual(expectedAmount, recurringVoucher.TotalScheduleAmountInLocalCurrency, "Local total should equal the sum of the schedule amounts divided by the exchange rate.");
            Assert.IsTrue(recurringVoucher.TotalScheduleAmountInLocalCurrency > recurringVoucher.Schedules.Sum(x => x.Amount), "The local total should be greater than the foreign amount.");
        }

        [TestMethod]
        public void TotalScheduleAmountInLocalCurrency_TotalIsZero()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = 1.14m;

            // Add schedules
            recurringVoucher.AddSchedule(scheduleBuilder.WithDate(new DateTime(2015, 01, 01)).WithAmount(100.00m).Build());
            recurringVoucher.AddSchedule(scheduleBuilder.WithDate(new DateTime(2015, 01, 02)).WithAmount(-100.00m).Build());

            // Check the value
            Assert.AreEqual(0m, recurringVoucher.TotalScheduleAmountInLocalCurrency, "Local total should be zero.");
        }
        #endregion

        #region TotalScheduleTaxAmountInLocalCurrency tests
        [TestMethod]
        public void TotalScheduleTaxAmountInLocalCurrency_NoCurrencyCode()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = null;

            Assert.IsNull(recurringVoucher.TotalScheduleTaxAmountInLocalCurrency, "Should be null if no currency code exists.");
        }

        [TestMethod]
        public void TotalScheduleTaxAmountInLocalCurrency_ReturnsNullWhenExchangeRateIsNull()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = null;

            // Add schedules
            for (int i = 1; i < 5; i++)
            {
                var schedule = scheduleBuilder.WithDate(new DateTime(2015, 01, i)).Build();
                schedule.TaxAmount = 44.12m;
                recurringVoucher.AddSchedule(schedule);
            }

            // Check the value
            Assert.AreEqual(null, recurringVoucher.TotalScheduleTaxAmountInLocalCurrency, "Local tax total should be null.");
        }

        [TestMethod]
        public void TotalScheduleTaxAmountInLocalCurrency_ReturnsNullWhenExchangeRateIsZero()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = 0m;

            // Add schedules
            for (int i = 1; i < 5; i++)
            {
                var schedule = scheduleBuilder.WithDate(new DateTime(2015, 01, i)).Build();
                schedule.TaxAmount = 44.12m;
                recurringVoucher.AddSchedule(schedule);
            }

            // Check the value
            Assert.AreEqual(null, recurringVoucher.TotalScheduleTaxAmountInLocalCurrency, "Local tax total should be null.");
        }

        [TestMethod]
        public void TotalScheduleTaxAmountInLocalCurrency_ReturnsNullWhenExchangeRateIsNegative()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = -1.1m;

            // Add schedules
            for (int i = 1; i < 5; i++)
            {
                var schedule = scheduleBuilder.WithDate(new DateTime(2015, 01, i)).Build();
                schedule.TaxAmount = 44.12m;
                recurringVoucher.AddSchedule(schedule);
            }

            // Check the value
            Assert.AreEqual(null, recurringVoucher.TotalScheduleTaxAmountInLocalCurrency, "Local tax total should be null.");
        }

        [TestMethod]
        public void TotalScheduleTaxAmountInLocalCurrency_ExchangeRateGreaterThan1()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "EUR";
            recurringVoucher.ExchangeRate = 1.3m;

            // Add schedules
            for (int i = 1; i < 5; i++)
            {
                var schedule = scheduleBuilder.WithDate(new DateTime(2015, 01, i)).Build();
                schedule.TaxAmount = 44.12m;
                recurringVoucher.AddSchedule(schedule);
            }

            // Check the value
            var expectedAmount = Math.Round(recurringVoucher.Schedules.Sum(x => x.TaxAmount) / recurringVoucher.ExchangeRate.Value, 2);
            Assert.AreEqual(expectedAmount, recurringVoucher.TotalScheduleTaxAmountInLocalCurrency, "Local tax total should equal the sum of the schedule tax amounts divided by the exchange rate.");
            Assert.IsTrue(recurringVoucher.TotalScheduleTaxAmountInLocalCurrency < recurringVoucher.Schedules.Sum(x => x.TaxAmount), "The local tax total should be less than the foreign tax total.");
        }

        [TestMethod]
        public void TotalScheduleTaxAmountInLocalCurrency_ExchangeRateLessThan1()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = 0.7m;

            // Add schedules
            for (int i = 1; i < 5; i++)
            {
                var schedule = scheduleBuilder.WithDate(new DateTime(2015, 01, i)).Build();
                schedule.TaxAmount = 44.12m;
                recurringVoucher.AddSchedule(schedule);
            }

            // Check the value
            var expectedAmount = Math.Round(recurringVoucher.Schedules.Sum(x => x.TaxAmount) / recurringVoucher.ExchangeRate.Value, 2);
            Assert.AreEqual(expectedAmount, recurringVoucher.TotalScheduleTaxAmountInLocalCurrency, "Local tax total should equal the sum of the schedule tax amounts divided by the exchange rate.");
            Assert.IsTrue(recurringVoucher.TotalScheduleTaxAmountInLocalCurrency > recurringVoucher.Schedules.Sum(x => x.TaxAmount), "The local tax total should be greater than the foreign tax total.");
        }

        [TestMethod]
        public void TotalScheduleTaxAmountInLocalCurrency_TotalIsZero()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = 1.14m;

            // Add schedules
            recurringVoucher.AddSchedule(scheduleBuilder.WithDate(new DateTime(2015, 01, 01)).WithAmount(100.00m).Build());
            recurringVoucher.AddSchedule(scheduleBuilder.WithDate(new DateTime(2015, 01, 02)).WithAmount(-50.00m).Build());
            recurringVoucher.AddSchedule(scheduleBuilder.WithDate(new DateTime(2015, 01, 03)).WithAmount(-50.00m).Build());

            // Check the value
            Assert.AreEqual(0m, recurringVoucher.TotalScheduleTaxAmountInLocalCurrency, "Local tax total should be zero.");
        }
        #endregion

        #region Adding schedule tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RecurringVoucher_AddNullSchedule()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            recurringVoucher.AddSchedule(null);
        }

        [TestMethod]
        public void RecurringVoucher_AddSameScheduleTwice()
        {
            var recurringVoucher = rcVoucherBuilder.Build();
            var schedule = scheduleBuilder.Build();
            recurringVoucher.AddSchedule(schedule);
            recurringVoucher.AddSchedule(schedule);

            Assert.AreEqual(1, recurringVoucher.Schedules.Count);
        }

        #endregion
    }
}
