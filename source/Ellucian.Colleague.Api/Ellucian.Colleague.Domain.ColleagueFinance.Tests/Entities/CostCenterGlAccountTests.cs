// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class CostCenterGlAccountTests
    {
        #region Initialize and Cleanup
        CostCenterGlAccountBuilder glAccountBuilder;
        GlTransactionBuilder glTransactionBuilder;

        [TestInitialize]
        public void Initialize()
        {
            glAccountBuilder = new CostCenterGlAccountBuilder();
            glTransactionBuilder = new GlTransactionBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            glAccountBuilder = null;
            glTransactionBuilder = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullGlAccount()
        {
            var glAccount = glAccountBuilder.WithGlAccountNumber(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_EmptyGlAccount()
        {
            var glAccount = glAccountBuilder.WithGlAccountNumber("").Build();
        }

        [TestMethod]
        public void Constructor_Success()
        {
            var glAccount = glAccountBuilder.Build();
            Assert.AreEqual(glAccountBuilder.GlAccountEntity.GlAccountNumber, glAccount.GlAccountNumber);
            Assert.AreEqual(glAccountBuilder.PoolType, glAccount.PoolType);
            Assert.IsTrue(glAccount.Transactions is ReadOnlyCollection<GlTransaction>);
            Assert.AreEqual(0, glAccount.Transactions.Count);
        }
        #endregion

        #region Tests for GetFormattedGlAccount
        [TestMethod]
        public void GetFormattedGlAccount_LongGLNumber()
        {
            var glAccount = glAccountBuilder.Build();
            Assert.AreEqual(glAccountBuilder.GlAccountEntity.GlAccountNumber.Replace("_", "-"), glAccount.GetFormattedGlAccount(new List<string>()));
        }

        [TestMethod]
        public void GetFormattedGlAccount_GlNumberLengthExactly15()
        {
            string expectedGlAccountNumber = "01-02-0304050-6077";
            var startPositions = new List<string>(){ "1", "3", "5", "12" };
            var glAccount = glAccountBuilder.WithGlAccountNumber("010203040506077").Build();
            Assert.AreEqual(expectedGlAccountNumber, glAccount.GetFormattedGlAccount(startPositions));
        }

        [TestMethod]
        public void GetFormattedGlAccount_ShortGLNumber()
        {
            string expectedGlAccountNumber = "01-0203-0405";
            var startPositions = new List<string>(){ "1", "3", "7" };
            var glAccount = glAccountBuilder.WithGlAccountNumber("0102030405").Build();
            Assert.AreEqual(expectedGlAccountNumber, glAccount.GetFormattedGlAccount(startPositions));
        }

        [TestMethod]
        public void GetFormattedGlAccount_TypicalShortGlNumber()
        {
            string expectedGlAccountNumber = "10-0000-65030-01";
            var startPositions = new List<string>() { "1", "3", "7", "12" };
            var glAccount = glAccountBuilder.WithGlAccountNumber("1000006503001").Build();
            Assert.AreEqual(expectedGlAccountNumber, glAccount.GetFormattedGlAccount(startPositions));
        }

        [TestMethod]
        public void GetFormattedGlAccount_InvalidStartPosition()
        {
            string expectedGlAccountNumber = "010203-0405";
            var startPositions = new List<string>() { "0", "7" };
            var glAccount = glAccountBuilder.WithGlAccountNumber("0102030405").Build();
            Assert.AreEqual(expectedGlAccountNumber, glAccount.GetFormattedGlAccount(startPositions));
        }
        #endregion

        #region AddGlTransaction
        [TestMethod]
        public void AddGlTransaction_Success()
        {
            var glAccount = glAccountBuilder.Build();
            var glTransaction1 = glTransactionBuilder.WithTransactionType(GlTransactionType.Actual).Build();
            var glTransaction2 = glTransactionBuilder.WithId("V0038383").WithTransactionType(GlTransactionType.Actual).Build();
            var glTransaction3 = glTransactionBuilder.WithTransactionType(GlTransactionType.Encumbrance).WithReferenceNumber("P001").WithAmount(241.98m)
                .WithTransactionDate(new DateTime(2016, 2, 14)).Build();
            var glTransaction4 = glTransactionBuilder.WithTransactionType(GlTransactionType.Requisition).WithReferenceNumber("P001").WithAmount(104.82m)
                .WithTransactionDate(new DateTime(2016, 4, 19)).Build();
            var glTransaction5 = glTransactionBuilder.WithTransactionType(GlTransactionType.Requisition).WithReferenceNumber("P002").Build();

            // Transaction count should start at zero.
            Assert.AreEqual(0, glAccount.Transactions.Count);

            // Adding an actual transaction should increase the count to 1.
            glAccount.AddGlTransaction(glTransaction1);
            Assert.AreEqual(1, glAccount.Transactions.Count);

            // Adding a second actual transaction should increase the count to 2.
            glAccount.AddGlTransaction(glTransaction2);
            Assert.AreEqual(2, glAccount.Transactions.Count);

            // Adding an encumbrance/requisition transaction should increase the count to 3.
            glAccount.AddGlTransaction(glTransaction3);
            Assert.AreEqual(3, glAccount.Transactions.Count);

            // Adding another encumbrance/requisition transaction with the same reference number as the previous transaction should cause
            // the count to stay at 3, sum the amounts of the two transactions, and use the more recent transaction date. Also, we should
            // explicitly test that there is only one transaction with the reference number.
            var expectedAmount = glTransaction3.Amount + glTransaction4.Amount;
            glAccount.AddGlTransaction(glTransaction4);
            var actualTransactions = glAccount.Transactions.Where(x => x.ReferenceNumber == glTransaction3.ReferenceNumber).ToList();
            Assert.AreEqual(3, glAccount.Transactions.Count);
            Assert.AreEqual(1, actualTransactions.Count);
            Assert.AreEqual(expectedAmount, actualTransactions.First().Amount);
            Assert.AreEqual(glTransaction4.TransactionDate, actualTransactions.First().TransactionDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddGlTransaction_NullTransaction()
        {
            var glAccount = glAccountBuilder.Build();
            glAccount.AddGlTransaction(null);
        }

        [TestMethod]
        public void AddGlTransaction_DuplicateActualsTransaction()
        {
            var glAccount = glAccountBuilder.Build();
            var glTransaction = glTransactionBuilder.WithTransactionType(GlTransactionType.Actual).Build();

            // The count should start at zero.
            Assert.AreEqual(0, glAccount.Transactions.Count());

            // The count should increase to 1.
            glAccount.AddGlTransaction(glTransaction);
            Assert.AreEqual(1, glAccount.Transactions.Count());

            // The count should stay at 1.
            glAccount.AddGlTransaction(glTransaction);
            Assert.AreEqual(1, glAccount.Transactions.Count());
        }
        #endregion

        #region RemoveZeroDollarEncumbranceTransactions
        [TestMethod]
        public void RemoveZeroDollarEncumbranceTransactions()
        {
            var transaction1 = glTransactionBuilder.WithTransactionType(GlTransactionType.Encumbrance).WithReferenceNumber("P001").WithAmount(100m).Build();
            var transaction2 = glTransactionBuilder.WithTransactionType(GlTransactionType.Encumbrance).WithReferenceNumber("P002").WithAmount(0m).Build();
            var transaction3 = glTransactionBuilder.WithTransactionType(GlTransactionType.Requisition).WithReferenceNumber("P003").WithAmount(150m).Build();
            var transaction4 = glTransactionBuilder.WithTransactionType(GlTransactionType.Requisition).WithReferenceNumber("P004").WithAmount(0m).Build();

            var glAccount = glAccountBuilder.Build();
            glAccount.AddGlTransaction(transaction1);
            glAccount.AddGlTransaction(transaction2);
            glAccount.AddGlTransaction(transaction3);
            glAccount.AddGlTransaction(transaction4);

            // The transactions list should start with 4 items and end up with two.
            Assert.AreEqual(4, glAccount.Transactions.Count);

            // More specifically, there should be two encumbrance transactions and two requisition transactions before removing the zero dollar transactions.
            Assert.AreEqual(2, glAccount.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).Count());
            Assert.AreEqual(2, glAccount.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).Count());

            // Remove the zero-dollar transactions.
            glAccount.RemoveZeroDollarEncumbranceTransactions();
            
            // Now there should be two transactions left in the list.
            Assert.AreEqual(2, glAccount.Transactions.Count);

            // More specifically, there should be one encumbrance transaction and one requisition transaction left.
            Assert.AreEqual(1, glAccount.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Encumbrance).Count());
            Assert.AreEqual(1, glAccount.Transactions.Where(x => x.GlTransactionType == GlTransactionType.Requisition).Count());
        }
        #endregion
    }
}