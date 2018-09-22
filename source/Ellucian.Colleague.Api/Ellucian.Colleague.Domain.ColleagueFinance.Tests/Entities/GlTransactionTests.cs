// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GlTransactionTests
    {
        #region Initialize and Cleanup
        private GlTransactionBuilder Builder;

        [TestInitialize]
        public void Initialize()
        {
            Builder = new GlTransactionBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Builder = null;
        }
        #endregion

        [TestMethod]
        public void Constructor_Success()
        {
            // Initialize the GL transaction object.
            var glTransaction = Builder.Build();

            Assert.AreEqual(Builder.Id, glTransaction.Id);
            Assert.AreEqual(Builder.TransactionType, glTransaction.GlTransactionType);
            Assert.AreEqual(Builder.GlAccountNumber, glTransaction.GlAccount);
            Assert.AreEqual(Builder.Amount, glTransaction.Amount);
            Assert.AreEqual(Builder.ReferenceNumber, glTransaction.ReferenceNumber);
            Assert.AreEqual(Builder.TransactionDate, glTransaction.TransactionDate);
            Assert.AreEqual(Builder.Description, glTransaction.Description);
            Assert.AreEqual(Builder.Source, glTransaction.Source);

        }

        #region ID null checks
        [TestMethod]
        public void Constructor_NullId()
        {
            string expectedParameterName = "id";
            string actualParameterName = "";
            try
            {
                Builder.WithId(null).Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParameterName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParameterName, actualParameterName);
        }

        [TestMethod]
        public void Constructor_EmptyId()
        {
            string expectedParameterName = "id";
            string actualParameterName = "";
            try
            {
                Builder.WithId("").Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParameterName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParameterName, actualParameterName);
        }
        #endregion

        #region Source null checks
        [TestMethod]
        public void Constructor_NullSource()
        {
            string expectedParameterName = "source";
            string actualParameterName = "";
            try
            {
                Builder.WithSource(null).Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParameterName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParameterName, actualParameterName);
        }

        [TestMethod]
        public void Constructor_EmptySource()
        {
            string expectedParameterName = "source";
            string actualParameterName = "";
            try
            {
                Builder.WithSource("").Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParameterName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParameterName, actualParameterName);
        }
        #endregion

        #region GL account number null checks
        [TestMethod]
        public void Constructor_NullGlAccount()
        {
            string expectedParameterName = "glaccount";
            string actualParameterName = "";
            try
            {
                Builder.WithGlAccountNumber(null).Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParameterName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParameterName, actualParameterName);
        }

        [TestMethod]
        public void Constructor_EmptyGlAccount()
        {
            string expectedParameterName = "glaccount";
            string actualParameterName = "";
            try
            {
                Builder.WithGlAccountNumber("").Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParameterName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParameterName, actualParameterName);
        }
        #endregion

        #region Reference number null checks
        [TestMethod]
        public void Constructor_NullReferenceNumber()
        {
            string expectedParameterName = "referencenumber";
            string actualParameterName = "";
            try
            {
                Builder.WithReferenceNumber(null).Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParameterName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParameterName, actualParameterName);
        }

        [TestMethod]
        public void Constructor_EmptyReferenceNumber()
        {
            string expectedParameterName = "referencenumber";
            string actualParameterName = "";
            try
            {
                Builder.WithReferenceNumber("").Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParameterName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParameterName, actualParameterName);
        }
        #endregion
    }
}
