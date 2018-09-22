// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ReceivablePaymentTests
    {
        string id = "123456789";
        string personId = "1234567";
        string receivableType = "01";
        string term = "2015/FA";
        string referenceNumber = "000067890";
        DateTime date = DateTime.Parse("07/31/2015");
        static decimal amount = 10000m;
        static string receiptId = "0001234567";

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public void ReceivablePayment_Constructor_ValidId()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, term, date, amount, receiptId);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReceivablePayment_IdAlreadySet()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, term, date, amount, receiptId);
            result.Id = "foo";
        }

        [TestMethod]
        public void ReceivablePayment_Constructor_ReferenceNumberValid()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, term, date, amount, receiptId);
            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReceivablePayment_ReferenceNumberAlreadySet()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, term, date, amount, receiptId);
            result.Id = "foo";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivablePayment_Constructor_NullPersonId()
        {
            var result = new ReceiptPayment(id, referenceNumber, null, receivableType, term, date, amount, receiptId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivablePayment_Constructor_EmptyPersonId()
        {
            var result = new ReceiptPayment(id, referenceNumber, string.Empty, receivableType, term, date, amount, receiptId);
        }

        [TestMethod]
        public void ReceivablePayment_Constructor_PersonIdValid()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, term, date, amount, receiptId);
            Assert.AreEqual(personId, result.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivablePayment_Constructor_NullReceivableType()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, null, term, date, amount, receiptId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceivablePayment_Constructor_EmptyReceivableType()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, string.Empty, term, date, amount, receiptId);
        }

        [TestMethod]
        public void ReceivablePayment_Constructor_ReceivableTypeValid()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, term, date, amount, receiptId);
            Assert.AreEqual(receivableType, result.ReceivableType);
        }

        [TestMethod]
        public void ReceivablePayment_Constructor_NullTermValid()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, null, date, amount, receiptId);
            Assert.AreEqual(null, result.TermId);
        }

        [TestMethod]
        public void ReceivablePayment_Constructor_EmptyTermValid()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, string.Empty, date, amount, receiptId);
            Assert.AreEqual(String.Empty, result.TermId);
        }

        [TestMethod]
        public void ReceivablePayment_Constructor_TermValid()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, term, date, amount, receiptId);
            Assert.AreEqual(term, result.TermId);
        }

        [TestMethod]
        public void ReceivablePayment_Constructor_DateValid()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, term, date, amount, receiptId);
            Assert.AreEqual(date, result.Date);
        }

        [TestMethod]
        public void ReceivablePayment_Amount_Valid()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, term, date, amount, receiptId);
            Assert.AreEqual(amount, result.Amount);
        }
    }
}
