// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class DepositAllocationTests
    {
        string id;
        string personId;
        string receivableType;
        string termId;
        string referenceNumber;
        DateTime date;
        decimal amount;
        string depositId;

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
            depositId = "234";
        }

        [TestMethod]
        public void DepositAllocation_Constructor_ValidId()
        {
            var result = new DepositAllocation(id, referenceNumber, personId, receivableType, termId, date, amount, depositId);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositAllocation_Constructor_NullPersonId()
        {
            var result = new DepositAllocation(id, referenceNumber, null, receivableType, termId, date, amount, depositId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositAllocation_Constructor_EmptyPersonId()
        {
            var result = new DepositAllocation(id, referenceNumber, string.Empty, receivableType, termId, date, amount, depositId);
        }

        [TestMethod]
        public void DepositAllocation_Constructor_ValidPersonId()
        {
            var result = new DepositAllocation(id, referenceNumber, personId, receivableType, termId, date, amount, depositId);
            Assert.AreEqual(personId, result.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositAllocation_Constructor_NullReceivableType()
        {
            var result = new DepositAllocation(id, referenceNumber, personId, null, termId, date, amount, depositId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositAllocation_Constructor_EmptyReceivableType()
        {
            var result = new DepositAllocation(id, referenceNumber, personId, string.Empty, termId, date, amount, depositId);
        }

        [TestMethod]
        public void DepositAllocation_Constructor_ValidReceivableType()
        {
            var result = new DepositAllocation(id, referenceNumber, personId, receivableType, termId, date, amount, depositId);
            Assert.AreEqual(receivableType, result.ReceivableType);
        }

        [TestMethod]
        public void DepositAllocation_Constructor_ValidReferenceNumber()
        {
            var result = new DepositAllocation(id, referenceNumber, personId, receivableType, termId, date, amount, depositId);
            Assert.AreEqual(referenceNumber, result.ReferenceNumber);
        }

        [TestMethod]
        public void DepositAllocation_Constructor_ValidDepositId()
        {
            var result = new DepositAllocation(id, referenceNumber, personId, receivableType, termId, date, amount, depositId);
            Assert.AreEqual(depositId, result.DepositId);
        }

        [TestMethod]
        public void DepositAllocation_TransactionType()
        {
            var result = new DepositAllocation(id, referenceNumber, personId, receivableType, termId, date, amount, depositId);
            Assert.AreEqual(ReceivableTransactionType.DepositAllocation, result.TransactionType);
        }
    }
}