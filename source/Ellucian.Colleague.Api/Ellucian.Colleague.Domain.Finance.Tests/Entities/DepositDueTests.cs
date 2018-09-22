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
    public class DepositDueTests
    {
        string id = "1234";
        string personId = "1234567";
        decimal amount = 500.00m;
        string depositType = "ROOM";
        DateTime dueDate = DateTime.Parse("08/01/2012");

        string depositId1 = "120";
        decimal depositAmount1 = 125.00m;
        DateTime depositDate1 = DateTime.Parse("02/01/2012");
        string depositId2 = "121";
        decimal depositAmount2 = 250.00m;
        DateTime depositDate2 = DateTime.Parse("03/01/2012");

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositDue_Constructor_NullId()
        {
            DepositDue result = new DepositDue(null, personId, amount, depositType, dueDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositDue_Constructor_EmptyId()
        {
            DepositDue result = new DepositDue(string.Empty, personId, amount, depositType, dueDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositDue_Constructor_NullPersonId()
        {
            DepositDue result = new DepositDue(id, null, amount, depositType, dueDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositDue_Constructor_EmptyPersonId()
        {
            DepositDue result = new DepositDue(id, string.Empty, amount, depositType, dueDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DepositDue_Constructor_NegativeAmount()
        {
            DepositDue result = new DepositDue(id, personId, -1m, depositType, dueDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositDue_Constructor_NullDepositType()
        {
            DepositDue result = new DepositDue(id, personId, amount, null, dueDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositDue_Constructor_EmptyDepositType()
        {
            DepositDue result = new DepositDue(id, personId, amount, string.Empty, dueDate);
        }

        [TestMethod]
        public void DepositDue_Constructor_Valid()
        {
            DepositDue result = new DepositDue(id, personId, amount, depositType, dueDate);

            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(personId, result.PersonId);
            Assert.AreEqual(amount, result.Amount);
            Assert.AreEqual(depositType, result.DepositType);
            Assert.AreEqual(dueDate, result.DueDate);

            Assert.AreEqual(0, result.Deposits.Count());

            Assert.AreEqual(string.Empty, result.DepositTypeDescription);
            Assert.AreEqual(string.Empty, result.TermDescription);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepositDue_AddDepositMethod_NullDeposit()
        {
            DepositDue result = new DepositDue(id, personId, amount, depositType, dueDate);
            result.AddDeposit(null);
        }

        [TestMethod]
        public void DepositDue_AddDeposit_Valid()
        {
            Deposit dep = new Deposit(depositId1, personId, depositDate1, depositType, depositAmount1);
            DepositDue result = new DepositDue(id, personId, amount, depositType, dueDate);
            result.AddDeposit(dep);

            Assert.AreNotEqual(result.Deposits, null);
            Assert.AreEqual(result.Deposits.Count(), 1);
            Assert.AreEqual(result.Deposits.ElementAt(0), dep);
        }

        [TestMethod]
        public void DepositDue_AddDeposit_DuplicateDeposit()
        {
            Deposit dep = new Deposit(depositId1, personId, depositDate1, depositType, depositAmount1);
            DepositDue result = new DepositDue(id, personId, amount, depositType, dueDate);
            result.AddDeposit(dep);
            result.AddDeposit(dep);

            Assert.AreEqual(1, result.Deposits.Count());
            Assert.AreEqual(dep, result.Deposits.ElementAt(0));
        }

        [TestMethod]
        public void DepositDue_BalanceProperty()
        {
            DepositDue result = new DepositDue(id, personId, amount, depositType, dueDate);
            result.AddDeposit(new Deposit(depositId1, personId, depositDate1, depositType, depositAmount1));
            result.AddDeposit(new Deposit(depositId2, personId, depositDate2, depositType, depositAmount2));

            Assert.AreEqual((amount - depositAmount1 - depositAmount2), result.Balance);
        }

        [TestMethod]
        public void DepositDue_SortOrderProperty()
        {
            DepositDue result = new DepositDue(id, personId, amount, depositType, dueDate);
            Assert.AreEqual(dueDate.ToString("s") + depositType.PadRight(5) + id.PadLeft(20, '0'), result.SortOrder);
        }

        [TestMethod]
        public void DepositDue_OverdueProperty_PositiveBalanceAndPastDueDate()
        {
            DepositDue result = new DepositDue(id, personId, amount, depositType, dueDate);
            Assert.IsTrue(result.Overdue);
        }

        [TestMethod]
        public void DepositDue_OverdueProperty_PositiveBalanceAndFutureDueDate()
        {
            DepositDue result = new DepositDue(id, personId, amount, depositType, DateTime.Today.AddDays(5));
            Assert.IsFalse(result.Overdue);
        }

        [TestMethod]
        public void DepositDue_AmountPaid_NoDeposits()
        {
            DepositDue result = new DepositDue(id, personId, 0, depositType, DateTime.Today.AddDays(5));
            Assert.AreEqual(0, result.AmountPaid);
        }

        [TestMethod]
        public void DepositDue_AmountPaid_Deposits()
        {
            Deposit dep = new Deposit(depositId1, personId, depositDate1, depositType, depositAmount1);
            DepositDue result = new DepositDue(id, personId, amount, depositType, dueDate);
            result.AddDeposit(dep);

            Assert.AreEqual(dep.Amount, result.AmountPaid);
        }

        [TestMethod]
        public void DepositDue_Equals_NullDeposit()
        {
            DepositDue dep = new DepositDue(id, personId, amount, depositType, dueDate);
            Deposit dep2 = null;
            Assert.IsFalse(dep.Equals(dep2));
        }

        [TestMethod]
        public void DepositDue_Equals_NonDepositObject()
        {
            DepositDue depDue = new DepositDue(id, personId, amount, depositType, dueDate);
            Deposit dep = new Deposit(id + "A", personId, dueDate, depositType, amount);
            Assert.IsFalse(depDue.Equals(dep));
        }

        [TestMethod]
        public void DepositDue_Equals_SameId()
        {
            DepositDue dep = new DepositDue(id, personId, amount, depositType, dueDate);
            DepositDue dep2 = new DepositDue(id, personId, amount + 100, depositType + "A", dueDate.AddDays(1));
            Assert.IsTrue(dep.Equals(dep2));
        }

        [TestMethod]
        public void DepositDue_Equals_DifferentId()
        {
            DepositDue dep = new DepositDue(id, personId, amount, depositType, dueDate);
            DepositDue dep2 = new DepositDue(id + "A", personId, amount + 100, depositType + "A", dueDate.AddDays(1));
            Assert.IsFalse(dep.Equals(dep2));
        }

        [TestMethod]
        public void DepositDue_GetHashCode_SameCodeHashEqual()
        {
            DepositDue dep = new DepositDue(id, personId, amount, depositType, dueDate);
            DepositDue dep2 = new DepositDue(id, personId, amount + 100, depositType + "A", dueDate.AddDays(1));
            Assert.AreEqual(dep.GetHashCode(), dep2.GetHashCode());
        }

        [TestMethod]
        public void DepositDue_GetHashCode_DifferentCodeHashNotEqual()
        {
            DepositDue dep = new DepositDue(id, personId, amount, depositType, dueDate);
            DepositDue dep2 = new DepositDue(id + "A", personId, amount + 100, depositType + "A", dueDate.AddDays(1));
            Assert.AreNotEqual(dep.GetHashCode(), dep2.GetHashCode());
        }
    }
}
