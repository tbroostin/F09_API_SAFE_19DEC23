// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GeneralLedgerClassConfigurationTests
    {
        #region Initialize and Cleanup
        public GeneralLedgerClassConfigurationBuilder Builder;

        [TestInitialize]
        public void Initialize()
        {
            Builder = new GeneralLedgerClassConfigurationBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Builder = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        public void Constructor_NullClassificationName()
        {
            var expectedParamName = "classname";
            var actualParamName = "";
            try
            {
                Builder.WithClassificationName(null).Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public void Constructor_EmptyClassificationName()
        {
            var expectedParamName = "classname";
            var actualParamName = "";
            try
            {
                Builder.WithClassificationName("").Build();
            }
            catch (ArgumentNullException anex)
            {
                actualParamName = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParamName, actualParamName);
        }

        #region Expense Class Values
        [TestMethod]
        public void Constructor_NullExpenseClassValues()
        {
            var glClassConfiguration = Builder.WithClassificationName("GL.CLASS")
                .WithExpenseClassValues(null).Build();
            Assert.AreEqual(0, glClassConfiguration.ExpenseClassValues.Count);
        }

        [TestMethod]
        public void Constructor_EmptyExpenseClassValues()
        {
            var glClassConfiguration = Builder.WithClassificationName("GL.CLASS")
                .WithExpenseClassValues(new List<string>()).Build();
            Assert.AreEqual(0, glClassConfiguration.ExpenseClassValues.Count);
        }

        [TestMethod]
        public void Constructor_ExpenseClassValuesContainsNull()
        {
            var expenseValues = new List<string>() { "5", null, "7" };
            var glClassDefinition = Builder.WithClassificationName("GL.CLASS")
                .WithExpenseClassValues(expenseValues).Build();
            Assert.AreEqual(expenseValues.Count - 1, glClassDefinition.ExpenseClassValues.Count);
            foreach (var value in glClassDefinition.ExpenseClassValues)
            {
                Assert.IsTrue(value == "5" || value == "7");
            }
        }

        [TestMethod]
        public void Constructor_ExpenseClassValuesContainsEmpty()
        {
            var expenseValues = new List<string>() { "5", "", "7" };
            var glClassDefinition = Builder.WithClassificationName("GL.CLASS")
                .WithExpenseClassValues(expenseValues).Build();
            Assert.AreEqual(expenseValues.Count - 1, glClassDefinition.ExpenseClassValues.Count);
            foreach (var value in glClassDefinition.ExpenseClassValues)
            {
                Assert.IsTrue(value == "5" || value == "7");
            }
        }
        #endregion

        #region Revenue Class Values
        [TestMethod]
        public void Constructor_NullRevenueClassValues()
        {
            var glClassConfiguration = Builder.WithClassificationName("GL.CLASS")
                .WithRevenueClassValues(null).Build();
            Assert.AreEqual(0, glClassConfiguration.RevenueClassValues.Count);
        }

        [TestMethod]
        public void Constructor_EmptyRevenueClassValues()
        {
            var glClassConfiguration = Builder.WithClassificationName("GL.CLASS")
                .WithRevenueClassValues(new List<string>()).Build();
            Assert.AreEqual(0, glClassConfiguration.RevenueClassValues.Count);
        }

        [TestMethod]
        public void Constructor_RevenueClassValuesContainsNull()
        {
            var revenueValues = new List<string>() { "4", null, "6" };
            var glClassDefinition = Builder.WithClassificationName("GL.CLASS")
                .WithRevenueClassValues(revenueValues).Build();
            Assert.AreEqual(revenueValues.Count - 1, glClassDefinition.RevenueClassValues.Count);
            foreach (var value in glClassDefinition.RevenueClassValues)
            {
                Assert.IsTrue(value == "4" || value == "6");
            }
        }

        [TestMethod]
        public void Constructor_RevenueClassValuesContainsEmpty()
        {
            var revenueValues = new List<string>() { "4", "", "6" };
            var glClassDefinition = Builder.WithClassificationName("GL.CLASS")
                .WithRevenueClassValues(revenueValues).Build();
            Assert.AreEqual(revenueValues.Count - 1, glClassDefinition.RevenueClassValues.Count);
            foreach (var value in glClassDefinition.RevenueClassValues)
            {
                Assert.IsTrue(value == "4" || value == "6");
            }
        }
        #endregion

        #region Asset Class Values

        [TestMethod]
        public void Constructor_NullAssetClassValues()
        {
            var glClassConfiguration = Builder.WithClassificationName("GL.CLASS")
                .WithAssetClassValues(null).Build();
            Assert.AreEqual(0, glClassConfiguration.AssetClassValues.Count);
        }

        [TestMethod]
        public void Constructor_EmptyAssetClassValues()
        {
            var glClassConfiguration = Builder.WithClassificationName("GL.CLASS")
                .WithAssetClassValues(new List<string>()).Build();
            Assert.AreEqual(0, glClassConfiguration.AssetClassValues.Count);
        }

        [TestMethod]
        public void Constructor_AssetClassValuesContainsNull()
        {
            var assetValues = new List<string>() { "4", null, "6" };
            var glClassDefinition = Builder.WithClassificationName("GL.CLASS")
                .WithAssetClassValues(assetValues).Build();
            Assert.AreEqual(assetValues.Count - 1, glClassDefinition.AssetClassValues.Count);
            foreach (var value in glClassDefinition.AssetClassValues)
            {
                Assert.IsTrue(value == "4" || value == "6");
            }
        }

        [TestMethod]
        public void Constructor_AssetClassValuesContainsEmpty()
        {
            var assetValues = new List<string>() { "4", "", "6" };
            var glClassDefinition = Builder.WithClassificationName("GL.CLASS")
                .WithAssetClassValues(assetValues).Build();
            Assert.AreEqual(assetValues.Count - 1, glClassDefinition.AssetClassValues.Count);
            foreach (var value in glClassDefinition.AssetClassValues)
            {
                Assert.IsTrue(value == "4" || value == "6");
            }
        }

        #endregion

        #region Liability Class Values

        [TestMethod]
        public void Constructor_NullLiabilityClassValues()
        {
            var glClassConfiguration = Builder.WithClassificationName("GL.CLASS")
                .WithLiabilityClassValues(null).Build();
            Assert.AreEqual(0, glClassConfiguration.LiabilityClassValues.Count);
        }

        [TestMethod]
        public void Constructor_EmptyLiabilityClassValues()
        {
            var glClassConfiguration = Builder.WithClassificationName("GL.CLASS")
                .WithLiabilityClassValues(new List<string>()).Build();
            Assert.AreEqual(0, glClassConfiguration.LiabilityClassValues.Count);
        }

        [TestMethod]
        public void Constructor_LiabilityClassValuesContainsNull()
        {
            var liabilityValues = new List<string>() { "4", null, "6" };
            var glClassDefinition = Builder.WithClassificationName("GL.CLASS")
                .WithLiabilityClassValues(liabilityValues).Build();
            Assert.AreEqual(liabilityValues.Count - 1, glClassDefinition.LiabilityClassValues.Count);
            foreach (var value in glClassDefinition.LiabilityClassValues)
            {
                Assert.IsTrue(value == "4" || value == "6");
            }
        }

        [TestMethod]
        public void Constructor_LiabilityClassValuesContainsEmpty()
        {
            var liabilityValues = new List<string>() { "4", "", "6" };
            var glClassDefinition = Builder.WithClassificationName("GL.CLASS")
                .WithLiabilityClassValues(liabilityValues).Build();
            Assert.AreEqual(liabilityValues.Count - 1, glClassDefinition.LiabilityClassValues.Count);
            foreach (var value in glClassDefinition.LiabilityClassValues)
            {
                Assert.IsTrue(value == "4" || value == "6");
            }
        }

        #endregion

        #region Fund Balance Class Values

        [TestMethod]
        public void Constructor_NullFundBalanceClassValues()
        {
            var glClassConfiguration = Builder.WithClassificationName("GL.CLASS")
                .WithFundBalanceClassValues(null).Build();
            Assert.AreEqual(0, glClassConfiguration.FundBalanceClassValues.Count);
        }

        [TestMethod]
        public void Constructor_EmptyFundBalanceClassValues()
        {
            var glClassConfiguration = Builder.WithClassificationName("GL.CLASS")
                .WithFundBalanceClassValues(new List<string>()).Build();
            Assert.AreEqual(0, glClassConfiguration.FundBalanceClassValues.Count);
        }

        [TestMethod]
        public void Constructor_FundBalanceClassValuesContainsNull()
        {
            var fundBalanceValues = new List<string>() { "4", null, "6" };
            var glClassDefinition = Builder.WithClassificationName("GL.CLASS")
                .WithFundBalanceClassValues(fundBalanceValues).Build();
            Assert.AreEqual(fundBalanceValues.Count - 1, glClassDefinition.FundBalanceClassValues.Count);
            foreach (var value in glClassDefinition.FundBalanceClassValues)
            {
                Assert.IsTrue(value == "4" || value == "6");
            }
        }

        [TestMethod]
        public void Constructor_FundBalanceClassValuesContainsEmpty()
        {
            var fundBalanceValues = new List<string>() { "4", "", "6" };
            var glClassDefinition = Builder.WithClassificationName("GL.CLASS")
                .WithFundBalanceClassValues(fundBalanceValues).Build();
            Assert.AreEqual(fundBalanceValues.Count - 1, glClassDefinition.FundBalanceClassValues.Count);
            foreach (var value in glClassDefinition.FundBalanceClassValues)
            {
                Assert.IsTrue(value == "4" || value == "6");
            }
        }

        #endregion

        [TestMethod]
        public void Constructor_Success()
        {
            string glClassName = "GL.CLASS";
            var expenseValues = new List<string>() { "5", "7" };
            var revenueValues = new List<string>() { "4" };
            var glClassDefinition = Builder.WithClassificationName(glClassName)
                .WithExpenseClassValues(expenseValues)
                .WithRevenueClassValues(revenueValues).Build();

            Assert.AreEqual(glClassName, glClassDefinition.ClassificationName);

            Assert.IsTrue(glClassDefinition.ExpenseClassValues is IEnumerable<string>);
            foreach (var expenseValue in expenseValues)
            {
                Assert.IsTrue(glClassDefinition.ExpenseClassValues.Contains(expenseValue));
            }

            Assert.IsTrue(glClassDefinition.RevenueClassValues is IEnumerable<string>);
            foreach (var revenueValue in revenueValues)
            {
                Assert.IsTrue(glClassDefinition.RevenueClassValues.Contains(revenueValue));
            }
        }
        #endregion
    }
}