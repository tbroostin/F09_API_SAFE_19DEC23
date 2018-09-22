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
    public class ReceiptPaymentTests
    {
        string id = "2946";
        DateTime date = DateTime.Parse("11/01/2013");
        string personId = "0008060";
        string receivableType = "01";
        string termId = "2013/FA";
        string referenceNumber = "2345432";
        decimal amount = 8765;
        string receiptId = "0000729";


        [TestMethod]
        public void PaymentReceipt_Constructor_ValidId()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, receiptId);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void PaymentReceipt_Constructor_ValidDate()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, receiptId);
            Assert.AreEqual(date, result.Date);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentReceipt_Constructor_NullPersonId()
        {
            var result = new ReceiptPayment(id, referenceNumber, null, receivableType, termId, date, amount, receiptId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentReceipt_Constructor_EmptyPersonId()
        {
            var result = new ReceiptPayment(id, referenceNumber, string.Empty, receivableType, termId, date, amount, receiptId);
        }

        [TestMethod]
        public void PaymentReceipt_Constructor_ValidPersonId()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, receiptId);
            Assert.AreEqual(personId, result.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentReceipt_Constructor_NullReceivableType()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, null, termId, date, amount, receiptId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentReceipt_Constructor_EmptyReceivableType()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, string.Empty, termId, date, amount, receiptId);
        }

        [TestMethod]
        public void PaymentReceipt_Constructor_ValidReceivableType()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, receiptId);
            Assert.AreEqual(receivableType, result.ReceivableType);
        }

        [TestMethod]
        public void PaymentReceipt_Constructor_ValidTermId()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, receiptId);
            Assert.AreEqual(termId, result.TermId);
        }

        [TestMethod]
        public void PaymentReceipt_Constructor_ValidReferenceNumber()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, receiptId);
            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        public void PaymentReceipt_Constructor_ValidAmount()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, receiptId);
            Assert.AreEqual(amount, result.Amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentReceipt_Constructor_NullReceiptId()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentReceipt_Constructor_EmptyReceiptId()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, string.Empty);
        }

        [TestMethod]
        public void PaymentReceipt_Constructor_ValidReceiptId()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, receiptId);
            Assert.AreEqual(receiptId, result.ReceiptId);
        }

        [TestMethod]
        public void PaymentReceipt_TransactionType()
        {
            var result = new ReceiptPayment(id, referenceNumber, personId, receivableType, termId, date, amount, receiptId);
            Assert.AreEqual(ReceivableTransactionType.ReceiptPayment, result.TransactionType);
        }
    }
}
