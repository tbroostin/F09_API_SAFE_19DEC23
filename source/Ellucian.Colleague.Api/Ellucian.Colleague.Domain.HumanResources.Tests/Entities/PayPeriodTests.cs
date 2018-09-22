/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayPeriodTests
    {
        public DateTime startDate;
        public DateTime endDate;

        public PayPeriod payPeriod;

        [TestClass]
        public class PayPeriodConstructorTests : PayPeriodTests
        {
            public PayPeriod createPayPeriod()
            {
                return new PayPeriod(startDate, endDate);
            }

            [TestInitialize]
            public void Initialize()
            {
                startDate = new DateTime(2016, 01, 01);
                endDate   = new DateTime(2016, 01, 31);
            }

            [TestMethod]
            public void ConstructorSetsPropertiesTest()
            {
                payPeriod = createPayPeriod();
                Assert.AreEqual(startDate, payPeriod.StartDate);
                Assert.AreEqual(endDate, payPeriod.EndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StartDateAfterEndDateTest()
            {
                startDate = endDate.AddDays(1);
                createPayPeriod();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EndDateBeforeStartDateTest()
            {
                endDate = startDate.AddDays(-1);
                createPayPeriod();
            }
        }

        [TestClass]
        public class PayPeriodAttributeTests : PayPeriodTests
        {
            [TestInitialize]
            public void Initialize()
            {
                startDate = new DateTime(2016, 01, 01);
                endDate = new DateTime(2016, 01, 31);

                payPeriod = new PayPeriod(startDate, endDate);
            }

            [TestMethod]
            public void StartDateTest()
            {
                var testDate = startDate.AddDays(-1);
                payPeriod.StartDate = testDate;
                Assert.AreEqual(testDate, payPeriod.StartDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void StartDateMustBeBeforeEndDateTest()
            {
                var testDate = endDate.AddDays(1);
                payPeriod.StartDate = testDate;
            }

            [TestMethod]
            public void EndDateTest()
            {
                var testDate = endDate.AddDays(30);
                payPeriod.EndDate = testDate;
                Assert.AreEqual(testDate, payPeriod.EndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EndDateMustBeAfterStartDateTest()
            {
                var testDate = startDate.AddDays(-1);
                payPeriod.EndDate = testDate;
            }
        }

        [TestClass]
        public class PayPeriodEqualsTests : PayPeriodTests
        {
            public PayPeriod createPayPeriod()
            {
                return new PayPeriod(startDate, endDate);
            }

            [TestInitialize]
            public void Initialize()
            {
                startDate = new DateTime(2016, 01, 01);
                endDate = new DateTime(2016, 01, 31);
            }

            [TestMethod]
            public void ObjectsEqualWhenDatesAreEqualTest()
            {
                var dateRange1 = createPayPeriod();
                var dateRange2 = createPayPeriod();

                Assert.AreEqual(dateRange1, dateRange2);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenStartDatesNotEqualTest()
            {
                var dateRange1 = createPayPeriod();
                startDate = new DateTime(2015, 07, 07);
                var dateRange2 = createPayPeriod();

                Assert.AreNotEqual(dateRange1, dateRange2);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenEndDatesNotEqualTest()
            {
                var dateRange1 = createPayPeriod();
                endDate = new DateTime(2016, 02, 29);
                var dateRange2 = createPayPeriod();

                Assert.AreNotEqual(dateRange1, dateRange2);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsNullTest()
            {
                var dateRange1 = createPayPeriod();
                Assert.AreNotEqual(dateRange1, null);
                Assert.AreNotEqual(null, dateRange1);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsDifferentTypeTest()
            {
                var dateRange1 = createPayPeriod();
                var notDateRange = new PayCycle("PC", "Fake Pay Cycle", DayOfWeek.Monday);
                Assert.AreNotEqual(dateRange1, notDateRange);
            }

            [TestMethod]
            public void HashCodesEqualWhenStartandEndDatesEqualTest()
            {
                var dateRange1 = createPayPeriod();
                var dateRange2 = createPayPeriod();
                Assert.AreEqual(dateRange1.GetHashCode(), dateRange2.GetHashCode());
            }

            [TestMethod]
            public void HashCodesNotEqualWhenStartDatesNotEqualTest()
            {
                var dateRange1 = createPayPeriod();
                startDate = new DateTime(2015, 07, 07);
                var dateRange2 = createPayPeriod();
                Assert.AreNotEqual(dateRange1.GetHashCode(), dateRange2.GetHashCode());
            }

            [TestMethod]
            public void HashCodesNotEqualWhenEndDatesNotEqualTest()
            {
                var dateRange1 = createPayPeriod();
                endDate = new DateTime(2016, 02, 29);
                var dateRange2 = createPayPeriod();
                Assert.AreNotEqual(dateRange1.GetHashCode(), dateRange2.GetHashCode());
            }
        }
    }
}
