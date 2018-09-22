/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
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
    public class PayStatementEarningTests
    {
        //public PayStatementEarnings actualEarnings;

        public EarningsType inputEarningsType;
        public EarningsDifferential inputEarningsDifferential;

        public decimal? inputHours;
        public decimal? inputRate;
        public decimal inputAmount;

        public List<PayrollRegisterEarningsEntry> ytdEarnings;

        public void InitializePayStatementEarningsTests()
        {
            inputEarningsType = new EarningsType("REG", "Regualar", true, EarningsCategory.Regular, EarningsMethod.Accrued, null);
            inputEarningsDifferential = new EarningsDifferential("1st", "First Shift");

            inputHours = 40;
            inputRate = 9;
            inputAmount = 360;

            ytdEarnings = new List<PayrollRegisterEarningsEntry>()
            {
                new PayrollRegisterEarningsEntry("REG", 360, 360, 0, 40, 9, HourlySalaryIndicator.Hourly),
                new PayrollRegisterEarningsEntry("REG", 360, 360, 0, 40, 9, HourlySalaryIndicator.Hourly)
            };
        }

        [TestClass]
        public class StandardPeriodEarningsConstructorTests : PayStatementEarningTests
        {
            public PayStatementEarnings actual
            {
                get
                {
                    return new PayStatementEarnings(inputEarningsType, inputHours, inputRate, inputAmount, ytdEarnings);
                }
            }
            [TestInitialize]
            public void Initialize()
            {
                InitializePayStatementEarningsTests();                
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullEarningsTypeThrowsExceptionTest()
            {
                EarningsType nullEarningsType = null;
                new PayStatementEarnings(nullEarningsType, inputHours, inputRate, inputAmount, ytdEarnings);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullYtdEarningsEntriesThrowsExceptionTest()
            {
                IEnumerable<PayrollRegisterEarningsEntry> nullYtdEarnings = null;
                new PayStatementEarnings(inputEarningsType, inputHours, inputRate, inputAmount, nullYtdEarnings);
            }

            [TestMethod]
            public void EarningsTypeTest()
            {
                Assert.AreEqual(inputEarningsType.Id, actual.EarningsTypeId);
                Assert.AreEqual(inputEarningsType.Description, actual.EarningsTypeDescription);
            }

            [TestMethod]
            public void PeriodAttributesTest()
            {
                Assert.AreEqual(inputHours, actual.UnitsWorked);
                Assert.AreEqual(inputRate, actual.Rate);
                Assert.AreEqual(inputAmount, actual.PeriodPaymentAmount);
            }

            [TestMethod]
            public void YearToDateTest()
            {
                Assert.AreEqual(ytdEarnings.Sum(ytd => ytd.TotalPeriodEarningsAmount), actual.YearToDatePaymentAmount);
            }
        }

        [TestClass]
        public class StandardYearToDateOnlyConstructorTests : PayStatementEarningTests
        {

            public PayStatementEarnings actual
            {
                get
                {
                    return new PayStatementEarnings(inputEarningsType, ytdEarnings);   
                }
            }
            [TestInitialize]
            public void Initialize()
            {
                InitializePayStatementEarningsTests();
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullEarningsTypeThrowsExceptionTest()
            {
                EarningsType nullEarningsType = null;
                new PayStatementEarnings(nullEarningsType, ytdEarnings);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullYtdEarningsEntriesThrowsExceptionTest()
            {
                IEnumerable<PayrollRegisterEarningsEntry> nullYtdEarnings = null;
                new PayStatementEarnings(inputEarningsType, nullYtdEarnings);
            }

            [TestMethod]
            public void EarningsTypeTest()
            {
                Assert.AreEqual(inputEarningsType.Id, actual.EarningsTypeId);
                Assert.AreEqual(inputEarningsType.Description, actual.EarningsTypeDescription);
            }

            [TestMethod]
            public void PeriodPropertiesAreNullTest()
            {
                Assert.IsNull(actual.UnitsWorked);
                Assert.IsNull(actual.Rate);
                Assert.IsNull(actual.PeriodPaymentAmount);
            }

            [TestMethod]
            public void YearToDateTest()
            {
                Assert.AreEqual(ytdEarnings.Sum(ytd => ytd.TotalPeriodEarningsAmount), actual.YearToDatePaymentAmount);
            }

        }   
        
        [TestClass]
        public class DifferentialPeriodConstructorTests : PayStatementEarningTests
        {
            
            public PayStatementEarnings actual
            {
                get
                {
                    return new PayStatementEarnings(inputEarningsDifferential, inputHours, inputRate, inputAmount, ytdEarnings);
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                InitializePayStatementEarningsTests();

                inputHours = 3;
                inputRate = .5m;
                inputAmount = 1.5m;

                ytdEarnings.ForEach(ytd => ytd.SetEarningsDifferential(inputEarningsDifferential.Code, inputHours.Value, inputRate, inputAmount));
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullEarningsTypeThrowsExceptionTest()
            {
                EarningsDifferential nullEarningsDifferential = null;
                new PayStatementEarnings(nullEarningsDifferential, inputHours, inputRate, inputAmount, ytdEarnings);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullYtdEarningsEntriesThrowsExceptionTest()
            {
                IEnumerable<PayrollRegisterEarningsEntry> nullYtdEarnings = null;
                new PayStatementEarnings(inputEarningsDifferential, inputHours, inputRate, inputAmount, nullYtdEarnings);
            }

            [TestMethod]
            public void EarningsDifferentialTest()
            {
                Assert.AreEqual(inputEarningsDifferential.Code, actual.EarningsTypeId);
                Assert.AreEqual(inputEarningsDifferential.Description, actual.EarningsTypeDescription);
            }

            [TestMethod]
            public void PeriodPropertiesTest()
            {
                Assert.AreEqual(inputHours, actual.UnitsWorked);
                Assert.AreEqual(inputRate, actual.Rate);
                Assert.AreEqual(inputAmount, actual.PeriodPaymentAmount);
            }

            [TestMethod]
            public void YearToDateTest()
            {
                Assert.AreEqual(ytdEarnings.Sum(ytd => ytd.DifferentialPeriodEarningsAmount), actual.YearToDatePaymentAmount);
            }
        }

        [TestClass]
        public class DifferentialYearToDateOnlyConstructorTests : PayStatementEarningTests
        {

            public PayStatementEarnings actual
            {
                get
                {
                    return new PayStatementEarnings(inputEarningsDifferential, ytdEarnings);
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                InitializePayStatementEarningsTests();

                inputHours = 3;
                inputRate = .5m;
                inputAmount = 1.5m;

                ytdEarnings.ForEach(ytd => ytd.SetEarningsDifferential(inputEarningsDifferential.Code, inputHours.Value, inputRate, inputAmount));
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullEarningsTypeThrowsExceptionTest()
            {
                EarningsDifferential nullEarningsDifferential = null;
                new PayStatementEarnings(nullEarningsDifferential, ytdEarnings);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullYtdEarningsEntriesThrowsExceptionTest()
            {
                IEnumerable<PayrollRegisterEarningsEntry> nullYtdEarnings = null;
                new PayStatementEarnings(inputEarningsDifferential, nullYtdEarnings);
            }

            [TestMethod]
            public void EarningsDifferentialTest()
            {
                Assert.AreEqual(inputEarningsDifferential.Code, actual.EarningsTypeId);
                Assert.AreEqual(inputEarningsDifferential.Description, actual.EarningsTypeDescription);
            }

            [TestMethod]
            public void PeriodPropertiesTest()
            {
                Assert.IsNull(actual.UnitsWorked);
                Assert.IsNull(actual.Rate);
                Assert.IsNull(actual.PeriodPaymentAmount);
            }

            [TestMethod]
            public void YearToDateTest()
            {
                Assert.AreEqual(ytdEarnings.Sum(ytd => ytd.DifferentialPeriodEarningsAmount), actual.YearToDatePaymentAmount);
            }
        }
    }
}

