// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ReceivableChargeTests
    {
        static string id = "123";
        static decimal amount = 10000m;
        static List<string> description = new List<string>() {"Engl 101 01 Tuition"};
        static string arCode = "TUIFT";
        static string invoiceId = "12345";
        static string allocationId = "987";
        static string paymentPlanId = "654";
        static List<string> emptyString = new List<string>();

        [TestMethod]
        public void ReceivableCharge_Constructor_ValidId()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void ReceivableCharge_Constructor_ValidAmount_NoTaxAmount()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.AreEqual(amount, result.Amount);
        }

        [TestMethod]
        public void ReceivableCharge_Constructor_ValidAmount_TaxAmount()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount) { TaxAmount = (.05m * amount) };
            Assert.AreEqual(amount * 1.05m, result.Amount);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReceivableCharge_Constructor_SetIdAfterInitialize()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.Id = id;
        }

        [TestMethod]
        public void ReceivableCharge_Constructor_SetIdAfterInitialize_NullId()
        {
            var result = new ReceivableCharge(null, invoiceId, description, arCode, amount);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void ReceivableCharge_Constructor_SetIdAfterInitialize_EmptyId()
        {
            var result = new ReceivableCharge(string.Empty, invoiceId, description, arCode, amount);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void ReceivableCharge_Constructor_ValidInvoiceId()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.AreEqual(invoiceId, result.InvoiceId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReceivableCharge_Constructor_SetInvoiceIdAfterInitialize()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.InvoiceId = invoiceId;
        }

        [TestMethod]
        public void ReceivableCharge_Constructor_SetInvoiceIdAfterInitialize_NullInvoiceId()
        {
            var result = new ReceivableCharge(id, null, description, arCode, amount);
            result.InvoiceId = invoiceId;
            Assert.AreEqual(invoiceId, result.InvoiceId);
        }

        [TestMethod]
        public void ReceivableCharge_Constructor_SetInvoiceIdAfterInitialize_EmptyInvoiceId()
        {
            var result = new ReceivableCharge(id, string.Empty, description, arCode, amount);
            result.InvoiceId = invoiceId;
            Assert.AreEqual(invoiceId, result.InvoiceId);
        }

        [TestMethod]
        public void ReceivableCharge_Constructor_ValidDescription()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.AreEqual(description.Count, result.Description.Count);
            CollectionAssert.AllItemsAreInstancesOfType(result.Description, typeof(string));
            CollectionAssert.AreEqual(description, result.Description.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_Constructor_NullDescription()
        {
            var result = new ReceivableCharge(id, invoiceId, null, arCode, amount);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_Constructor_EmptyDescription()
        {
            var result = new ReceivableCharge(id, invoiceId, emptyString, arCode, amount);

        }

        [TestMethod]
        public void ReceivableCharge_Constructor_ValidChargeCode()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.AreEqual(arCode, result.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_Constructor_NullArCode()
        {
            var result = new ReceivableCharge(id, invoiceId, description, null, amount);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_Constructor_EmptyArCode()
        {
            var result = new ReceivableCharge(id, invoiceId, description, string.Empty, amount);

        }

        [TestMethod]
        public void ReceivableCharge_Constructor_ValidBaseAmount()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.AreEqual(amount, result.BaseAmount);
        }

        [TestMethod]
        public void ReceivableCharge_Constructor_ValidAmountNoTax()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.AreEqual(amount, result.Amount);
        }

        [TestMethod]
        public void ReceivableCharge_Constructor_ValidAmountWithTax()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.TaxAmount = 5m;
            Assert.AreEqual(amount + 5m, result.Amount);
        }

        [TestMethod]
        public void ReceivableCharge_Id_ValidId()
        {
            var result = new ReceivableCharge(null, invoiceId, description, arCode, amount);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void ReceivableCharge_InvoiceId_ValidInvoiceId()
        {
            var result = new ReceivableCharge(null, null, description, arCode, amount);
            result.InvoiceId = invoiceId;
            Assert.AreEqual(invoiceId, result.InvoiceId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_AddAllocation_NullAllocation()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.AddAllocation(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_AddAllocation_EmptyAllocation()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.AddAllocation(string.Empty);
        }

        [TestMethod]
        public void ReceivableCharge_AddAllocation_ValidAllocation()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.AddAllocation(allocationId);
            CollectionAssert.Contains(result.AllocationIds, allocationId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_RemoveAllocation_NullAllocation()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.RemoveAllocation(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_RemoveAllocation_EmptyAllocation()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.RemoveAllocation(string.Empty);
        }

        [TestMethod]
        public void ReceivableCharge_RemoveAllocation_ValidAllocation()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.AddAllocation(allocationId);
            result.RemoveAllocation(allocationId);
            CollectionAssert.DoesNotContain(result.AllocationIds, allocationId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_AddPaymentPlan_NullPaymentPlan()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.AddPaymentPlan(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_AddPaymentPlan_EmptyPaymentPlan()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.AddPaymentPlan(string.Empty);
        }

        [TestMethod]
        public void ReceivableCharge_AddPaymentPlan_ValidPaymentPlan()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.AddPaymentPlan(paymentPlanId);
            CollectionAssert.Contains(result.PaymentPlanIds, paymentPlanId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_RemovePaymentPlan_NullPaymentPlan()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.RemovePaymentPlan(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivableCharge_RemovePaymentPlan_EmptyPaymentPlan()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.RemovePaymentPlan(string.Empty);
        }

        [TestMethod]
        public void ReceivableCharge_RemovePaymentPlan_ValidPaymentPlan()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            result.AddPaymentPlan(paymentPlanId);
            result.RemovePaymentPlan(paymentPlanId);
            CollectionAssert.DoesNotContain(result.PaymentPlanIds, paymentPlanId);
        }

        [TestMethod]
        public void ReceivableCharge_Equals_ThisIdNull()
        {
            var result = new ReceivableCharge(null, invoiceId, description, arCode, amount);
            var other = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.IsFalse(result.Equals(other));
        }

        [TestMethod]
        public void ReceivableCharge_Equals_ThisIdEmpty()
        {
            var result = new ReceivableCharge(string.Empty, invoiceId, description, arCode, amount);
            var other = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.IsFalse(result.Equals(other));
        }

        [TestMethod]
        public void ReceivableCharge_Equals_CompareToNull()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.IsFalse(result.Equals(null));
        }

        [TestMethod]
        public void ReceivableCharge_Equals_CompareToOtherTypeObject()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.IsFalse(result.Equals("test"));
        }

        [TestMethod]
        public void ReceivableCharge_Equals_OtherIdNull()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            var other = new ReceivableCharge(null, invoiceId, description, arCode, amount);
            Assert.IsFalse(result.Equals(other));
        }

        [TestMethod]
        public void ReceivableCharge_Equals_OtherIdEmpty()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            var other = new ReceivableCharge(string.Empty, invoiceId, description, arCode, amount);
            Assert.IsFalse(result.Equals(other));
        }

        [TestMethod]
        public void ReceivableCharge_Equals_IdsDifferent()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            var other = new ReceivableCharge("12345", invoiceId, description, arCode, amount);
            Assert.IsFalse(result.Equals(other));
        }

        [TestMethod]
        public void ReceivableCharge_Equals_IdsSameNotNull()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            var other = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.IsTrue(result.Equals(other));
        }

        [TestMethod]
        public void ReceivableCharge_GetHashCode_IdNull()
        {
            var result = new ReceivableCharge(null, invoiceId, description, arCode, amount);
            Assert.IsInstanceOfType(result.GetHashCode(), typeof(int));
        }

        [TestMethod]
        public void ReceivableCharge_GetHashCode_IdEmpty()
        {
            var result = new ReceivableCharge(string.Empty, invoiceId, description, arCode, amount);
            Assert.IsInstanceOfType(result.GetHashCode(), typeof(int));
        }

        [TestMethod]
        public void ReceivableCharge_Equals_IdNotNull()
        {
            var result = new ReceivableCharge(id, invoiceId, description, arCode, amount);
            Assert.IsInstanceOfType(result.GetHashCode(), typeof(int));
        }
    }
}
