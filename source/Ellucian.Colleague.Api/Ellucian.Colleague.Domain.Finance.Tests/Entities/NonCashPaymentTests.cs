// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class NonCashPaymentTests
    {
        static string payMethod = "BANK";
        static decimal goodAmount = 100;
        static decimal badAmount = 0;

        [TestMethod]
        public void NonCashPayment_Constructor_GoodPaymentMethod()
        {
            var result = new NonCashPayment(payMethod, goodAmount);
            Assert.AreEqual(payMethod, result.PaymentMethodCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonCashPayment_Constructor_NullPaymentMethod()
        {
            var result = new NonCashPayment(null, goodAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonCashPayment_Constructor_EmptyPaymentMethod()
        {
            var result = new NonCashPayment(string.Empty, goodAmount);
        }

        [TestMethod]
        public void NonCashPayment_Constructor_GoodAmount()
        {
            var result = new NonCashPayment(payMethod, goodAmount);
            Assert.AreEqual(goodAmount, result.Amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NonCashPayment_Constructor_ZeroAmount()
        {
            var result = new NonCashPayment(payMethod, badAmount);
        }
    }
}
