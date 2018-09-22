// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class TaxFormBoxesPdfDataTests
    {
        private string boxNumber = "2017";
        private decimal amount = 100m;

        [TestMethod]
        public void Constructor_Success()
        {
            var entity = new TaxFormBoxesPdfData(boxNumber, amount);
            Assert.AreEqual(boxNumber, entity.BoxNumber);
            Assert.AreEqual(amount, entity.Amount);
        }

        [TestMethod]
        public void Constructor_NullBoxNumber()
        {
            var expectedParam = "boxNumber";
            var actualParam = "";
            try
            {
                new TaxFormBoxesPdfData(null, amount);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void Constructor_EmptyBoxNumber()
        {
            var expectedParam = "boxNumber";
            var actualParam = "";
            try
            {
                new TaxFormBoxesPdfData("", amount);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void AddAMount_StartAtZero()
        {
            var entity = new TaxFormBoxesPdfData(boxNumber, 0m);
            entity.AddAmount(75m);
            Assert.AreEqual(75m, entity.Amount);
        }

        [TestMethod]
        public void AddAMount_StartAtPositiveAmount()
        {
            var entity = new TaxFormBoxesPdfData(boxNumber, -25m);
            entity.AddAmount(50m);
            Assert.AreEqual(25m, entity.Amount);
        }
    }
}