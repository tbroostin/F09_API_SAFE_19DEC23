// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GeneralLedgerComponentRangeTests
    {
        private string startValue;
        private string endValue;

        #region Constructor tests
        [TestMethod]
        public void GeneralLedgerComponentRangeTests_StartValuePreceedsEndValue()
        {
            this.startValue = "1a";
            this.endValue = "1b";
            var componentRange = BuildComponentRange();

            Assert.AreEqual(this.startValue.ToUpperInvariant(), componentRange.StartValue);
            Assert.AreEqual(this.endValue.ToUpperInvariant(), componentRange.EndValue);
        }

        [TestMethod]
        public void GeneralLedgerComponentRangeTests_StartValueSameAsEndValue()
        {
            this.startValue = "1A";
            this.endValue = "1a";
            var componentRange = BuildComponentRange();

            Assert.AreEqual(this.startValue.ToUpperInvariant(), componentRange.StartValue);
            Assert.AreEqual(this.endValue.ToUpperInvariant(), componentRange.EndValue);
        }

        [TestMethod]
        public void GeneralLedgerComponentRangeTests_StartValueAfterEndValue()
        {
            var expectedMessage = "The start value must preceed the end value.";
            var actualMessage = "";
            try
            {
                this.startValue = "2";
                this.endValue = "1";
                BuildComponentRange();
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public void GeneralLedgerComponentRangeTests_NullStartValue()
        {
            var expectedParam = "startValue";
            var actualParam = "";
            try
            {
                this.startValue = null;
                this.endValue = "1";
                BuildComponentRange();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void GeneralLedgerComponentRangeTests_EmptyStartValue()
        {
            var expectedParam = "startValue";
            var actualParam = "";
            try
            {
                this.startValue = "";
                this.endValue = "1";
                BuildComponentRange();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void GeneralLedgerComponentRangeTests_NullEndValue()
        {
            var expectedParam = "endValue";
            var actualParam = "";
            try
            {
                this.startValue = "1";
                this.endValue = null;
                BuildComponentRange();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void GeneralLedgerComponentRangeTests_EmptyEndValue()
        {
            var expectedParam = "endValue";
            var actualParam = "";
            try
            {
                this.startValue = "1";
                this.endValue = "";
                BuildComponentRange();
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }
        #endregion

        public GeneralLedgerComponentRange BuildComponentRange()
        {
            var temp = new GeneralLedgerComponentRange(this.startValue, this.endValue);
            return temp;
        }
    }
}