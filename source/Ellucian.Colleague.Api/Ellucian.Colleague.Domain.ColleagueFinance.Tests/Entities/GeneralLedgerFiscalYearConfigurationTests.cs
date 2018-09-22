// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GeneralLedgerFiscalYearConfigurationTests
    {
        #region Initialize and Cleanup
        private GeneralLedgerFiscalYearConfigurationBuilder Builder;

        [TestInitialize]
        public void Initialize()
        {
            Builder = new GeneralLedgerFiscalYearConfigurationBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Builder = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        public void Success()
        {
            var actualEntity = Builder.Build();
            Assert.AreEqual(Builder.StartMonth, actualEntity.StartMonth);
            Assert.AreEqual(Builder.CurrentYear, actualEntity.CurrentFiscalYear);
            Assert.AreEqual(Builder.FiscalMonth, actualEntity.CurrentFiscalMonth);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public void StartMonthOutOfRange_0()
        {
            Builder.WithStartMonth(0).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public void StartMonthOutOfRange_13()
        {
            Builder.WithStartMonth(13).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCurrentYear()
        {
            Builder.WithCurrentYear(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyCurrentYear()
        {
            Builder.WithCurrentYear("").Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public void CurrentFiscalMonthOutOfRange_0()
        {
            Builder.WithFiscalMonth(0).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationException))]
        public void CurrentFiscalMonthOutOfRange_12()
        {
            Builder.WithFiscalMonth(13).Build();
        }
        #endregion

        #region FiscalYearForToday tests
        [TestMethod]
        public void FiscalYearForToday_CurrentMonthIsBeforeStartMonth()
        {
            // Set the FY start month in a future month
            var startMonth = DateTime.Now.Add(new TimeSpan(32, 0, 0, 0)).Month;
            var expectedYear = DateTime.Now.Year;

            if (DateTime.Now.Month >= startMonth)
                expectedYear += 1;

            var configuration = Builder.WithStartMonth(startMonth).Build();
            var actualYear = configuration.FiscalYearForToday;
            Assert.AreEqual(expectedYear, actualYear);
        }

        [TestMethod]
        public void FiscalYearForToday_CurrentMonthSameAsStartMonth()
        {
            // Set the FY start month as today
            var startMonth = DateTime.Now.Month;
            var expectedYear = DateTime.Now.Year;

            if (DateTime.Now.Month >= startMonth)
                expectedYear += 1;

            var configuration = Builder.WithStartMonth(startMonth).Build();
            var actualYear = configuration.FiscalYearForToday;
            Assert.AreEqual(expectedYear, actualYear);
        }

        [TestMethod]
        public void FiscalYearForToday_CurrentMonthIsAfterStartMonth()
        {
            // Set the FY start month as today
            var startMonth = DateTime.Now.Subtract(new TimeSpan(32, 0, 0, 0)).Month;
            var expectedYear = DateTime.Now.Year;

            if (DateTime.Now.Month >= startMonth)
                expectedYear += 1;

            var configuration = Builder.WithStartMonth(startMonth).Build();
            var actualYear = configuration.FiscalYearForToday;
            Assert.AreEqual(expectedYear, actualYear);
        }
        #endregion

        #region StartOfFiscalYear
        [TestMethod]
        public void StartOfFiscalYear_StartInJanuary()
        {
            var fiscalYearEntity = Builder
                .WithCurrentYear(DateTime.Now.Year.ToString())
                .WithStartMonth(1)
                .Build();

            var expectedDate = new DateTime(Int32.Parse(fiscalYearEntity.CurrentFiscalYear), fiscalYearEntity.StartMonth, 1);
            Assert.AreEqual(expectedDate, fiscalYearEntity.StartOfFiscalYear);
        }

        [TestMethod]
        public void StartOfFiscalYear_StartInJune()
        {
            var fiscalYearEntity = Builder
                .WithCurrentYear(DateTime.Now.Year.ToString())
                .WithStartMonth(6)
                .Build();

            var expectedDate = new DateTime(Int32.Parse(fiscalYearEntity.CurrentFiscalYear)-1, fiscalYearEntity.StartMonth, 1);
            Assert.AreEqual(expectedDate, fiscalYearEntity.StartOfFiscalYear);
        }

        [TestMethod]
        public void StartOfFiscalYear_StartInDecember()
        {
            var fiscalYearEntity = Builder
                .WithCurrentYear(DateTime.Now.Year.ToString())
                .WithStartMonth(12)
                .Build();

            var expectedDate = new DateTime(Int32.Parse(fiscalYearEntity.CurrentFiscalYear)-1, fiscalYearEntity.StartMonth, 1);
            Assert.AreEqual(expectedDate, fiscalYearEntity.StartOfFiscalYear);
        }
        #endregion

        #region EndOfFiscalYear
        [TestMethod]
        public void EndOfFiscalYear_StartInJanuary()
        {
            var fiscalYearEntity = Builder
                .WithCurrentYear(DateTime.Now.Year.ToString())
                .WithStartMonth(1)
                .Build();

            var expectedDate = new DateTime(Int32.Parse(fiscalYearEntity.CurrentFiscalYear), 12, 31);
            Assert.AreEqual(expectedDate, fiscalYearEntity.EndOfFiscalYear);
        }

        [TestMethod]
        public void EndOfFiscalYear_StartInJune()
        {
            var fiscalYearEntity = Builder
                .WithCurrentYear(DateTime.Now.Year.ToString())
                .WithStartMonth(6)
                .Build();

            var expectedDate = new DateTime(Int32.Parse(fiscalYearEntity.CurrentFiscalYear), fiscalYearEntity.StartMonth, 1).AddDays(-1);
            Assert.AreEqual(expectedDate, fiscalYearEntity.EndOfFiscalYear);
        }

        [TestMethod]
        public void EndOfFiscalYear_StartInDecember()
        {
            var fiscalYearEntity = Builder
                .WithCurrentYear(DateTime.Now.Year.ToString())
                .WithStartMonth(12)
                .Build();

            var expectedDate = new DateTime(Int32.Parse(fiscalYearEntity.CurrentFiscalYear), fiscalYearEntity.StartMonth, 1).AddDays(-1);
            Assert.AreEqual(expectedDate, fiscalYearEntity.EndOfFiscalYear);
        }
        #endregion
    }
}