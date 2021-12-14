/* Copyright 2017-2021 Ellucian Company L.P. and its affiliates. */
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
        public decimal? employeeAdjustmentAmount;
        public decimal? employerAdjustmentAmount;
        public decimal? employeeTaxableAdjustmentAmount;
        public decimal? employerTaxableAdjustmentAmount;

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
            employeeAdjustmentAmount = 34m;
            employeeTaxableAdjustmentAmount = 200m;
            employerAdjustmentAmount = -35m;
            employerTaxableAdjustmentAmount = 100m;
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

        [TestMethod]
        public void EmployeeAdjustmentAmount_GetSetTest()
        {
            var entry = new PayrollRegisterTaxEntry(taxCode, processingCode);
            entry.EmployeeAdjustmentAmount = employeeAdjustmentAmount;
            Assert.AreEqual(employeeAdjustmentAmount, entry.EmployeeAdjustmentAmount);
        }

        [TestMethod]
        public void EmployerAdjustmentAmount_GetSetTest()
        {
            var entry = new PayrollRegisterTaxEntry(taxCode, processingCode);
            entry.EmployerAdjustmentAmount = employerAdjustmentAmount;
            Assert.AreEqual(employerAdjustmentAmount, entry.EmployerAdjustmentAmount);
        }

        [TestMethod]
        public void EmployeeTaxableAdjustmentAmount_GetSetTest()
        {
            var entry = new PayrollRegisterTaxEntry(taxCode, processingCode);
            entry.EmployeeTaxableAdjustmentAmount = employeeTaxableAdjustmentAmount;
            Assert.AreEqual(employeeTaxableAdjustmentAmount, entry.EmployeeTaxableAdjustmentAmount);
        }

        [TestMethod]
        public void EmployerTaxableAdjustmentAmount_GetSetTest()
        {
            var entry = new PayrollRegisterTaxEntry(taxCode, processingCode);
            entry.EmployerTaxableAdjustmentAmount = employerTaxableAdjustmentAmount;
            Assert.AreEqual(employerTaxableAdjustmentAmount, entry.EmployerTaxableAdjustmentAmount);
        }
    }
}
