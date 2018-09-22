// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    /// <summary>
    /// Test valid and invalid conditions for a FormW2Pdf domain entity.
    /// </summary>
    [TestClass]
    public class FormW2PdfDataTests
    {
        public FormW2PdfData pdfData;
        public FormW2PdfDataBuilder pdfDataBuilder = new FormW2PdfDataBuilder();
 
        [TestMethod]
        public void FormW2PdfData_Contructor()
        {
            pdfData = pdfDataBuilder.Build();
            Assert.AreEqual(pdfDataBuilder.TaxYear, pdfData.TaxYear);
            Assert.AreEqual(pdfDataBuilder.EmployerId, pdfData.EmployerEin);
            Assert.AreEqual(pdfDataBuilder.EmployeeSsn, pdfData.EmployeeSsn);
        }
        
        [TestMethod]
        public void FormW2PdfData_NameSuccess()
        {
            pdfData = pdfDataBuilder.Build();
            pdfData.EmployeeFirstName = "Ken";
            pdfData.EmployeeMiddleName = "John";
            pdfData.EmployeeLastName = "Ellucian";
            pdfData.EmployeeSuffix = "Jr.";
            string expectedName = pdfData.EmployeeFirstName + " " + pdfData.EmployeeMiddleName.Substring(0, 1)
                               + " " + pdfData.EmployeeLastName + ", " + pdfData.EmployeeSuffix;
            Assert.AreEqual(expectedName, pdfData.EmployeeName());
        }

        [TestMethod]
        public void FormW2PdfData_Name_NoName()
        {
            pdfData = pdfDataBuilder.Build();
            pdfData.EmployeeFirstName = string.Empty;
            pdfData.EmployeeMiddleName = string.Empty;
            pdfData.EmployeeLastName = string.Empty;
            pdfData.EmployeeSuffix = string.Empty;

            Assert.AreEqual(string.Empty, pdfData.EmployeeName());
        }

        [TestMethod]
        public void FormW2PdfData_Name_FirstNameOnly()
        {
            pdfData = pdfDataBuilder.Build();
            pdfData.EmployeeFirstName = "Ken";
            pdfData.EmployeeMiddleName = string.Empty;
            pdfData.EmployeeLastName = string.Empty;
            pdfData.EmployeeSuffix = string.Empty;

            Assert.AreEqual(pdfData.EmployeeFirstName, pdfData.EmployeeName());
        }

        [TestMethod]
        public void FormW2PdfData_Name_MiddleNameOnly()
        {
            pdfData = pdfDataBuilder.Build();
            pdfData.EmployeeFirstName = string.Empty;
            pdfData.EmployeeMiddleName = "John";
            pdfData.EmployeeLastName = string.Empty;
            pdfData.EmployeeSuffix = string.Empty;

            Assert.AreEqual(string.Empty, pdfData.EmployeeName());
        }

        [TestMethod]
        public void FormW2PdfData_Name_LastNameOnly()
        {
            pdfData = pdfDataBuilder.Build();
            pdfData.EmployeeFirstName = string.Empty;
            pdfData.EmployeeMiddleName = string.Empty;
            pdfData.EmployeeLastName = "Ellucian";
            pdfData.EmployeeSuffix = string.Empty;

            Assert.AreEqual(string.Empty, pdfData.EmployeeName());
        }

        [TestMethod]
        public void FormW2PdfData_Name_SuffixOnly()
        {
            pdfData = pdfDataBuilder.Build();
            pdfData.EmployeeFirstName = string.Empty;
            pdfData.EmployeeMiddleName = string.Empty;
            pdfData.EmployeeLastName = string.Empty;
            pdfData.EmployeeSuffix = "Jr.";

            Assert.AreEqual(string.Empty, pdfData.EmployeeName());
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FormW2PdfData_NullTaxYear()
        {
            pdfData = pdfDataBuilder.WithTaxYear(string.Empty).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FormW2PdfData_NullEmployerId()
        {
            pdfData = pdfDataBuilder.WithEmployerId(string.Empty).Build();
        }

        [TestMethod]
        public void FormW2PdfData_NullEmployeeSsn()
        {
            pdfData = pdfDataBuilder.WithEmployeeSsn(null).Build();
            Assert.AreEqual("", pdfData.EmployeeSsn);
        }

        [TestMethod]
        public void FormW2PdfData_EmptyEmployeeSsn()
        {
            pdfData = pdfDataBuilder.WithEmployeeSsn(string.Empty).Build();
            Assert.AreEqual("", pdfData.EmployeeSsn);
        }
    }
}
