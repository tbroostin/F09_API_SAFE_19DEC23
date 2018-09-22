/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayrollDeductionArrangementTests
    {
        PayrollDeductionArrangements actualPayrollDeductionArrangement;
        string expectedGuid = "11";
        string expectedPersonId = "22";
        string expectedId = "33";
        string expectedCommitmentContributionId = "44";
        string expectedCommitmentType = "AA";
        string expectedDeductionTypeCode = "BB";
        string expectedStatus = "CC";
        decimal? expectedAmountPerPayment = 47m;
        decimal? expectedTotalAmount = 56m;
        DateTime? expectedStartDate = new DateTime(2017, 1, 1);
        DateTime? expectedEndDate = new DateTime(2017, 2, 1) ;
        int? expectedInterval = 55;
        List<int?> expectedMonthlyPayPeriods = new List<int?>() { 11, 22, 33, 44 };
        string expectedChangeReason = "DD";

        [TestMethod]
        public void ConstructorSetsPropertiesTest()
        {
            actualPayrollDeductionArrangement = new PayrollDeductionArrangements(expectedGuid, expectedPersonId);

            Assert.AreEqual(expectedGuid, actualPayrollDeductionArrangement.Guid);
            Assert.AreEqual(expectedPersonId, actualPayrollDeductionArrangement.PersonId);
        }

        [TestMethod]
        public void PropertySettersAndGettersTest()
        {
            actualPayrollDeductionArrangement = new PayrollDeductionArrangements(expectedGuid, expectedPersonId);
            actualPayrollDeductionArrangement.Id = expectedId;
            actualPayrollDeductionArrangement.CommitmentContributionId = expectedCommitmentContributionId;
            actualPayrollDeductionArrangement.CommitmentType = expectedCommitmentType;
            actualPayrollDeductionArrangement.DeductionTypeCode = expectedDeductionTypeCode;
            actualPayrollDeductionArrangement.Status = expectedStatus;
            actualPayrollDeductionArrangement.AmountPerPayment = expectedAmountPerPayment;
            actualPayrollDeductionArrangement.TotalAmount = expectedTotalAmount;
            actualPayrollDeductionArrangement.StartDate = expectedStartDate;
            actualPayrollDeductionArrangement.EndDate = expectedEndDate;
            actualPayrollDeductionArrangement.Interval = expectedInterval;
            actualPayrollDeductionArrangement.MonthlyPayPeriods = expectedMonthlyPayPeriods;
            actualPayrollDeductionArrangement.ChangeReason = expectedChangeReason;

            Assert.AreEqual(expectedId, actualPayrollDeductionArrangement.Id);
            Assert.AreEqual(expectedCommitmentContributionId, actualPayrollDeductionArrangement.CommitmentContributionId);
            Assert.AreEqual(expectedCommitmentType, actualPayrollDeductionArrangement.CommitmentType);
            Assert.AreEqual(expectedDeductionTypeCode, actualPayrollDeductionArrangement.DeductionTypeCode);
            Assert.AreEqual(expectedStatus, actualPayrollDeductionArrangement.Status);
            Assert.AreEqual(expectedAmountPerPayment, actualPayrollDeductionArrangement.AmountPerPayment);
            Assert.AreEqual(expectedTotalAmount, actualPayrollDeductionArrangement.TotalAmount);
            Assert.AreEqual(expectedStartDate, actualPayrollDeductionArrangement.StartDate);
            Assert.AreEqual(expectedEndDate, actualPayrollDeductionArrangement.EndDate);
            Assert.AreEqual(expectedInterval, actualPayrollDeductionArrangement.Interval);
            Assert.AreEqual(expectedMonthlyPayPeriods, actualPayrollDeductionArrangement.MonthlyPayPeriods);
            Assert.AreEqual(expectedChangeReason, actualPayrollDeductionArrangement.ChangeReason);
        }

        [TestMethod]
        public void EqualsOverrideTests()
        {
            // objects are equal based off GUID
            actualPayrollDeductionArrangement = new PayrollDeductionArrangements(expectedGuid, expectedPersonId);
            var actualPayrollDeductionArrangement2 = new PayrollDeductionArrangements(expectedGuid, expectedPersonId);
            Assert.IsTrue(actualPayrollDeductionArrangement.Equals(actualPayrollDeductionArrangement2));

            // objects are not equal based off GUID
            actualPayrollDeductionArrangement = new PayrollDeductionArrangements("99", expectedPersonId);
            Assert.IsFalse(actualPayrollDeductionArrangement.Equals(actualPayrollDeductionArrangement2));

            // objects are not equal based on type or null object passed in
            var employeeObject = new Employee(expectedGuid, expectedPersonId);
            Assert.IsFalse(actualPayrollDeductionArrangement.Equals(employeeObject));
            Assert.IsFalse(actualPayrollDeductionArrangement.Equals(null));
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void GuidIsRequiredTest()
        {
            actualPayrollDeductionArrangement = new PayrollDeductionArrangements(null, expectedPersonId);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void PersonIdIsRequiredTest()
        {
            actualPayrollDeductionArrangement = new PayrollDeductionArrangements(expectedGuid, null);
        }

        [TestMethod]
        public void GetHashCodeOverrideTest()
        {
            actualPayrollDeductionArrangement = new PayrollDeductionArrangements(expectedGuid, expectedPersonId);
            Assert.AreEqual(expectedGuid.GetHashCode(), actualPayrollDeductionArrangement.GetHashCode());
        }

        [TestMethod]
        public void ToStringOverrideTest()
        {
            actualPayrollDeductionArrangement = new PayrollDeductionArrangements(expectedGuid, expectedPersonId);
            Assert.AreEqual(expectedGuid, actualPayrollDeductionArrangement.ToString());
        }
    }
}
