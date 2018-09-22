using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class FinancialPeriodTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinancialPeriod_PastPeriod_NullEndDate()
        {
            var result = new FinancialPeriod(PeriodType.Past, DateTime.Now, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FinancialPeriod_PastPeriod_MaxEndDate()
        {
            var result = new FinancialPeriod(PeriodType.Past, DateTime.Now, DateTime.MaxValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinancialPeriod_CurrentPeriod_NullStartDate()
        {
            var result = new FinancialPeriod(PeriodType.Current, null, DateTime.Now);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinancialPeriod_CurrentPeriod_NullEndDate()
        {
            var result = new FinancialPeriod(PeriodType.Current, DateTime.Now, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FinancialPeriod_CurrentPeriod_DatesOutOfOrder()
        {
            DateTime start = new DateTime(2012, 10, 1);
            DateTime end = new DateTime(2012, 9, 1);
            var result = new FinancialPeriod(PeriodType.Current, start, end);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinancialPeriod_FuturePeriod_NullStartDate()
        {
            var result = new FinancialPeriod(PeriodType.Future, null, DateTime.Now);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FinancialPeriod_FuturePeriod_MinStartDate()
        {
            var result = new FinancialPeriod(PeriodType.Future, DateTime.MinValue, DateTime.Now);
        }

        [TestMethod]
        public void FinancialPeriod_PastPeriodValid_NullStartDate()
        {
            DateTime? start = null;
            DateTime end = new DateTime(2011, 12, 31);
            var result = new FinancialPeriod(PeriodType.Past, start, end);
            Assert.AreEqual(result.Type, PeriodType.Past);
            Assert.AreEqual(result.Start.Date, DateTime.MinValue.Date);
            Assert.AreEqual(result.End.Date, end.Date);
        }

        [TestMethod]
        public void FinancialPeriod_PastPeriodValid_NonNullStartDate()
        {
            DateTime? start = new DateTime(2001, 12, 31);
            DateTime end = new DateTime(2011, 12, 31);
            var result = new FinancialPeriod(PeriodType.Past, start, end);
            Assert.AreEqual(result.Type, PeriodType.Past);
            Assert.AreEqual(result.Start.Date, DateTime.MinValue.Date);
            Assert.AreEqual(result.End.Date, end.Date);
        }

        [TestMethod]
        public void FinancialPeriod_CurrentPeriodValid()
        {
            DateTime start = new DateTime(2012, 1, 1);
            DateTime end = new DateTime(2012, 12, 31);
            var result = new FinancialPeriod(PeriodType.Current, start, end);
            Assert.AreEqual(result.Type, PeriodType.Current);
            Assert.AreEqual(result.Start.Date, start.Date);
            Assert.AreEqual(result.End.Date, end.Date);
        }

        [TestMethod]
        public void FinancialPeriod_FuturePeriodValid_NullEndDate()
        {
            DateTime start = new DateTime(2013, 1, 1);
            DateTime? end = null;
            var result = new FinancialPeriod(PeriodType.Future, start, end);
            Assert.AreEqual(result.Type, PeriodType.Future);
            Assert.AreEqual(result.Start.Date, start.Date);
            Assert.AreEqual(result.End.Date, DateTime.MaxValue.Date);
        }

        [TestMethod]
        public void FinancialPeriod_FuturePeriodValid_NonNullEndDate()
        {
            DateTime start = new DateTime(2013, 1, 1);
            DateTime? end = new DateTime(2023, 1, 1);
            var result = new FinancialPeriod(PeriodType.Future, start, end);
            Assert.AreEqual(result.Type, PeriodType.Future);
            Assert.AreEqual(result.Start.Date, start.Date);
            Assert.AreEqual(result.End.Date, DateTime.MaxValue.Date);
        }
    }
}
