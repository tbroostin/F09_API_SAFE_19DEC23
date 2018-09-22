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
    public class ScheduledPaymentTests
    {
        string id = "12345";
        decimal amount = 1500;
        string planId = "34567";
        DateTime dueDate = DateTime.Parse("09/01/2013");
        decimal amountPaid = 0;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduledPayment_Constructor_NullIdValidPlanId()
        {
            var result = new ScheduledPayment(null, planId, amount, dueDate, amountPaid, null);
            Assert.AreEqual(null, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduledPayment_Constructor_EmptyIdValidPlanId()
        {
            var result = new ScheduledPayment(string.Empty, planId, amount, dueDate, amountPaid, null);
            Assert.AreEqual(string.Empty, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduledPayment_Constructor_ValidIdNullPlanId()
        {
            var result = new ScheduledPayment(id, null, amount, dueDate, amountPaid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScheduledPayment_Constructor_ValidIdEmptyPlanId()
        {
            var result = new ScheduledPayment(id, string.Empty, amount, dueDate, amountPaid, null);
        }

        [TestMethod]
        public void ScheduledPayment_Constructor_ValidId()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void ScheduledPayment_Constructor_ValidPlanId()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void ScheduledPayment_Constructor_NullIdNullPlanId()
        {
            id = null;
            planId = null;
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);
            Assert.IsTrue(string.IsNullOrEmpty(result.Id));
            Assert.IsTrue(string.IsNullOrEmpty(result.PlanId));
        }

        [TestMethod]
        public void ScheduledPayment_Constructor_NullIdEmptyPlanId()
        {
            id = null;
            planId = string.Empty;
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);
            Assert.IsTrue(string.IsNullOrEmpty(result.Id));
            Assert.IsTrue(string.IsNullOrEmpty(result.PlanId));
        }

        [TestMethod]
        public void ScheduledPayment_Constructor_EmptyIdNullPlanId()
        {
            id = string.Empty;
            planId = null;
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);
            Assert.IsTrue(string.IsNullOrEmpty(result.Id));
            Assert.IsTrue(string.IsNullOrEmpty(result.PlanId));
        }

        [TestMethod]
        public void ScheduledPayment_Constructor_EmptyIdEmptyPlanId()
        {
            id = string.Empty;
            planId = string.Empty;
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);
            Assert.IsTrue(string.IsNullOrEmpty(result.Id));
            Assert.IsTrue(string.IsNullOrEmpty(result.PlanId));
        }

        [TestMethod]
        public void ScheduledPayment_SetIdAfterInitialization()
        {
            var result = new ScheduledPayment(null, null, amount, dueDate, amountPaid, null);
            result.Id = "23456";
            Assert.AreEqual("23456", result.Id);
        }

        [TestMethod]
        public void ScheduledPayment_SetPlanIdAfterInitialization()
        {
            var result = new ScheduledPayment(null, null, amount, dueDate, amountPaid, null);
            result.PlanId = "23456";
            Assert.AreEqual("23456", result.PlanId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ScheduledPayment_SetIdAfterSettingInConstructor()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);
            result.Id = "23456";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ScheduledPayment_SetPlanIdAfterSettingInConstructor()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);
            result.PlanId = "45678";
        }

        [TestMethod]
        public void ScheduledPayment_Constructor_ValidAmount()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);

            Assert.AreEqual(amount, result.Amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ScheduledPayment_Constructor_ZeroAmount()
        {
            var result = new ScheduledPayment(id, planId, 0, dueDate, amountPaid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ScheduledPayment_Constructor_NegativeAmount()
        {
            var result = new ScheduledPayment(id, planId, -1, dueDate, amountPaid, null);
        }

        [TestMethod]
        public void ScheduledPayment_Constructor_ValidAmountPaid()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, 50m, null);
            Assert.AreEqual(50m, result.AmountPaid);
        }

        [TestMethod]
        public void ScheduledPayment_Constructor_ZeroAmountPaid()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);
            Assert.AreEqual(amountPaid, result.AmountPaid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ScheduledPayment_Constructor_NegativeAmountPaid()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, -50m, null);
        }

        [TestMethod]
        public void ScheduledPayment_Constructor_ValidDueDate()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);

            Assert.AreEqual(dueDate, result.DueDate);
        }

        [TestMethod]
        public void ScheduledPayment_IsPastDue_False()
        {
            dueDate = DateTime.Now.AddDays(1);
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);

            Assert.IsFalse(result.IsPastDue);
        }

        [TestMethod]
        public void ScheduledPayment_IsPastDue_True()
        {
            dueDate = DateTime.Now.AddDays(-1);
            var result = new ScheduledPayment(id, planId, amount, dueDate, amountPaid, null);

            Assert.IsTrue(result.IsPastDue);
        }

        [TestMethod]
        public void ScheduledPayment_LastPaidDate()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, 50m, dueDate.AddDays(-3));
            Assert.AreEqual(dueDate.AddDays(-3), result.LastPaidDate);
        }

        [TestMethod]
        public void ScheduledPayment_Equals_NullScheduledPayment()
        {
            var result = new ScheduledPayment(id, planId, amount, dueDate, 50m, dueDate.AddDays(-3));
            ScheduledPayment sp2 = null;
            Assert.IsFalse(result.Equals(sp2));
        }

        [TestMethod]
        public void ScheduledPayment_Equals_NonScheduledPaymentObject()
        {
            ScheduledPayment sp1 = new ScheduledPayment(id, planId, amount, dueDate, 50m, dueDate.AddDays(-3));
            var sp2 = "NotAScheduledPayment";
            Assert.IsFalse(sp1.Equals(sp2));
        }

        [TestMethod]
        public void ScheduledPayment_Equals_SameId()
        {
            ScheduledPayment sp1 = new ScheduledPayment(id, planId, amount, dueDate, 50m, dueDate.AddDays(-3));
            ScheduledPayment sp2 = new ScheduledPayment(id, planId, amount, dueDate.AddDays(7), 50m, null);
            Assert.IsTrue(sp1.Equals(sp2));
        }

        [TestMethod]
        public void ScheduledPayment_Equals_DifferentId()
        {
            ScheduledPayment sp1 = new ScheduledPayment(id, planId, amount, dueDate, 50m, dueDate.AddDays(-3));
            ScheduledPayment sp2 = new ScheduledPayment(id+"1", planId, amount, dueDate.AddDays(7), 50m, null);
            Assert.IsFalse(sp1.Equals(sp2));
        }

        [TestMethod]
        public void ScheduledPayment_GetHashCode_SameIdHashEqual()
        {
            ScheduledPayment sp1 = new ScheduledPayment(id, planId, amount, dueDate, 50m, dueDate.AddDays(-3));
            ScheduledPayment sp2 = new ScheduledPayment(id, planId, amount, dueDate.AddDays(7), 50m, null); 
            Assert.AreEqual(sp1.GetHashCode(), sp2.GetHashCode());
        }

        [TestMethod]
        public void ScheduledPayment_GetHashCode_DifferentIdHashNotEqual()
        {
            ScheduledPayment sp1 = new ScheduledPayment(id, planId, amount, dueDate, 50m, dueDate.AddDays(-3));
            ScheduledPayment sp2 = new ScheduledPayment(id + "1", planId, amount, dueDate.AddDays(7), 50m, null);
            Assert.AreNotEqual(sp1.GetHashCode(), sp2.GetHashCode());
        }

        [TestMethod]
        public void ScheduledPayment_GetHashCode_HashNotEqual()
        {
            ScheduledPayment sp1 = new ScheduledPayment(null, null, amount, dueDate, 50m, dueDate.AddDays(-3));
            ScheduledPayment sp2 = new ScheduledPayment(id + "1", planId, amount, dueDate.AddDays(7), 50m, null);
            Assert.AreNotEqual(sp1.GetHashCode(), sp2.GetHashCode());
        }
    }
}
