using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ChargeTests
    {
        string id = "12345";
        string invoiceId = "678901234";
        List<String> description = new List<String>() { "Materials Fee" };
        string code = "MATFE";
        decimal amount = 100m;
        decimal tax = 5m;
        List<String> allocationIds = new List<String>();
        List<String> paymentPlanIds = new List<String>();

        string allocation1 = "123";
        string paymentPlan1 = "456";

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Charge_Constructor_NullId()
        {
            var result = new Charge(null, invoiceId, description, code, amount);
        }

        [TestMethod]
        public void Charge_Constructor_ValidId()
        {
            var result = new Charge(id, invoiceId, description, code, amount);

            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Charge_Constructor_NullInvoiceId()
        {
            var result = new Charge(id, null, description, code, amount);
        }

        [TestMethod]
        public void Charge_Constructor_ValidInvoiceId()
        {
            var result = new Charge(id, invoiceId, description, code, amount);

            Assert.AreEqual(invoiceId, result.InvoiceId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Charge_Constructor_NullDescription()
        {
            var result = new Charge(id, invoiceId, null, code, amount);
        }

        [TestMethod]
        public void Charge_Constructor_ValidDescription()
        {
            var result = new Charge(id, invoiceId, description, code, amount);

            CollectionAssert.AreEqual(description, result.Description.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Charge_Constructor_NullCode()
        {
            var result = new Charge(id, invoiceId, description, null, amount);
        }

        [TestMethod]
        public void Charge_Constructor_ValidCode()
        {
            var result = new Charge(id, invoiceId, description, code, amount);

            Assert.AreEqual(code, result.Code);
        }

        [TestMethod]
        public void Charge_Constructor_ValidAmount()
        {
            var result = new Charge(id, invoiceId, description, code, amount);

            Assert.AreEqual(amount, result.BaseAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Charge_AddAllocation_NullId()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            result.AddAllocation(null);
        }

        [TestMethod]
        public void Charge_AddAllocation_Valid()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            result.AddAllocation(allocation1);

            Assert.IsNotNull(result.AllocationIds);
            Assert.AreEqual(allocation1, result.AllocationIds.ElementAt(0));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Charge_AddPaymentPlan_NullId()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            result.AddPaymentPlan(null);
        }

        [TestMethod]
        public void Charge_AddPaymentPlan_Valid()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            result.AddPaymentPlan(paymentPlan1);

            Assert.IsNotNull(result.PaymentPlanIds);
            Assert.AreEqual(paymentPlan1, result.PaymentPlanIds.ElementAt(0));
        }

        [TestMethod]
        public void Charge_Amount_ZeroTaxAmount()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            result.TaxAmount = 0m;

            Assert.AreEqual(amount, result.Amount);
        }

        [TestMethod]
        public void Charge_Amount_NonZeroTaxAmount()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            result.TaxAmount = tax;

            Assert.AreEqual(amount + tax, result.Amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Charge_RemoveAllocation_NullId()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            List<String> allocationIds = new List<String>() { "123", "456" };
            foreach (string allocationId in allocationIds)
            {
                result.AddAllocation(allocationId);
            }
            result.RemoveAllocation(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Charge_RemoveAllocation_EmptyId()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            List<String> allocationIds = new List<String>() { "123", "456" };
            foreach (string allocationId in allocationIds)
            {
                result.AddAllocation(allocationId);
            }
            result.RemoveAllocation(string.Empty);
        }

        [TestMethod]
        public void Charge_RemoveAllocation_Valid()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            List<String> allocationIds = new List<String>() { "123", "456" };
            foreach (string allocationId in allocationIds)
            {
                result.AddAllocation(allocationId);
            }
            result.RemoveAllocation(allocationIds[0]);

            Assert.IsNotNull(result.AllocationIds);
            Assert.AreEqual(allocationIds[1], result.AllocationIds.ElementAt(0));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Charge_RemovePaymentPlan_NullId()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            List<String> paymentPlanIds = new List<String>() { "123", "456" };
            foreach (string planId in paymentPlanIds)
            {
                result.AddPaymentPlan(planId);
            }
            result.RemovePaymentPlan(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Charge_RemovePaymentPlan_EmptyId()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            List<String> paymentPlanIds = new List<String>() { "123", "456" };
            foreach (string planId in paymentPlanIds)
            {
                result.AddPaymentPlan(planId);
            }
            result.RemovePaymentPlan(string.Empty);
        }

        [TestMethod]
        public void Charge_RemovePaymentPlan_Valid()
        {
            var result = new Charge(id, invoiceId, description, code, amount);
            List<String> paymentPlanIds = new List<String>() { "123", "456" };
            foreach (string planId in paymentPlanIds)
            {
                result.AddPaymentPlan(planId);
            }
            result.RemovePaymentPlan(paymentPlanIds[0]);

            Assert.IsNotNull(result.PaymentPlanIds);
            Assert.AreEqual(paymentPlanIds[1], result.PaymentPlanIds.ElementAt(0));
        }

        [TestMethod]
        public void Charge_Equals_NullObject()
        {
            var original = new Charge(id, invoiceId, description, code, amount);
            object newCharge = null;
            Assert.IsFalse(original.Equals(newCharge));
        }

        [TestMethod]
        public void Charge_Equals_NullCharge()
        {
            var original = new Charge(id, invoiceId, description, code, amount);
            Charge newCharge = null;
            Assert.IsFalse(original.Equals(newCharge));
        }

        [TestMethod]
        public void Charge_Equals_NonChargeObject()
        {
            var original = new Charge(id, invoiceId, description, code, amount);
            ChargeCode chargeCode = new ChargeCode("code", "description", null);
            Assert.IsFalse(original.Equals(chargeCode));
        }

        [TestMethod]
        public void Charge_Equals_IdMismatch()
        {
            var original = new Charge(id, invoiceId, description, code, amount);
            var newCharge = new Charge("23456", invoiceId, description, code, amount / 2);
            Assert.IsFalse(original.Equals(newCharge));
        }

        [TestMethod]
        public void Charge_Equals_Valid()
        {
            var original = new Charge(id, invoiceId, description, code, amount);
            var newCharge = new Charge(id, invoiceId, description, code, amount / 2);
            Assert.IsTrue(original.Equals(newCharge));
        }
       
        [TestMethod]
        public void Charge_GetHashCode_SameCodeHashEqual()
        {
            var charge1 = new Charge(id, invoiceId, description, code, amount);
            var charge2 = new Charge(id, invoiceId, description, code+"A", amount+5);
            Assert.AreEqual(charge1.GetHashCode(), charge2.GetHashCode());
        }

        [TestMethod]
        public void Charge_GetHashCode_DifferentCodeHashNotEqual()
        {
            var charge1 = new Charge(id, invoiceId, description, code, amount);
            var charge2 = new Charge(id+"1", invoiceId, description, code + "A", amount + 5);
            Assert.AreNotEqual(charge1.GetHashCode(), charge2.GetHashCode());
        }
    }
}
