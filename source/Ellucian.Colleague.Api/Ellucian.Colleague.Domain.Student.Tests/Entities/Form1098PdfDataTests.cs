// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class Form1098PdfDataTests
    {
        [TestMethod]
        public void Constructor_Success()
        {
            string taxYear = "2015";
            string institutionEin = "52-12382323";
            var pdfData = new Form1098PdfData(taxYear, institutionEin);

            Assert.AreEqual(taxYear, pdfData.TaxYear);
            Assert.AreEqual(institutionEin, pdfData.InstitutionEin);
        }

        [TestMethod]
        public void Constructor_NullTaxYear()
        {
            var expectedParam = "taxyear";
            var actualParam = "";
            try
            {
                new Form1098PdfData(null, "234");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void Constructor_EmptyTaxYear()
        {
            var expectedParam = "taxyear";
            var actualParam = "";
            try
            {
                new Form1098PdfData("", "234");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void Constructor_NullInstitutionEin()
        {
            var expectedParam = "institutionein";
            var actualParam = "";
            try
            {
                new Form1098PdfData("2015", null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void Constructor_EmptyInstitutionEin()
        {
            var expectedParam = "institutionein";
            var actualParam = "";
            try
            {
                new Form1098PdfData("2015", "");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }
            Assert.AreEqual(expectedParam, actualParam);
        }
    }
}