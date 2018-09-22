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
    public class PaymentPlanOptionTests
    {
        private DateTime startDate = new DateTime(2013, 5, 1);
        private DateTime endDate;
        private string templateId = "TEMPLATE";
        private DateTime firstPayDate = new DateTime(2013, 9, 1);

        [TestMethod]
        public void PaymentPlanOption_Constructor_ValidStartDate()
        {
            endDate = startDate.AddDays(30);
            var result = new PaymentPlanOption(startDate, endDate, templateId, firstPayDate);

            Assert.AreEqual(startDate, result.EffectiveStart);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlanOption_Constructor_EndDateEarlierThanStartDate()
        {
            endDate = startDate.AddDays(-1);
            var result = new PaymentPlanOption(startDate, endDate, templateId, firstPayDate);
        }

        [TestMethod]
        public void PaymentPlanOption_Constructor_EndDateLaterThanStartDate()
        {
            endDate = startDate.AddDays(30);
            var result = new PaymentPlanOption(startDate, endDate, templateId, firstPayDate);

            Assert.AreEqual(endDate, result.EffectiveEnd);
        }

        [TestMethod]
        public void PaymentPlanOption_Constructor_EndDateEqualsStartDate()
        {
            endDate = startDate;
            var result = new PaymentPlanOption(startDate, endDate, templateId, firstPayDate);

            Assert.AreEqual(endDate, result.EffectiveEnd);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanOption_Constructor_NullTemplateId()
        {
            endDate = startDate.AddDays(30);
            templateId = null;
            var result = new PaymentPlanOption(startDate, endDate, templateId, firstPayDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlanOption_Constructor_EmptyTemplateId()
        {
            endDate = startDate.AddDays(30);
            templateId = String.Empty;
            var result = new PaymentPlanOption(startDate, endDate, templateId, firstPayDate);
        }

        [TestMethod]
        public void PaymentPlanOption_Constructor_ValidTemplateId()
        {
            endDate = startDate.AddDays(30);
            var result = new PaymentPlanOption(startDate, endDate, templateId, firstPayDate);

            Assert.AreEqual(templateId, result.TemplateId);
        }

        [TestMethod]
        public void PaymentPlanOption_Constructor_ValidPayDate()
        {
            endDate = startDate.AddDays(30);
            var result = new PaymentPlanOption(startDate, endDate, templateId, firstPayDate);

            Assert.AreEqual(firstPayDate, result.FirstPaymentDate);
        }
    }
}
