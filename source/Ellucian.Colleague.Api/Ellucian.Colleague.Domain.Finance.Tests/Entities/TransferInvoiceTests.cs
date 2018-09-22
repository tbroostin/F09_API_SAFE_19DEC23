using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class TransferInvoiceTests
    {
        string id;
        string personId;
        string receivableType;
        string termId;
        string referenceNumber;
        DateTime date;
        decimal amount;
        string invoiceId;

        [TestInitialize]
        public void Initialize()
        {
            id = "123";
            personId = "1234567";
            receivableType = "01";
            termId = "2014/SP";
            referenceNumber = "23456";
            date = DateTime.Now;
            amount = 100.00m;
            invoiceId = "234";
        }

        [TestMethod]
        public void TransferInvoice_Constructor_ValidId()
        {
            var result = new TransferInvoice(id, referenceNumber, personId, receivableType, termId, date, amount, invoiceId);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransferInvoice_Constructor_NullPersonId()
        {
            var result = new TransferInvoice(id, referenceNumber, null, receivableType, termId, date, amount, invoiceId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransferInvoice_Constructor_EmptyPersonId()
        {
            var result = new TransferInvoice(id, referenceNumber, string.Empty, receivableType, termId, date, amount, invoiceId);
        }

        [TestMethod]
        public void TransferInvoice_Constructor_ValidPersonId()
        {
            var result = new TransferInvoice(id, referenceNumber, personId, receivableType, termId, date, amount, invoiceId);
            Assert.AreEqual(personId, result.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransferInvoice_Constructor_NullReceivableType()
        {
            var result = new TransferInvoice(id, referenceNumber, personId, null, termId, date, amount, invoiceId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransferInvoice_Constructor_EmptyReceivableType()
        {
            var result = new TransferInvoice(id, referenceNumber, personId, string.Empty, termId, date, amount, invoiceId);
        }

        [TestMethod]
        public void TransferInvoice_Constructor_ValidReceivableType()
        {
            var result = new TransferInvoice(id, referenceNumber, personId, receivableType, termId, date, amount, invoiceId);
            Assert.AreEqual(receivableType, result.ReceivableType);
        }

        [TestMethod]
        public void TransferInvoice_Constructor_ValidReferenceNumber()
        {
            var result = new TransferInvoice(id, referenceNumber, personId, receivableType, termId, date, amount, invoiceId);
            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        public void TransferInvoice_Constructor_ValidDepositId()
        {
            var result = new TransferInvoice(id, referenceNumber, personId, receivableType, termId, date, amount, invoiceId);
            Assert.AreEqual(invoiceId, result.InvoiceId);
        }

        [TestMethod]
        public void TransferInvoice_TransactionType()
        {
            var result = new TransferInvoice(id, referenceNumber, personId, receivableType, termId, date, amount, invoiceId);
            Assert.AreEqual(ReceivableTransactionType.Transfer, result.TransactionType);
        }
    }
}