/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayStatementDeductionTests
    {
        PayrollRegisterTaxEntry expectedPayrollRegisterTaxEntry;
        TaxCode expectedTaxCode;
        List<PayrollRegisterTaxEntry> expectedYearToDateTaxEntries;

        BenefitDeductionType expectedBenefitDeductionType;
        PayrollRegisterBenefitDeductionEntry expectedPayrollRegisterBenefitDeductionEntry;
        List<PayrollRegisterBenefitDeductionEntry> expectedYearToDateBenefitDeductionEntries;

        [TestClass]
        public class TaxTests : PayStatementDeductionTests
        {
            [TestInitialize]
            public void Initialize()
            {
                expectedYearToDateTaxEntries = new List<PayrollRegisterTaxEntry>();
                expectedTaxCode = new TaxCode("FED", "Federal Tax", TaxCodeType.FederalWithholding);

                expectedPayrollRegisterTaxEntry = new PayrollRegisterTaxEntry(expectedTaxCode.Code, PayrollTaxProcessingCode.Regular);
                expectedPayrollRegisterTaxEntry.EmployeeTaxableAmount = 1000.00m;
                expectedPayrollRegisterTaxEntry.EmployeeTaxAmount = 100.00m;
                expectedPayrollRegisterTaxEntry.EmployerTaxableAmount = 1000.00m;
                expectedPayrollRegisterTaxEntry.EmployerTaxAmount = 90.00m;

                expectedYearToDateTaxEntries.Add(expectedPayrollRegisterTaxEntry);

                expectedYearToDateTaxEntries.Add(
                    new PayrollRegisterTaxEntry(expectedTaxCode.Code, PayrollTaxProcessingCode.Regular)
                    {
                        EmployeeTaxAmount = 44.00m,
                        EmployerTaxAmount = 44.00m,
                        EmployeeTaxableAmount = 4444.00m,
                        EmployerTaxableAmount = 4444.00m
                    });

                expectedYearToDateTaxEntries.Add(
                    new PayrollRegisterTaxEntry("VAST", PayrollTaxProcessingCode.Regular)
                    {
                        EmployeeTaxAmount = 55.00m,
                        EmployerTaxAmount = 55.00m,
                        EmployeeTaxableAmount = 5555.00m,
                        EmployerTaxableAmount = 5555.00m
                    });
            }

            [TestMethod]
            public void PropertiesAreSetForTaxWithPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(expectedTaxCode, expectedPayrollRegisterTaxEntry, expectedYearToDateTaxEntries);
                Assert.AreEqual(expectedTaxCode.Code, actual.Code);
                Assert.AreEqual(expectedTaxCode.Description, actual.Description);
                Assert.AreEqual(PayStatementDeductionType.Tax, actual.Type);
                Assert.AreEqual(expectedPayrollRegisterTaxEntry.EmployeeTaxAmount, actual.EmployeePeriodAmount);
                Assert.AreEqual(expectedPayrollRegisterTaxEntry.EmployerTaxAmount, actual.EmployerPeriodAmount);
                Assert.AreEqual(expectedPayrollRegisterTaxEntry.EmployeeTaxableAmount, actual.ApplicableGrossPeriodAmount);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullTaxCodeForTaxWithPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(null, expectedPayrollRegisterTaxEntry, expectedYearToDateTaxEntries);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullPeriodTaxEntryForTaxWithPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(expectedTaxCode, null, expectedYearToDateTaxEntries);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullYearToDateTaxEntriesForTaxWithPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(expectedTaxCode, expectedPayrollRegisterTaxEntry, null);
            }

            [TestMethod]
            public void PropertiesAreSetForTaxWithOutPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(expectedTaxCode, expectedYearToDateTaxEntries);
                Assert.AreEqual(expectedTaxCode.Code, actual.Code);
                Assert.AreEqual(expectedTaxCode.Description, actual.Description);
                Assert.AreEqual(PayStatementDeductionType.Tax, actual.Type);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullTaxCodeForTaxWithOutPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(null, expectedYearToDateTaxEntries);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullPeriodTaxEntryForTaxWithOutPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(expectedTaxCode, null);
            }

            [TestMethod]
            public void EmployeeYearToDateAmountTaxTest()
            {
                var actual = new PayStatementDeduction(expectedTaxCode, expectedPayrollRegisterTaxEntry, expectedYearToDateTaxEntries);
                var expected = expectedYearToDateTaxEntries.Where(y => y.TaxCode == expectedTaxCode.Code).Sum(y => y.EmployeeTaxAmount);
                Assert.AreEqual(expected, actual.EmployeeYearToDateAmount);
            }

            [TestMethod]
            public void EmployerYearToDateAmountTaxTest()
            {
                var actual = new PayStatementDeduction(expectedTaxCode, expectedPayrollRegisterTaxEntry, expectedYearToDateTaxEntries);
                var expected = expectedYearToDateTaxEntries.Where(y => y.TaxCode == expectedTaxCode.Code).Sum(y => y.EmployerTaxAmount);
                Assert.AreEqual(expected, actual.EmployerYearToDateAmount);
            }

            [TestMethod]
            public void ApplicableGrossYearToDateAmountTaxTest()
            {
                var actual = new PayStatementDeduction(expectedTaxCode, expectedPayrollRegisterTaxEntry, expectedYearToDateTaxEntries);
                var expected = expectedYearToDateTaxEntries.Where(y => y.TaxCode == expectedTaxCode.Code).Sum(y => y.EmployeeTaxableAmount);
                Assert.AreEqual(expected, actual.ApplicableGrossYearToDateAmount);
            }
        }

        [TestClass]
        public class BendedTests : PayStatementDeductionTests
        {
            [TestInitialize]
            public void Initialize()
            {
                expectedYearToDateBenefitDeductionEntries = new List<PayrollRegisterBenefitDeductionEntry>();
                expectedBenefitDeductionType = new BenefitDeductionType("MEDI", "Medical Insurance", "Self-Service Medical Insurance", BenefitDeductionTypeCategory.Benefit);

                expectedPayrollRegisterBenefitDeductionEntry = new PayrollRegisterBenefitDeductionEntry(expectedBenefitDeductionType.Id);
                expectedPayrollRegisterBenefitDeductionEntry.EmployeeAmount = 111.11m;
                expectedPayrollRegisterBenefitDeductionEntry.EmployeeBasisAmount = 4566.00m;
                expectedPayrollRegisterBenefitDeductionEntry.EmployerAmount = 333.33m;
                expectedPayrollRegisterBenefitDeductionEntry.EmployerBasisAmount = 4566.00m;

                expectedYearToDateBenefitDeductionEntries.Add(expectedPayrollRegisterBenefitDeductionEntry);
                expectedYearToDateBenefitDeductionEntries.Add(
                    new PayrollRegisterBenefitDeductionEntry(expectedBenefitDeductionType.Id)
                    {
                        EmployeeAmount = 111.11m,
                        EmployeeBasisAmount = 4566.00m,
                        EmployerAmount = 333.33m,
                        EmployerBasisAmount = 4566.00m
                    });

                expectedYearToDateBenefitDeductionEntries.Add(
                    new PayrollRegisterBenefitDeductionEntry("LIFE")
                    {
                        EmployeeAmount = 343.99m,
                        EmployeeBasisAmount = 4566.00m,
                        EmployerAmount = 56.00M,
                        EmployerBasisAmount = 4566.00m
                    });
            }

            [TestMethod]
            public void PropertiesAreSetForBenefitWithPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedPayrollRegisterBenefitDeductionEntry, expectedYearToDateBenefitDeductionEntries);
                Assert.AreEqual(expectedBenefitDeductionType.Id, actual.Code);
                //Assert.AreEqual(expectedBenefitDeductionType.Description, actual.Description);
                Assert.AreEqual(PayStatementDeductionType.Benefit, actual.Type);
                Assert.AreEqual(expectedPayrollRegisterBenefitDeductionEntry.EmployeeAmount, actual.EmployeePeriodAmount);
                Assert.AreEqual(expectedPayrollRegisterBenefitDeductionEntry.EmployerAmount, actual.EmployerPeriodAmount);
                Assert.AreEqual(expectedPayrollRegisterBenefitDeductionEntry.EmployeeBasisAmount, actual.ApplicableGrossPeriodAmount);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullBenefitDeductionTypeForBenefitWithPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(null, expectedPayrollRegisterBenefitDeductionEntry, expectedYearToDateBenefitDeductionEntries);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullBenefitDeductionEntryForBenefitWithPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, null, expectedYearToDateBenefitDeductionEntries);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullBenefitDeductionEntriesForBenefitWithPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedPayrollRegisterBenefitDeductionEntry, null);
            }

            [TestMethod]
            public void PropertiesAreSetForBenefitWithOutPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedYearToDateBenefitDeductionEntries);
                Assert.AreEqual(expectedBenefitDeductionType.Id, actual.Code);
                //Assert.AreEqual(expectedBenefitDeductionType.Description, actual.Description);
                Assert.AreEqual(PayStatementDeductionType.Benefit, actual.Type);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullBenefitDeductionTypeForBenefitWithOutPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(null, expectedYearToDateBenefitDeductionEntries);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullBenefitDeductionEntriesForBenefitWithOutPeriodEntryTest()
            {
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, null);
            }

            [TestMethod]
            public void EmployeeYearToDateAmountBenefitTest()
            {
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedPayrollRegisterBenefitDeductionEntry, expectedYearToDateBenefitDeductionEntries);
                var expected = expectedYearToDateBenefitDeductionEntries.Where(y => y.BenefitDeductionId == expectedBenefitDeductionType.Id).Sum(y => y.EmployeeAmount);
                Assert.AreEqual(expected, actual.EmployeeYearToDateAmount);
            }

            [TestMethod]
            public void EmployerYearToDateAmountBenefitTest()
            {
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedPayrollRegisterBenefitDeductionEntry, expectedYearToDateBenefitDeductionEntries);
                var expected = expectedYearToDateBenefitDeductionEntries.Where(y => y.BenefitDeductionId == expectedBenefitDeductionType.Id).Sum(y => y.EmployerAmount);
                Assert.AreEqual(expected, actual.EmployerYearToDateAmount);
            }

            [TestMethod]
            public void ApplicableGrossYearToDateAmountBenefitTest()
            {
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedPayrollRegisterBenefitDeductionEntry, expectedYearToDateBenefitDeductionEntries);
                var expected = expectedYearToDateBenefitDeductionEntries.Where(y => y.BenefitDeductionId == expectedBenefitDeductionType.Id).Sum(y => y.EmployeeBasisAmount);
                Assert.AreEqual(expected, actual.ApplicableGrossYearToDateAmount);
            }

            [TestMethod]
            public void DescriptionWhenNoSelfServiceDescriptionTest()
            {
                expectedBenefitDeductionType = new BenefitDeductionType("MEDI", "Medical Insurance", null, BenefitDeductionTypeCategory.Benefit);
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedPayrollRegisterBenefitDeductionEntry, expectedYearToDateBenefitDeductionEntries);
                Assert.AreEqual(expectedBenefitDeductionType.Description, actual.Description);
                actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedYearToDateBenefitDeductionEntries);
                Assert.AreEqual(expectedBenefitDeductionType.Description, actual.Description);
            }

            [TestMethod]
            public void TypeIsBenefitTest()
            {
                expectedBenefitDeductionType = new BenefitDeductionType("MEDI", "Medical Insurance", "Self - Service Medical Insurance", BenefitDeductionTypeCategory.Benefit);
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedPayrollRegisterBenefitDeductionEntry, expectedYearToDateBenefitDeductionEntries);
                Assert.AreEqual(expectedBenefitDeductionType.Category.ToString(), actual.Type.ToString());
                actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedYearToDateBenefitDeductionEntries);
                Assert.AreEqual(expectedBenefitDeductionType.Category.ToString(), actual.Type.ToString());
            }

            [TestMethod]
            public void TypeIsDeductionTest()
            {
                expectedBenefitDeductionType = new BenefitDeductionType("DED", "A Deduction", "Self - Service Deduction", BenefitDeductionTypeCategory.Deduction);
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedPayrollRegisterBenefitDeductionEntry, expectedYearToDateBenefitDeductionEntries);
                Assert.AreEqual(expectedBenefitDeductionType.Category.ToString(), actual.Type.ToString());
                actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedYearToDateBenefitDeductionEntries);
                Assert.AreEqual(expectedBenefitDeductionType.Category.ToString(), actual.Type.ToString());
            }

            [TestMethod]
            public void ZeroYTDAmountIsFalseWhenEmployeeAmountNotZeroTest()
            {
                expectedYearToDateBenefitDeductionEntries = new List<PayrollRegisterBenefitDeductionEntry>();
                expectedYearToDateBenefitDeductionEntries.Add(
                    new PayrollRegisterBenefitDeductionEntry(expectedBenefitDeductionType.Id)
                    {
                        EmployeeAmount = 343.99m,
                        EmployeeBasisAmount = 4566.00m,
                        EmployerAmount = 0m,
                        EmployerBasisAmount = 0m
                    });
                var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedPayrollRegisterBenefitDeductionEntry, expectedYearToDateBenefitDeductionEntries);
                Assert.IsFalse(actual.IsZeroYearToDateAmount);
            }

            //[TestMethod]
            //public void ZeroYTDAmountIsFalseWhenEmployerAmountNotZeroTest()
            //{
            //    expectedYearToDateBenefitDeductionEntries = new List<PayrollRegisterBenefitDeductionEntry>();
            //    expectedYearToDateBenefitDeductionEntries.Add(
            //        new PayrollRegisterBenefitDeductionEntry(expectedBenefitDeductionType.Id)
            //        {
            //            EmployeeAmount = 0m,
            //            EmployeeBasisAmount = 0m,
            //            EmployerAmount = 333.33m,
            //            EmployerBasisAmount = 4566.00m
            //        });
            //    var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedPayrollRegisterBenefitDeductionEntry, expectedYearToDateBenefitDeductionEntries);
            //    Assert.IsFalse(actual.IsZeroYearToDateAmount);
            //}

            //[TestMethod]
            //public void ZeroYTDAmountIsTrueWhenAmountsAreNullTest()
            //{
            //    expectedYearToDateBenefitDeductionEntries = new List<PayrollRegisterBenefitDeductionEntry>();
            //    expectedYearToDateBenefitDeductionEntries.Add(
            //        new PayrollRegisterBenefitDeductionEntry(expectedBenefitDeductionType.Id)
            //        {
            //            EmployeeAmount = null,
            //            EmployeeBasisAmount = 0m,
            //            EmployerAmount = null,
            //            EmployerBasisAmount = 0m
            //        });
            //    var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedYearToDateBenefitDeductionEntries);
            //    Assert.IsTrue(actual.IsZeroYearToDateAmount);
            //}

            //[TestMethod]
            //public void ZeroYTDAmountIsTrueTest()
            //{
            //    expectedYearToDateBenefitDeductionEntries = new List<PayrollRegisterBenefitDeductionEntry>();
            //    expectedYearToDateBenefitDeductionEntries.Add(
            //        new PayrollRegisterBenefitDeductionEntry(expectedBenefitDeductionType.Id)
            //        {
            //            EmployeeAmount = 0m,
            //            EmployeeBasisAmount = 0m,
            //            EmployerAmount = 0m,
            //            EmployerBasisAmount = 0m
            //        });
            //    var actual = new PayStatementDeduction(expectedBenefitDeductionType, expectedYearToDateBenefitDeductionEntries);
            //    Assert.IsTrue(actual.IsZeroYearToDateAmount);
            //}

        }
    }
}
