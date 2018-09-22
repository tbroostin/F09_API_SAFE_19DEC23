/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayStatementReferenceDataUtilityTests
    {
        public List<BenefitDeductionType> benefitDeductionTypes;
        public List<TaxCode> taxCodes;
        public List<EarningsType> earningsTypes;
        public List<EarningsDifferential> earningsDifferentials;
        public List<LeaveType> leaveTypes;
        public PayStatementConfiguration payStatementConfiguration;
        public PayrollRegisterEarningsEntry payrollRegisterEarningsEntry;
        public List<Position> positions;
        public PayStatementReferenceDataUtility referenceUtility;

        [TestInitialize]
        public void Initialize()
        {
            benefitDeductionTypes = new List<BenefitDeductionType>();
            benefitDeductionTypes.Add(new BenefitDeductionType("LIF", "Life Insurance", "Self-Service Life Insurance", BenefitDeductionTypeCategory.Benefit));
            benefitDeductionTypes.Add(new BenefitDeductionType("401K", "401K Deduction","self-service 401k deduction", BenefitDeductionTypeCategory.Deduction));

            taxCodes = new List<TaxCode>();
            taxCodes.Add(new TaxCode("FED", "Federal Tax", TaxCodeType.FederalWithholding));
            taxCodes.Add(new TaxCode("VAST", "Virginia Tax", TaxCodeType.StateWithholding));

            earningsTypes = new List<EarningsType>();
            earningsTypes.Add(new EarningsType("REG", "Regular", true, EarningsCategory.Regular, EarningsMethod.None, null));
            earningsTypes.Add(new EarningsType("OVT", "Overtime", true, EarningsCategory.Overtime, EarningsMethod.None, 1.5m));

            earningsDifferentials = new List<EarningsDifferential>();
            earningsDifferentials.Add(new EarningsDifferential("1ST", "First Shift"));
            earningsDifferentials.Add(new EarningsDifferential("2ND", "Second Shift"));

            leaveTypes = new List<LeaveType>();
            leaveTypes.Add(new LeaveType(Guid.NewGuid().ToString(), "VACA", "Vacation"));
            leaveTypes.Add(new LeaveType(Guid.NewGuid().ToString(), "SICK", "Sick Leave"));
            leaveTypes.Add(new LeaveType(Guid.NewGuid().ToString(), "COMP", "Comp time"));

            positions = new List<Position>();
            positions.Add(new Position("ABC", "Alphabet", "Alpha", "ENG", new DateTime(2015, 1, 1), true));
            positions.Add(new Position("123", "Numeric", "Num", "MATH", new DateTime(2016, 1, 1), false));

            payStatementConfiguration = new PayStatementConfiguration()
            {
                DisplayZeroAmountBenefitDeductions = true,
                OffsetDaysCount = -5,
                PreviousYearsCount = 5
            };  
           
            payrollRegisterEarningsEntry = new PayrollRegisterEarningsEntry("REG", 1000, 1000, 0, 40, 20, HourlySalaryIndicator.Hourly);

            referenceUtility = new PayStatementReferenceDataUtility(earningsTypes, earningsDifferentials, taxCodes, benefitDeductionTypes, leaveTypes, positions, payStatementConfiguration);
        }

        [TestMethod]
        public void GetEarningsTypeTest()
        {
            var expected = earningsTypes.FirstOrDefault();
            var actual = referenceUtility.GetEarningsType(expected.Id);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetEarningsTypeAsStipendTest()
        {
            var earnRef = earningsTypes.Last();
            var expected = new EarningsType(earnRef.Id, earnRef.Description + " - Stipend", earnRef.IsActive, earnRef.Category, earnRef.Method, 1);

            Assert.AreEqual(expected, referenceUtility.GetEarningsTypeAsStipend(earnRef.Id));
        }

        [TestMethod]
        public void GetEarningsTypeAsStipendReturnsNullTest()
        {
            var actual = referenceUtility.GetEarningsTypeAsStipend("foo");
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void GetEarnTypeLeaveTimeTypeTest()
        {
            var earnType = earningsTypes.FirstOrDefault();
            var expected = referenceUtility.GetLeaveTimeType(earnType.EarningsLeaveType);
            var actual = referenceUtility.GetEarnTypeLeaveTimeType(earnType.Id);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetLeaveTypeLeaveTimeTypeTest()
        {
            var leaveType = leaveTypes.FirstOrDefault();
            var expected = referenceUtility.GetLeaveTimeType(leaveType);
            var actual = referenceUtility.GetLeaveTypeLeaveTimeType(leaveType.Code);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetLeaveTimeTypeForNullLeaveTypeReturnsNoneTest()
        {
            var actual = referenceUtility.GetLeaveTimeType(null);

            Assert.AreEqual(LeaveTypeCategory.None, actual);
        }

        [TestMethod]
        public void GetEarningsDifferentialTest()
        {
            var expected = earningsDifferentials.FirstOrDefault();
            var actual = referenceUtility.GetEarningsDifferential(expected.Code);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTaxCodeTest()
        {
            var expected = taxCodes.FirstOrDefault();
            var actual = referenceUtility.GetTaxCode(expected.Code);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTaxCodesForTypeTest()
        {
            var taxCode = taxCodes.FirstOrDefault();
            var targetType = taxCode.Type;
            var expected = taxCodes.Where(tax => tax.Type == targetType);
            var actual = referenceUtility.GetTaxCodesForType(targetType);

            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        [TestMethod]
        public void GetBenefitDeductionTypeTest()
        {
            var expected = benefitDeductionTypes.FirstOrDefault();
            var actual = referenceUtility.GetBenefitDeductionType(expected.Id);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LeaveTypeTest()
        {
            var leaveType = leaveTypes.FirstOrDefault();
            var expected = leaveType;
            var actual = referenceUtility.GetLeaveType(leaveType.Code);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PositionTest()
        {
            var positionDict = positions.ToDictionary(p => p.Id);
            var targetPositionId = positions.FirstOrDefault().Id;
            var expected = positionDict.ContainsKey(targetPositionId) ? positionDict[targetPositionId] : null;
            var actual = referenceUtility.GetPosition(targetPositionId);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void UnknownPositionReturnsNullTest()
        {
            var positionDict = positions.ToDictionary(p => p.Id);
            var targetPositionId = "foo";
            var expected = positionDict.ContainsKey(targetPositionId) ? positionDict[targetPositionId] : null;
            var actual = referenceUtility.GetPosition(targetPositionId);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ConfigurationTest()
        {
            Assert.AreEqual(payStatementConfiguration, referenceUtility.Configuration);
        }
    }
}
