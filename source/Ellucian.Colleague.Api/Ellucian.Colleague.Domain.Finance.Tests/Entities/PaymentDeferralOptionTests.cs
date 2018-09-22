using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentDeferralOptionTests
    {
        private DateTime startDate = new DateTime(2013, 5, 1);
        private DateTime endDate;
        private decimal percent;

        [TestMethod]
        public void PaymentDeferralOption_Constructor_ValidStartDate()
        {
            endDate = startDate.AddDays(30);
            percent = 50;
            var result = new PaymentDeferralOption(startDate, endDate, percent);

            Assert.AreEqual(startDate, result.EffectiveStart);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentDeferralOption_Constructor_BadEndDate()
        {
            endDate = startDate.AddDays(-1);
            percent = 50;
            var result = new PaymentDeferralOption(startDate, endDate, percent);
        }

        [TestMethod]
        public void PaymentDeferralOption_Constructor_ValidNullEndDate()
        {
            percent = 50;
            var result = new PaymentDeferralOption(startDate, null, percent);

            Assert.AreEqual(null, result.EffectiveEnd);
        }

        [TestMethod]
        public void PaymentDeferralOption_Constructor_ValidEndDate()
        {
            endDate = startDate.AddDays(30);
            percent = 50;
            var result = new PaymentDeferralOption(startDate, endDate, percent);

            Assert.AreEqual(endDate, result.EffectiveEnd);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentDeferralOption_Constructor_NegativePercent()
        {
            endDate = startDate.AddDays(30);
            percent = -50;
            var result = new PaymentDeferralOption(startDate, endDate, percent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentDeferralOption_Constructor_PercentOver100()
        {
            endDate = startDate.AddDays(30);
            percent = 150;
            var result = new PaymentDeferralOption(startDate, endDate, percent);
        }

        [TestMethod]
        public void PaymentDeferralOption_Constructor_ValidPercent()
        {
            endDate = startDate.AddDays(30);
            percent = 50;
            var result = new PaymentDeferralOption(startDate, endDate, percent);

            Assert.AreEqual(percent, result.DeferralPercent);
        }
    }
}
