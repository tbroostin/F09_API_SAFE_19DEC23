/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayStatementSourceTaxTests
    {

        public PayStatementSourceTax tax;

        public string TaxCode;
        public string TaxDescription;
        public decimal? EmployeePeriodTaxAmount;
        public decimal? EmployerPeriodTaxAmount;
        public decimal? EmployeePeriodTaxableAmount;
        public decimal? EmployerPeriodTaxableAmount;
        public decimal? PeriodApplicableTaxableGross;

        [TestInitialize]
        public void Initialize()
        {
            TaxCode = "deathAndTaxes";
            TaxDescription = "are the certainties of life";
            EmployeePeriodTaxAmount = 10.00m;
            EmployerPeriodTaxAmount = 11.10m;
            EmployeePeriodTaxableAmount = 12.11m;
            EmployerPeriodTaxableAmount = 13.12m;           
        }
        [TestMethod]
        public void PropertiesAreSet()
        {
            tax = new PayStatementSourceTax(
                TaxCode, 
                TaxDescription, 
                EmployeePeriodTaxAmount, 
                EmployerPeriodTaxAmount,
                EmployeePeriodTaxableAmount, 
                EmployerPeriodTaxableAmount
            );
            Assert.AreEqual(TaxCode,tax.TaxCode);
            Assert.AreEqual(TaxDescription, tax.TaxDescription);
            Assert.AreEqual(EmployeePeriodTaxAmount, tax.EmployeePeriodTaxAmount);
            Assert.AreEqual(EmployerPeriodTaxAmount, tax.EmployerPeriodTaxAmount);
            Assert.AreEqual(EmployeePeriodTaxableAmount, tax.EmployeePeriodTaxableAmount);
            Assert.AreEqual(EmployerPeriodTaxableAmount,tax.EmployerPeriodTaxableAmount);
            Assert.AreEqual(EmployeePeriodTaxableAmount ?? EmployerPeriodTaxableAmount, tax.PeriodApplicableTaxableGross);
        }
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullTaxCode()
        {
            tax = new PayStatementSourceTax(
                null,
                TaxDescription,
                EmployeePeriodTaxAmount,
                EmployerPeriodTaxAmount,
                EmployeePeriodTaxableAmount,
                EmployerPeriodTaxableAmount
            );
        }
        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        public void NullTaxDescription()
        {
            tax = new PayStatementSourceTax(
                TaxCode,
                null,
                EmployeePeriodTaxAmount,
                EmployerPeriodTaxAmount,
                EmployeePeriodTaxableAmount,
                EmployerPeriodTaxableAmount
            );
        }
        [TestMethod]
        public void Equals()
        {
           var taxa = new PayStatementSourceTax(
                TaxCode,
                TaxDescription,
                EmployeePeriodTaxAmount,
                EmployerPeriodTaxAmount,
                EmployeePeriodTaxableAmount,
                EmployerPeriodTaxableAmount
            );
            var taxb = new PayStatementSourceTax(
                TaxCode,
                TaxDescription,
                EmployeePeriodTaxAmount,
                EmployerPeriodTaxAmount,
                EmployeePeriodTaxableAmount,
                EmployerPeriodTaxableAmount
            );
            Assert.IsTrue(taxa.Equals(taxb));
        }
        [TestMethod]
        public void HashCode()
        {
            tax = new PayStatementSourceTax(
                TaxCode,
                TaxDescription,
                EmployeePeriodTaxAmount,
                EmployerPeriodTaxAmount,
                EmployeePeriodTaxableAmount,
                EmployerPeriodTaxableAmount
            );
            Assert.AreEqual(TaxCode.GetHashCode(), tax.GetHashCode());
        }
    }
}
