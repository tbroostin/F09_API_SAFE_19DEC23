// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GlObjectCodeGlAccountTests
    {
        #region Initialize and Cleanup
        GlObjectCodeGlAccount glAccount;
        GlTransactionBuilder glTransactionBuilder;
        private string glAccountId = "10_00_00_01_20601_51000";
        private GlBudgetPoolType poolType = GlBudgetPoolType.Poolee;

        [TestInitialize]
        public void Initialize()
        {
            glAccount = new GlObjectCodeGlAccount(glAccountId, poolType);
            glTransactionBuilder = new GlTransactionBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            glAccount = null;
            glTransactionBuilder = null;
        }
        #endregion

        #region Constructor
        [TestMethod]
        public void Constructor()
        {
            Assert.AreEqual(glAccountId, glAccount.GlAccountNumber);
            Assert.AreEqual(poolType, glAccount.PoolType);
            Assert.AreEqual(0, glAccount.Transactions.Count);
        }
        #endregion

        #region AddGlTransaction
        [TestMethod]
        public void AddGlTransaction_Success()
        {
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
            glAccount.AddGlTransaction(null);
        }

        [TestMethod]
        public void AddGlTransaction_DuplicateActualsTransaction()
        {
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