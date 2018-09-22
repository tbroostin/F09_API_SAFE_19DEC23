// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentAllocationTests
    {
        string id = "12345";
        string id2 = "23456";
        string paymentId = "67890";
        string paymentId2 = "78901";
        PaymentAllocationSource source = PaymentAllocationSource.User;
        decimal amount = 100;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentAllocation_Constructor_NullId()
        {
            var result = new PaymentAllocation(null,paymentId,source,amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentAllocation_Constructor_EmptyId()
        {
            var result = new PaymentAllocation(string.Empty, paymentId, source, amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentAllocation_Constructor_NullPaymentId()
        {
            var result = new PaymentAllocation(id, null, source, amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentAllocation_Constructor_EmptyPaymentId()
        {
            var result = new PaymentAllocation(id, string.Empty, source, amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentAllocation_Constructor_NullAllocationSource()
        {
            var result = new PaymentAllocation(id, paymentId, null, amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentAllocation_Constructor_NullAmount()
        {
            var result = new PaymentAllocation(id, paymentId, source, null);
        }

        [TestMethod]
        public void PaymentAllocation_Constructor_Valid()
        {
            var result = new PaymentAllocation(id, paymentId, source, amount);

            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(paymentId, result.PaymentId);
            Assert.AreEqual(source, result.Source);
            Assert.AreEqual(amount, result.Amount);
        }

        [TestMethod]
        public void PaymentAllocation_UnallocatedAmount_NullChargeId()
        {
            var result = new PaymentAllocation(id, paymentId, source, amount) { ChargeId = null };
            Assert.AreEqual(result.Amount, result.UnallocatedAmount);
        }

        [TestMethod]
        public void PaymentAllocation_UnallocatedAmount_EmptyChargeId()
        {
            var result = new PaymentAllocation(id, paymentId, source, amount) { ChargeId = string.Empty };
            Assert.AreEqual(result.Amount, result.UnallocatedAmount);
        }

        [TestMethod]
        public void PaymentAllocation_UnallocatedAmount_ValidChargeId()
        {
            var result = new PaymentAllocation(id, paymentId, source, amount) { ChargeId = "13579" };
            Assert.AreEqual(0, result.UnallocatedAmount);
        }

        [TestMethod]
        public void PaymentAllocation_Equals_NullObject()
        {
            var original = new PaymentAllocation(id, paymentId, source, amount);
            object newPa = null;
            Assert.IsFalse(original.Equals(newPa));
        }

        [TestMethod]
        public void PaymentAllocation_Equals_NullCharge()
        {
            var original = new PaymentAllocation(id, paymentId, source, amount);
            Charge newPa = null;
            Assert.IsFalse(original.Equals(newPa));
        }

        [TestMethod]
        public void PaymentAllocation_Equals_NonPaymentAllocationObject()
        {
            var original = new PaymentAllocation(id, paymentId, source, amount);
            var pa = new PaymentDeferralOption(DateTime.Today, DateTime.Today.AddDays(3), 25m);
            Assert.IsFalse(original.Equals(pa));
        }

        [TestMethod]
        public void PaymentAllocation_Equals_IdMismatch()
        {
            var original = new PaymentAllocation(id, paymentId, source, amount);
            var newPa = new PaymentAllocation(id2, paymentId, source, amount);
            Assert.IsFalse(original.Equals(newPa));
        }

        [TestMethod]
        public void PaymentAllocation_Equals_Valid()
        {
            var original = new PaymentAllocation(id, paymentId, source, amount);
            var newPa = new PaymentAllocation(id, paymentId, source, amount);
            Assert.IsTrue(original.Equals(newPa));
        }

        [TestMethod]
        public void PaymentAllocation_GetHashCode_SameIdHashEqual()
        {
            var original = new PaymentAllocation(id, paymentId, source, amount);
            var newPa = new PaymentAllocation(id, paymentId2, source, amount);
            Assert.AreEqual(original.GetHashCode(), newPa.GetHashCode());
        }

        [TestMethod]
        public void PaymentAllocation_GetHashCode_DifferentIdHashNotEqual()
        {
            var original = new PaymentAllocation(id, paymentId, source, amount);
            var newPa = new PaymentAllocation(id2, paymentId2, source, amount);
            Assert.AreNotEqual(original.GetHashCode(), newPa.GetHashCode());
        }
    }
}
