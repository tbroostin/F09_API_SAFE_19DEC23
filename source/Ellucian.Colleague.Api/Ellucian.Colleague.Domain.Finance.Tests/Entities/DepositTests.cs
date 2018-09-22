// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class DepositTests
    {
        private string id;
        private string personId;
        private DateTime date;
        private string depositType;
        private decimal amount;
        private string receiptId;
        private string externalSystem;
        private string externalId;

        [TestInitialize]
        public void Initialize()
        {
            id = "123";
            personId = "1234567";
            date = DateTime.Now;
            depositType = "ROOM";
            amount = 100.00m;
            receiptId = "0000012345";
            externalSystem = "AHD";
            externalId = "ABC1234";
        }

        [TestMethod]
        public void Deposit_Constructor_NullId()
        {
            var dep = new Deposit(null, personId, date, depositType, amount);
            Assert.AreEqual(null, dep.Id);
        }

        [TestMethod]
        public void Deposit_Constructor_EmptyId()
        {
            var dep = new Deposit(string.Empty, personId, date, depositType, amount);
            Assert.AreEqual(string.Empty, dep.Id);
        }

        [TestMethod]
        public void Deposit_Constructor_ValidId()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            Assert.AreEqual(id, dep.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Deposit_Constructor_NullPersonId()
        {
            var dep = new Deposit(id, null, date, depositType, amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Deposit_Constructor_EmptyPersonId()
        {
            var dep = new Deposit(id, string.Empty, date, depositType, amount);
        }

        [TestMethod]
        public void Deposit_Constructor_ValidPersonId()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            Assert.AreEqual(dep.PersonId, personId);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Deposit_Constructor_DefaultDate()
        {
            var dep = new Deposit(id, personId, DateTime.MinValue, depositType, amount);
        }

        [TestMethod]
        public void Deposit_Constructor_ValidDate()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            Assert.AreEqual(date, dep.Date);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Deposit_Constructor_NullDepositType()
        {
            var dep = new Deposit(id, personId, date, null, amount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Deposit_Constructor_EmptyDepositType()
        {
            var dep = new Deposit(id, personId, date, string.Empty, amount);
        }

        [TestMethod]
        public void Deposit_Constructor_ValidDepositType()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            Assert.AreEqual(dep.DepositType, depositType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Deposit_Constructor_ZeroAmount()
        {
            var dep = new Deposit(id, personId, date, depositType, 0m);
        }

        [TestMethod]
        public void Deposit_Constructor_ValidAmount()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            Assert.AreEqual(dep.Amount, amount);
        }

        [TestMethod]
        public void Deposit_Id_ValidUpdate()
        {
            var dep = new Deposit(null, personId, date, depositType, amount);
            dep.Id = id;
            Assert.AreEqual(id, dep.Id);
        }

        [TestMethod]
        public void Deposit_Id_ValidUpdate2()
        {
            var dep = new Deposit(null, personId, date, depositType, amount);
            // ok to update with same value
            dep.Id = id;
            dep.Id = id;
            Assert.AreEqual(id, dep.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Deposit_Id_InvalidUpdate()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.Id = receiptId;
            dep.Id = "123321";
        }

        [TestMethod]
        public void Deposit_ReceiptId_ValidUpdate()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.ReceiptId = receiptId;
            Assert.AreEqual(receiptId, dep.ReceiptId);
        }

        [TestMethod]
        public void Deposit_ReceiptId_ValidUpdate2()
        {
            // ok to update wiht same value
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.ReceiptId = receiptId;
            dep.ReceiptId = receiptId;
            Assert.AreEqual(receiptId, dep.ReceiptId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Deposit_ReceiptId_InvalidUpdate()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.ReceiptId = receiptId;
            dep.ReceiptId = "123321";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Deposit_AddExternalSystemAndId_NullSystem()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.AddExternalSystemAndId(null, externalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Deposit_AddExternalSystemAndId_EmptySystem()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.AddExternalSystemAndId(string.Empty, externalId);
        }

        [TestMethod]
        public void Deposit_AddExternalSystemAndId_ValidSystem()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.AddExternalSystemAndId(externalSystem, externalId);
            Assert.AreEqual(externalSystem, dep.ExternalSystem);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Deposit_AddExternalSystemAndId_NullId()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.AddExternalSystemAndId(externalSystem, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Deposit_AddExternalSystemAndId_EmptyId()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.AddExternalSystemAndId(externalSystem, string.Empty);
        }

        [TestMethod]
        public void Deposit_AddExternalSystemAndId_ValidId()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.AddExternalSystemAndId(externalSystem, externalId);
            Assert.AreEqual(externalId, dep.ExternalIdentifier);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Deposit_AddExternalSystemAndId_NullSystemAndId()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            dep.AddExternalSystemAndId(null, null);
        }

        [TestMethod]
        public void Deposit_Equals_NullDeposit()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            Assert.IsFalse(dep.Equals(null));
        }

        [TestMethod]
        public void Deposit_Equals_NonDepositObject()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            var depDue = new DepositDue(id+"A", personId, amount, depositType, date.AddDays(7));
            Assert.IsFalse(dep.Equals(depDue));
        }

        [TestMethod]
        public void Deposit_Equals_SameId()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            var dep2 = new Deposit(id, personId, date.AddDays(1), depositType+"A", amount+100);
            Assert.IsTrue(dep.Equals(dep2));
        }

        [TestMethod]
        public void Deposit_Equals_DifferentId()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            var dep2 = new Deposit(id+"A", personId, date.AddDays(1), depositType + "A", amount + 100);
            Assert.IsFalse(dep.Equals(dep2));
        }

        [TestMethod]
        public void Deposit_Equals_OneNewDepositFirst()
        {
            var dep = new Deposit(null, personId, date, depositType, amount);
            var dep2 = new Deposit(id, personId, date.AddDays(1), depositType + "A", amount + 100);
            Assert.IsFalse(dep.Equals(dep2));
        }

        [TestMethod]
        public void Deposit_Equals_OneNewDepositSecond()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            var dep2 = new Deposit(null, personId, date.AddDays(1), depositType + "A", amount + 100);
            Assert.IsFalse(dep.Equals(dep2));
        }

        [TestMethod]
        public void Deposit_Equals_TwoNewDeposits()
        {
            var dep = new Deposit(null, personId, date, depositType, amount);
            var dep2 = new Deposit(null, personId, date.AddDays(1), depositType + "A", amount + 100);
            Assert.IsFalse(dep.Equals(dep2));
        }

        [TestMethod]
        public void Deposit_GetHashCode_SameIdHashEqual()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            var dep2 = new Deposit(id, personId+"A", date.AddDays(1), depositType + "A", amount + 100);
            Assert.AreEqual(dep.GetHashCode(), dep2.GetHashCode());
        }

        [TestMethod]
        public void Deposit_GetHashCode_DifferentIdHashNotEqual()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            var dep2 = new Deposit(id+"A", personId + "A", date.AddDays(1), depositType + "A", amount + 100);
            Assert.AreNotEqual(dep.GetHashCode(), dep2.GetHashCode());
        }

        [TestMethod]
        public void Deposit_GetHashCode_FirstDepositNewHashNotEqual()
        {
            var dep = new Deposit(null, personId, date, depositType, amount);
            var dep2 = new Deposit(id, personId, date, depositType, amount);
            Assert.AreNotEqual(dep.GetHashCode(), dep2.GetHashCode());
        }

        [TestMethod]
        public void Deposit_GetHashCode_SecondDepositNewHashNotEqual()
        {
            var dep = new Deposit(id, personId, date, depositType, amount);
            var dep2 = new Deposit(null, personId, date, depositType, amount);
            Assert.AreNotEqual(dep.GetHashCode(), dep2.GetHashCode());
        }

        [TestMethod]
        public void Deposit_GetHashCode_BothDepositsNewHashNotEqual()
        {
            var dep = new Deposit(null, personId, date, depositType, amount);
            var dep2 = new Deposit(null, personId, date, depositType, amount);
            Assert.AreNotEqual(dep.GetHashCode(), dep2.GetHashCode());
        }
    }
}
