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
    public class PayStatementSourceEarningsTests
    {
        string earningTypeId;
        string earningTypeDescription;
        decimal? unitsWorked;
        decimal rate;
        decimal periodPaymentAmount;
        PayStatementSourceEarnings earning;
        [TestInitialize]
        public void Initialize()
        {
            earningTypeId = "Reg";
            earningTypeDescription = "ular";
            unitsWorked = 12.1M;
            rate = 1.12M;
            periodPaymentAmount = 13.552M;
        }

        [TestMethod]
        public void PropertiesAreSet()
        {
            earning = new PayStatementSourceEarnings(earningTypeId, earningTypeDescription, unitsWorked,rate, periodPaymentAmount);
            Assert.AreEqual(earningTypeId, earning.EarningsTypeId);
            Assert.AreEqual(earningTypeDescription, earning.EarningsTypeDescription);
            Assert.AreEqual(unitsWorked, earning.UnitsWorked);
            Assert.AreEqual(rate, earning.Rate);
            Assert.AreEqual(periodPaymentAmount, earning.PeriodPaymentAmount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullEarningTypeIdError()
        {
            earning = new PayStatementSourceEarnings(null, earningTypeDescription, unitsWorked, rate, periodPaymentAmount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullEarningTypeDescriptionError()
        {
            earning = new PayStatementSourceEarnings(earningTypeId, null, unitsWorked, rate, periodPaymentAmount);
        }
    }
}
