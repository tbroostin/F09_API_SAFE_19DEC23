/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayrollDeductionTests
    {
        PayrollDeduction actualPayrollDeduction;
        string expectedGuid;
        string expectedId;
        string expectedArrangmentId;
        string expectedArrangmentGuid;
        DateTime expectedDeductionDate;
        string expectedCurrencyContry;
        decimal expectedAmount;

        [TestInitialize]
        public void Initialize()
        {
            expectedGuid = "1234";
            expectedId = "33";
            expectedArrangmentId = "44";
            expectedArrangmentGuid = "55";
            expectedDeductionDate = new DateTime(2011, 1, 1);
            expectedCurrencyContry = "Canada";
            expectedAmount = 44.00m;
        }

        [TestMethod]
        public void ConstructorSetsPropertiesTest()
        {
            actualPayrollDeduction = new PayrollDeduction(expectedGuid, expectedId, expectedArrangmentId, expectedArrangmentGuid, expectedDeductionDate, expectedCurrencyContry, expectedAmount);

            Assert.AreEqual(expectedGuid, actualPayrollDeduction.Guid);
            Assert.AreEqual(expectedId, actualPayrollDeduction.Id);
            Assert.AreEqual(expectedArrangmentId, actualPayrollDeduction.ArrangementId);
            Assert.AreEqual(expectedArrangmentGuid, actualPayrollDeduction.ArrangementGuid);
            Assert.AreEqual(expectedDeductionDate, actualPayrollDeduction.DeductionDate);
            Assert.AreEqual(expectedCurrencyContry, actualPayrollDeduction.AmountCountry);
            Assert.AreEqual(expectedAmount, actualPayrollDeduction.Amount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void GuidIsRequiredTest()
        {
            actualPayrollDeduction = new PayrollDeduction(null, expectedId, expectedArrangmentId, expectedArrangmentGuid, expectedDeductionDate, expectedCurrencyContry, expectedAmount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void IdIsRequiredTest()
        {
            actualPayrollDeduction = new PayrollDeduction(expectedGuid, null, expectedArrangmentId, expectedArrangmentGuid, expectedDeductionDate, expectedCurrencyContry, expectedAmount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ArrangementIdIsRequiredTest()
        {
            actualPayrollDeduction = new PayrollDeduction(expectedGuid, expectedId, null, expectedArrangmentGuid, expectedDeductionDate, expectedCurrencyContry, expectedAmount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ArrangementGuidIsRequiredTest()
        {
            actualPayrollDeduction = new PayrollDeduction(expectedGuid, expectedId, expectedArrangmentId, null, expectedDeductionDate, expectedCurrencyContry, expectedAmount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void CurrencyContryIsRequiredTest()
        {
            actualPayrollDeduction = new PayrollDeduction(expectedGuid, expectedId, expectedArrangmentId, expectedArrangmentGuid, expectedDeductionDate, null, expectedAmount);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void DeductionDateIsRequiredTest()
        {
            actualPayrollDeduction = new PayrollDeduction(expectedGuid, expectedId, expectedArrangmentId, expectedArrangmentGuid, default(DateTime), expectedCurrencyContry, expectedAmount);
        }
    }
}
