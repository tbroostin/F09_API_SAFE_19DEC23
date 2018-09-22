/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayrollRegisterBenefitDeductionEntryTests
    {
        public string benefitDeductionId;
        public decimal? employeeAmount;
        public decimal? employeeBasisAmount;
        public decimal? employerAmount;
        public decimal? employerBasisAmount;
        public PayrollRegisterBenefitDeductionEntry prbde;

        [TestInitialize]
        public void Initialize()
        {
            benefitDeductionId = "ATMA";
            employeeAmount = 42.1m;
            employeeBasisAmount = 24.1m;
            employerAmount = 14.2m;
            employerBasisAmount = 241m;
        }

        [TestMethod]
        public void PropertiesAreOrAreNotSet()
        {
            prbde = new PayrollRegisterBenefitDeductionEntry(benefitDeductionId);
            Assert.AreEqual(benefitDeductionId, prbde.BenefitDeductionId);
            Assert.AreEqual(null, prbde.EmployeeAmount);
            Assert.AreEqual(null, prbde.EmployeeBasisAmount);
            Assert.AreEqual(null, prbde.EmployerAmount);
            Assert.AreEqual(null, prbde.EmployerBasisAmount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullIdArgumentException()
        {
            prbde = new PayrollRegisterBenefitDeductionEntry(null);
        }

        [TestMethod]
        public void SettersTest()
        {
            prbde = new PayrollRegisterBenefitDeductionEntry(benefitDeductionId);
            prbde.EmployeeAmount = employeeAmount;
            prbde.EmployeeBasisAmount = employeeBasisAmount;
            prbde.EmployerAmount = employerAmount;
            prbde.EmployerBasisAmount = employerBasisAmount;

            Assert.AreEqual(employeeAmount, prbde.EmployeeAmount);
            Assert.AreEqual(employeeBasisAmount, prbde.EmployeeBasisAmount);
            Assert.AreEqual(employerAmount, prbde.EmployerAmount);
            Assert.AreEqual(employerBasisAmount, prbde.EmployerBasisAmount);
        }
    }
}
