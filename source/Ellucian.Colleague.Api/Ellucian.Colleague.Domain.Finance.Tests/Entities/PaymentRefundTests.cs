// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentRefundTests
    {
        const string id = "12345";
        const string personId = "0003315";
        const string receivableType = "01";
        const string termId = "2014/FA";
        const string referenceNumber = "23456";
        static readonly DateTime date = DateTime.Today.AddDays(-3);
        const decimal amount = 1500;
        const string reason = "OVCHG";

        readonly PaymentRefund rfnd = new PaymentRefund(id, referenceNumber, personId, receivableType, termId, date, amount, reason);
        
        [TestMethod]
        public void PaymentRefund_Constructor_ValidId()
        {
            Assert.AreEqual(id, rfnd.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentRefund_Constructor_NullPersonId()
        {
            var pmt = new PaymentRefund(id, referenceNumber, null, receivableType, termId, date, amount, reason);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentRefund_Constructor_EmptyPersonId()
        {
            var pmt = new PaymentRefund(id, referenceNumber, string.Empty, receivableType, termId, date, amount, reason);
        }

        [TestMethod]
        public void PaymentRefund_Constructor_ValidPersonId()
        {
            Assert.AreEqual(personId, rfnd.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentRefund_Constructor_NullReceivableType()
        {
            var pmt = new PaymentRefund(id, referenceNumber, personId, null, termId, date, amount, reason);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentRefund_Constructor_EmptyReceivableType()
        {
            var pmt = new PaymentRefund(id, referenceNumber, personId, string.Empty, termId, date, amount, reason);
        }

        [TestMethod]
        public void PaymentRefund_Constructor_ValidReceivableType()
        {
            Assert.AreEqual(receivableType, rfnd.ReceivableType);
        }

        [TestMethod]
        public void PaymentRefund_Constructor_ValidReferenceNumber()
        {
            Assert.AreEqual(referenceNumber, rfnd.ReferenceNumber);
        }

        [TestMethod]
        public void PaymentRefund_Constructor_ValidReason()
        {
            Assert.AreEqual(reason, rfnd.Reason);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentRefund_AddAllocations_Null()
        {
            rfnd.AddAllocation(null);
        }

        [TestMethod]
        public void PaymentRefund_AddAllocations_Valid()
        {
            var alloc = new PaymentAllocation("111", "222", PaymentAllocationSource.System, 25m);
            rfnd.AddAllocation(alloc);
            Assert.AreEqual(1, rfnd.Allocations.Count);
            Assert.AreEqual(alloc, rfnd.Allocations[0]);
        }

        [TestMethod]
        public void PaymentRefund_TransactionType()
        {
            Assert.AreEqual(ReceivableTransactionType.Refund, rfnd.TransactionType);
        }
    }
}
