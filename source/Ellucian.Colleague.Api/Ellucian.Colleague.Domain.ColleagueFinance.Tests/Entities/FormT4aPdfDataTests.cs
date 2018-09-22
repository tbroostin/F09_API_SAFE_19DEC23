// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class FormT4aPdfDataTests
    {
        private string taxYear = "2017";
        private string payerName = "Ellucian University";

        [TestMethod]
        public void Constructor_Success()
        {
            var entity = new FormT4aPdfData(taxYear, payerName);
            Assert.AreEqual(taxYear, entity.TaxYear);
            Assert.AreEqual(payerName, entity.PayerName);
            Assert.IsTrue(entity.TaxFormBoxesList is List<TaxFormBoxesPdfData>);
            Assert.AreEqual(0, entity.TaxFormBoxesList.Count);
        }

        [TestMethod]
        public void Constructor_NullTaxYear()
        {
            var expectedParam = "taxYear";
            var actualParam = "";
            try
            {
                new FormT4aPdfData(null, payerName);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void Constructor_EmptyTaxYear()
        {
            var expectedParam = "taxYear";
            var actualParam = "";
            try
            {
                new FormT4aPdfData("", payerName);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void Constructor_NullPayerName()
        {
            var expectedParam = "payerName";
            var actualParam = "";
            try
            {
                new FormT4aPdfData(taxYear, null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void Constructor_EmptyPayerName()
        {
            var expectedParam = "payerName";
            var actualParam = "";
            try
            {
                new FormT4aPdfData(taxYear, "");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }
    }
}