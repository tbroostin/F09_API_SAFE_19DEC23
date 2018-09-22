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
    public class PayStatementSourceBenefitDeductionTests
    {
        public string benefitDeductionCode;
        public string benefitDeductionDescription;
        public decimal? employeePeriodDeductionAmount;
        public decimal? employerPeriodDeductionAmount;
        public decimal? employeePeriodDeductionBaseAmount;
        public decimal? employerPeriodDeductionBaseAmount;
        public decimal? periodApplicableBenefitDeductionGross;

        public PayStatementSourceBenefitDeduction bended;

        [TestInitialize]
        public void Initialize()
        {
            benefitDeductionCode = "somebended";
            benefitDeductionDescription = "this is a benefit or a deduction";
            employeePeriodDeductionAmount = 11.00M;
            employerPeriodDeductionAmount = 12.11M;
            employeePeriodDeductionBaseAmount = 13.12M;
            employerPeriodDeductionBaseAmount = 14.13M;
        }

        [TestMethod]
        public void PropertiesAreSetTest()
        {
            bended = new PayStatementSourceBenefitDeduction(
                benefitDeductionCode,
                benefitDeductionDescription,
                employeePeriodDeductionAmount,
                employerPeriodDeductionAmount,
                employeePeriodDeductionBaseAmount, 
                employerPeriodDeductionBaseAmount
            );
            Assert.AreEqual(benefitDeductionCode,bended.BenefitDeductionCode);
            Assert.AreEqual(benefitDeductionDescription,bended.BenefitDeductionDescription);
            Assert.AreEqual(employeePeriodDeductionAmount,bended.EmployeePeriodDeductionAmount);
            Assert.AreEqual(employerPeriodDeductionAmount, bended.EmployerPeriodDeductionAmount);
            Assert.AreEqual(employeePeriodDeductionBaseAmount, bended.EmployeePeriodDeductionBaseAmount);
            Assert.AreEqual(employerPeriodDeductionBaseAmount, bended.EmployerPeriodDeductionBaseAmount);
            Assert.AreEqual(employeePeriodDeductionBaseAmount ?? employerPeriodDeductionBaseAmount, bended.PeriodApplicableBenefitDeductionGross);
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        public void NoBenefitDeductionCode()
        {
            bended = new PayStatementSourceBenefitDeduction(
                null,
                benefitDeductionDescription,
                employeePeriodDeductionAmount,
                employerPeriodDeductionAmount,
                employeePeriodDeductionBaseAmount,
                employerPeriodDeductionBaseAmount
            );
        }
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NoBenefitDeductionDescription()
        {
            bended = new PayStatementSourceBenefitDeduction(
                benefitDeductionCode,
                null,
                employeePeriodDeductionAmount,
                employerPeriodDeductionAmount,
                employeePeriodDeductionBaseAmount,
                employerPeriodDeductionBaseAmount
            );
        }

        [TestMethod]
        public void Equals()
        {
            var bendedA = new PayStatementSourceBenefitDeduction(
                benefitDeductionCode,
                benefitDeductionDescription,
                employeePeriodDeductionAmount,
                employerPeriodDeductionAmount,
                employeePeriodDeductionBaseAmount,
                employerPeriodDeductionBaseAmount
            );
            var bendedB = new PayStatementSourceBenefitDeduction(
                benefitDeductionCode,
                benefitDeductionDescription,
                employeePeriodDeductionAmount,
                employerPeriodDeductionAmount,
                employeePeriodDeductionBaseAmount,
                employerPeriodDeductionBaseAmount
            );
            Assert.IsTrue(bendedA.Equals(bendedB));
        }

        [TestMethod]
        public void HashCode()
        {
            bended = new PayStatementSourceBenefitDeduction(
                benefitDeductionCode,
                benefitDeductionDescription,
                employeePeriodDeductionAmount,
                employerPeriodDeductionAmount,
                employeePeriodDeductionBaseAmount,
                employerPeriodDeductionBaseAmount
            );
            Assert.AreEqual(bended.BenefitDeductionCode.GetHashCode(),bended.GetHashCode());
        }
    }
}
