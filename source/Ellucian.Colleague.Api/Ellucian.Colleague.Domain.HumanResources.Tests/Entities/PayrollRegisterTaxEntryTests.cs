/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayrollRegisterTaxEntryTests
    {
        public string taxCode;
        public PayrollTaxProcessingCode processingCode;
        public decimal? specialProcessingAmount;
        public int exemptions;
        public decimal? employeeTaxAmount;
        public decimal? employerTaxAmount;
        public decimal? employeeTaxableAmount;
        public decimal? employerTaxableAmount;
        public PayrollRegisterTaxEntry taxEntry
        {
            get
            {
                return new PayrollRegisterTaxEntry(taxCode, processingCode);
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            taxCode = "cielo";
            processingCode = PayrollTaxProcessingCode.AdditionalTaxAmount;
            specialProcessingAmount = 100m;
            exemptions = 2;
            employeeTaxAmount = 5m;
            employeeTaxableAmount = 10m;
            employerTaxAmount = 15m;
            employerTaxableAmount = 22m;
        }

        [TestMethod]
        public void PropertiesAreSet()
        {
            Assert.AreEqual(taxCode,taxEntry.TaxCode);
            Assert.AreEqual(processingCode, taxEntry.ProcessingCode);
        }

        [TestMethod]
        public void PropertiesNotInConstructorAreSettable()
        {

            var tax = taxEntry;
            tax.EmployeeTaxAmount = employeeTaxAmount;
            tax.EmployeeTaxableAmount = employeeTaxableAmount;
            tax.EmployerTaxAmount = employerTaxAmount;
            tax.EmployerTaxableAmount = employerTaxableAmount;
            tax.SpecialProcessingAmount = specialProcessingAmount;
            tax.Exemptions = exemptions;
            Assert.AreEqual(employeeTaxableAmount, tax.EmployeeTaxableAmount);
            Assert.AreEqual(employeeTaxAmount, tax.EmployeeTaxAmount);
            Assert.AreEqual(employerTaxAmount, tax.EmployerTaxAmount);
            Assert.AreEqual(employerTaxableAmount, tax.EmployerTaxableAmount);
            Assert.AreEqual(exemptions, tax.Exemptions);
            Assert.AreEqual(specialProcessingAmount, tax.SpecialProcessingAmount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullTaxCodeIsHandled()
        {
            taxCode = null;
            var error = taxEntry;
        }
    }
}
