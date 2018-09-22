// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Services
{
    [TestClass]
    public class FinancialPeriodProcessorTests
    {
        private List<FinancialPeriod> periods;

        [TestInitialize]
        public void Initialize()
        {
            periods = TestFinancialPeriodRepository.FinancialPeriods;
        }

        [TestCleanup]
        public void Cleanup()
        {
            periods = null;
        }

        [TestClass]
        public class FinancialPeriodProcessor_GetPeriodByCode : FinancialPeriodProcessorTests
        {
            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByCode_NullCode()
            {
                var period = FinancialPeriodProcessor.GetPeriodByCode(null, periods);
                Assert.IsNull(period);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByCode_EmptyCode()
            {
                var period = FinancialPeriodProcessor.GetPeriodByCode(string.Empty, periods);
                Assert.IsNull(period);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByCode_PastCode()
            {
                var period = FinancialPeriodProcessor.GetPeriodByCode(FinanceTimeframeCodes.PastPeriod, periods);
                Assert.AreEqual(periods[0].End, period.End);
                Assert.AreEqual(periods[0].Start, period.Start);
                Assert.AreEqual(periods[0].Type, period.Type);
            }   

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByCode_CurrentCode()
            {
                var period = FinancialPeriodProcessor.GetPeriodByCode(FinanceTimeframeCodes.CurrentPeriod, periods);
                Assert.AreEqual(periods[1].End, period.End);
                Assert.AreEqual(periods[1].Start, period.Start);
                Assert.AreEqual(periods[1].Type, period.Type);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByCode_FutureCode()
            {
                var period = FinancialPeriodProcessor.GetPeriodByCode(FinanceTimeframeCodes.FuturePeriod, periods);
                Assert.AreEqual(periods[2].End, period.End);
                Assert.AreEqual(periods[2].Start, period.Start);
                Assert.AreEqual(periods[2].Type, period.Type);
            }
        }

        [TestClass]
        public class FinancialPeriodProcessor_GetPeriodByType : FinancialPeriodProcessorTests
        {
            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByType_NullType()
            {
                var period = FinancialPeriodProcessor.GetPeriodByType(null, periods);
                Assert.IsNull(period);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByType_NullPeriods()
            {
                var period = FinancialPeriodProcessor.GetPeriodByType(PeriodType.Past, null);
                Assert.IsNull(period);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByType_NoPeriods()
            {
                var period = FinancialPeriodProcessor.GetPeriodByType(PeriodType.Past, new List<FinancialPeriod>());
                Assert.IsNull(period);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByType_Past()
            {
                var period = FinancialPeriodProcessor.GetPeriodByType(PeriodType.Past, periods);
                Assert.AreEqual(periods[0].End, period.End);
                Assert.AreEqual(periods[0].Start, period.Start);
                Assert.AreEqual(periods[0].Type, period.Type);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByType_Current()
            {
                var period = FinancialPeriodProcessor.GetPeriodByType(PeriodType.Current, periods);
                Assert.AreEqual(periods[1].End, period.End);
                Assert.AreEqual(periods[1].Start, period.Start);
                Assert.AreEqual(periods[1].Type, period.Type);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByType_Future()
            {
                var period = FinancialPeriodProcessor.GetPeriodByType(PeriodType.Future, periods);
                Assert.AreEqual(periods[2].End, period.End);
                Assert.AreEqual(periods[2].Start, period.Start);
                Assert.AreEqual(periods[2].Type, period.Type);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodByType_NoMatchingPeriod()
            {
                var financialPeriods = new List<FinancialPeriod>(periods.GetRange(0, 2));
                var period = FinancialPeriodProcessor.GetPeriodByType(PeriodType.Future, financialPeriods);
                Assert.IsNull(period);
            }
        }

        [TestClass]
        public class FinancialPeriodProcessor_GetPeriodType : FinancialPeriodProcessorTests
        {
            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodType_NonPeriodCode()
            {
                var type = FinancialPeriodProcessor.GetPeriodType(null);
                Assert.IsNull(type);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodType_PastPeriodCode()
            {
                var type = FinancialPeriodProcessor.GetPeriodType(FinanceTimeframeCodes.PastPeriod);
                Assert.AreEqual(PeriodType.Past, type);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodType_CurrentPeriodCode()
            {
                var type = FinancialPeriodProcessor.GetPeriodType(FinanceTimeframeCodes.CurrentPeriod);
                Assert.AreEqual(PeriodType.Current, type);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetPeriodType_FuturePeriodCode()
            {
                var type = FinancialPeriodProcessor.GetPeriodType(FinanceTimeframeCodes.FuturePeriod);
                Assert.AreEqual(PeriodType.Future, type);
            }
        }

        [TestClass]
        public class FinancialPeriodProcessor_GetDateRangePeriod : FinancialPeriodProcessorTests
        {
            [TestMethod]
            public void FinancialPeriodProcessor_GetDateRangePeriod_NullPeriods()
            {
                var period = FinancialPeriodProcessor.GetDateRangePeriod(DateTime.Today.AddDays(-15), 
                    DateTime.Today.AddDays(15), null);
                Assert.IsNull(period);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetDateRangePeriod_NoPeriods()
            {
                var period = FinancialPeriodProcessor.GetDateRangePeriod(DateTime.Today.AddDays(-15), 
                    DateTime.Today.AddDays(15), new List<FinancialPeriod>());
                Assert.IsNull(period);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetDateRangePeriod_PastPeriod()
            {
                var period = FinancialPeriodProcessor.GetDateRangePeriod(DateTime.Today.AddDays(-90),
                    DateTime.Today.AddDays(-45), periods);
                Assert.AreEqual(PeriodType.Past, period);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetDateRangePeriod_CurrentPeriod()
            {
                var period = FinancialPeriodProcessor.GetDateRangePeriod(DateTime.Today.AddDays(-15),
                    DateTime.Today.AddDays(15), periods);
                Assert.AreEqual(PeriodType.Current, period);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_GetDateRangePeriod_FuturePeriod()
            {
                var period = FinancialPeriodProcessor.GetDateRangePeriod(DateTime.Today.AddDays(45),
                    null, periods);
                Assert.AreEqual(PeriodType.Future, period);
            }
        }

        [TestClass]
        public class FinancialPeriodProcessor_IsInPeriod : FinancialPeriodProcessorTests
        {
            [TestMethod]
            public void FinancialPeriodProcessor_IsInPeriod_True()
            {
                var isInPeriod = FinancialPeriodProcessor.IsInPeriod(DateTime.Today.AddDays(-90), DateTime.Today.AddDays(-45),
                    PeriodType.Past, periods);
                Assert.IsTrue(isInPeriod);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_IsInPeriod_False()
            {
                var isInPeriod = FinancialPeriodProcessor.IsInPeriod(DateTime.Today.AddDays(-90), DateTime.Today.AddDays(-45),
                    PeriodType.Future, periods);
                Assert.IsFalse(isInPeriod);
            }

            [TestMethod]
            public void FinancialPeriodProcessor_IsInPeriod_NoPeriods_False()
            {
                var isInPeriod = FinancialPeriodProcessor.IsInPeriod(DateTime.Today.AddDays(-90), DateTime.Today.AddDays(-45),
                    PeriodType.Past, null);
                Assert.IsFalse(isInPeriod);
            }
        }
    }
}
