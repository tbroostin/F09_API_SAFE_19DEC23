// Copyright 2015 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    /// <summary>
    /// Test valid and invalid conditions for a Form1095cPdf domain entity.
    /// </summary>
    [TestClass]
    public class Form1095cPdfDependentsDataTests
    {
        public Form1095cPdfData form1095cPdfData;
        public Form1095cPdfDataBuilder form1095cPdfDataBuilder = new Form1095cPdfDataBuilder();

        [TestMethod]
        public void Form1095cPdfData_Success()
        {
            form1095cPdfData = form1095cPdfDataBuilder.Build();
            Assert.AreEqual(form1095cPdfDataBuilder.TaxYear, form1095cPdfData.TaxYear);
            Assert.AreEqual(form1095cPdfDataBuilder.Ein, form1095cPdfData.EmployerEin);
            Assert.AreEqual(form1095cPdfDataBuilder.Ssn, form1095cPdfData.EmployeeSsn);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Form1095cPdfData_NullTaxYear()
        {
            form1095cPdfData = form1095cPdfDataBuilder.WithTaxYear(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Form1095cPdfData_EmptyTaxYear()
        {
            form1095cPdfData = form1095cPdfDataBuilder.WithTaxYear(string.Empty).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Form1095cPdfData_NullEin()
        {
            form1095cPdfData = form1095cPdfDataBuilder.WithEin(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Form1095cPdfData_EmptyEin()
        {
            form1095cPdfData = form1095cPdfDataBuilder.WithEin(string.Empty).Build();
        }
       
        [TestMethod]
        public void Form1095cPdfData_NullSsn()
        {
            form1095cPdfData = form1095cPdfDataBuilder.WithSsn(null).Build();
            Assert.AreEqual("", form1095cPdfData.EmployeeSsn);
        }

        [TestMethod]
        public void Form1095cPdfData_EmptySsn()
        {
            form1095cPdfData = form1095cPdfDataBuilder.WithSsn(string.Empty).Build();
            Assert.AreEqual("", form1095cPdfData.EmployeeSsn);
        }
    }
}
