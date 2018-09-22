/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class StudentAwardYearTests
    {
        [TestClass]
        public class StudentAwardYearConstructorTests
        {
            private string studentId;
            private string awardYearCode;
            private FinancialAidOffice currentOffice;
            private bool isApplicationReviewed;
            private bool isPaperCopyOptionSelected;            

            private StudentAwardYear studentAwardYear;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003914";
                awardYearCode = "2014";
                currentOffice = new FinancialAidOffice("Office");
                isApplicationReviewed = true;
                isPaperCopyOptionSelected = true;

                studentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice);
            }

            [TestMethod]
            public void NumberOfPropertiesTest()
            {
                var studentAwardYearProperties = typeof(StudentAwardYear).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(22, studentAwardYearProperties.Count());
            }

            [TestMethod]
            public void StudentIdTest()
            {
                Assert.AreEqual(studentId, studentAwardYear.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                new StudentAwardYear("", awardYearCode, currentOffice);
            }

            [TestMethod]
            public void AwardYearCodeTest()
            {
                Assert.AreEqual(awardYearCode, studentAwardYear.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequiredTest()
            {
                new StudentAwardYear(studentId, "", currentOffice);
            }

            [TestMethod]
            public void OfficeTest()
            {
                Assert.AreEqual(currentOffice, studentAwardYear.CurrentOffice);
            }

            [TestMethod]
            public void OfficeIdTest()
            {
                Assert.AreEqual(currentOffice.Id, studentAwardYear.FinancialAidOfficeId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OfficeRequiredTest()
            {
                new StudentAwardYear(studentId, awardYearCode, null);
            }

            [TestMethod]
            public void NullConfigurations_NullCurrentConfigurationTest()
            {
                currentOffice = new FinancialAidOffice("Office");
                Assert.IsNull(studentAwardYear.CurrentConfiguration);
            }

            [TestMethod]
            public void NoConfigurationForAwardYear_NullCurrentConfigurationTest()
            {
                currentOffice.AddConfiguration(new FinancialAidConfiguration(currentOffice.Id, "foobar"));
                Assert.IsNull(studentAwardYear.CurrentConfiguration);
            }

            [TestMethod]
            public void CurrentConfigurationTest()
            {
                var configuration = new FinancialAid.Entities.FinancialAidConfiguration(currentOffice.Id, studentAwardYear.Code);
                currentOffice.AddConfiguration(configuration);

                Assert.AreEqual(configuration, studentAwardYear.CurrentConfiguration);
            }

            [TestMethod]
            public void NullCurrentConfiguration_IsActiveFalseTest()
            {
                currentOffice = new FinancialAidOffice("Office");
                Assert.IsFalse(studentAwardYear.IsActive);
            }

            [TestMethod]
            public void ConfigurationDrivesIsActiveTest()
            {
                var configuration = new FinancialAidConfiguration(currentOffice.Id, studentAwardYear.Code)
                {
                    IsSelfServiceActive = true
                };
                currentOffice.AddConfiguration(configuration);

                Assert.IsTrue(studentAwardYear.IsActive);

                configuration.IsSelfServiceActive = false;
                Assert.IsFalse(studentAwardYear.IsActive);
            }

            [TestMethod]
            public void NullCurrentConfiguration_LoanRequestsAllowedFalseTest()
            {
                currentOffice = new FinancialAidOffice("Office");
                Assert.IsFalse(studentAwardYear.CanRequestLoan);
            }

            [TestMethod]
            public void ConfigurationDrivesLoanRequestsAllowed()
            {
                var configuration = new FinancialAidConfiguration(currentOffice.Id, studentAwardYear.Code)
                {
                    AreLoanRequestsAllowed = true
                };
                currentOffice.AddConfiguration(configuration);

                Assert.IsTrue(studentAwardYear.CanRequestLoan);

                configuration.AreLoanRequestsAllowed = false;
                Assert.IsFalse(studentAwardYear.CanRequestLoan);
            }

            [TestMethod]
            public void NullCurrentConfiguration_IsAwardingActive()
            {
                currentOffice = new FinancialAidOffice("Office");
                Assert.IsFalse(studentAwardYear.IsAwardingActive);
            }

            [TestMethod]
            public void ConfigurationDrivesIsAwardingActive()
            {
                var configuration = new FinancialAidConfiguration(currentOffice.Id, studentAwardYear.Code)
                {
                    IsAwardingActive = true
                };
                currentOffice.AddConfiguration(configuration);

                Assert.IsTrue(studentAwardYear.IsAwardingActive);

                configuration.IsAwardingActive = false;
                Assert.IsFalse(studentAwardYear.IsAwardingActive);
            }

            [TestMethod]
            public void NullCurrentConfiguration_AreAwardingChangesAllowed()
            {
                currentOffice = new FinancialAidOffice("Office");
                Assert.IsFalse(studentAwardYear.AreAwardingChangesAllowed);
            }

            [TestMethod]
            public void ConfigurationDrivesAwardingChangesAllowed()
            {
                var configuration = new FinancialAidConfiguration(currentOffice.Id, studentAwardYear.Code)
                {
                    AreAwardChangesAllowed = true
                };
                currentOffice.AddConfiguration(configuration);

                Assert.IsTrue(studentAwardYear.AreAwardingChangesAllowed);

                configuration.AreAwardChangesAllowed = false;
                Assert.IsFalse(studentAwardYear.AreAwardingChangesAllowed);
            }

            [TestMethod]
            public void NullConfiguration_NeedsProfileFalseTest()
            {
                currentOffice = new FinancialAidOffice("Office");
                Assert.IsFalse(studentAwardYear.NeedsProfileApplication);
            }

            [TestMethod]
            public void ConfigurationDrivesNeedsProfileTest()
            {
                var configuration = new FinancialAidConfiguration(currentOffice.Id, studentAwardYear.Code)
                {
                    IsProfileActive = true
                };
                currentOffice.AddConfiguration(configuration);


                Assert.IsTrue(studentAwardYear.NeedsProfileApplication);

                configuration.IsProfileActive = false;
                Assert.IsFalse(studentAwardYear.NeedsProfileApplication);
            }

            /// <summary>
            /// Test if SuppressInstanceData is set to false if configuration is null
            /// </summary>
            [TestMethod]
            public void NullConfiguration_SuppressInstanceDataFalseTest()
            {
                currentOffice = new FinancialAidOffice("Office");
                Assert.IsFalse(studentAwardYear.SuppressInstanceData);
            }

            /// <summary>
            /// Test if SuppressInstanceData gets set to the correct value whenever
            /// the corresponding value of the configuration object gets changed
            /// </summary>
            [TestMethod]
            public void ConfigurationDrivesSuppressInstanceData()
            {
                var configuration = new FinancialAidConfiguration(currentOffice.Id, studentAwardYear.Code)
                {
                    SuppressInstanceData = true
                };
                currentOffice.AddConfiguration(configuration);


                Assert.IsTrue(studentAwardYear.SuppressInstanceData);

                configuration.SuppressInstanceData = false;
                Assert.IsFalse(studentAwardYear.SuppressInstanceData);
            }            

            [TestMethod]
            public void IsApplicationReviewInitFalseTest()
            {
                Assert.AreEqual(false, studentAwardYear.IsApplicationReviewed);
            }

            [TestMethod]
            public void IsApplicationReviewedGetSetTest()
            {
                studentAwardYear.IsApplicationReviewed = isApplicationReviewed;
                Assert.AreEqual(isApplicationReviewed, studentAwardYear.IsApplicationReviewed);
            }

            [TestMethod]
            public void IsPaperCopyOptionSelectedGetSetTest()
            {
                studentAwardYear.IsPaperCopyOptionSelected = isPaperCopyOptionSelected;
                Assert.AreEqual(isPaperCopyOptionSelected, studentAwardYear.IsPaperCopyOptionSelected);
            }

            [TestMethod]
            public void PendingLoanRequestIdGetSetTest()
            {
                var loanRequestId = "5";
                studentAwardYear.PendingLoanRequestId = loanRequestId;
                Assert.AreEqual(loanRequestId, studentAwardYear.PendingLoanRequestId);
            }

            [TestMethod]
            public void NullStudentAwardYearDescription_NoCurrentConfigurationTest()
            {
                currentOffice = new FinancialAidOffice("Office");
                Assert.IsNull(studentAwardYear.Description);
            }

            [TestMethod]
            public void StudentAwardDescriptionIsNotNull_CurrentConfigurationExistsTest()
            {
                awardYearCode = "2014";
                currentOffice.AddConfigurationRange(new List<FinancialAid.Entities.FinancialAidConfiguration>()
                {
                    new FinancialAid.Entities.FinancialAidConfiguration("Office", "2014"){AwardYearDescription = "2014/2015 year"}
                });
                Assert.IsNotNull(studentAwardYear.Description);
            }

            [TestMethod]
            public void TotalEstimatedExpensesGetSetTest()
            {
                var totalEstimatedExpenses = (decimal)1234.56;
                studentAwardYear.TotalEstimatedExpenses = totalEstimatedExpenses;
                Assert.AreEqual(totalEstimatedExpenses, studentAwardYear.TotalEstimatedExpenses);
            }

            [TestMethod]
            public void EstimatedExpensesAdjustmentGetSetTest()
            {
                var estimatedExpensesAdjustment = (decimal)1234.56;
                studentAwardYear.EstimatedExpensesAdjustment = estimatedExpensesAdjustment;
                Assert.AreEqual(estimatedExpensesAdjustment, studentAwardYear.EstimatedExpensesAdjustment);
            }

            [TestMethod]
            public void TotalAwardedAmountGetSetTest()
            {
                var totalAwardedAmount = (decimal)1234.56;
                studentAwardYear.TotalAwardedAmount = totalAwardedAmount;
                Assert.AreEqual(totalAwardedAmount, studentAwardYear.TotalAwardedAmount);
            }

            [TestMethod]
            public void TotalAwardedAmountNegativeValueSetToZeroTest()
            {
                studentAwardYear.TotalAwardedAmount = -1;
                Assert.AreEqual(0, studentAwardYear.TotalAwardedAmount);
            }

            [TestMethod]
            public void EstimatedCostOfAttendance_TotalExpensesAndAdjustmentHaveValueTest()
            {
                decimal? total = 55555;
                decimal? adjustment = 66666;
                studentAwardYear.TotalEstimatedExpenses = total;
                studentAwardYear.EstimatedExpensesAdjustment = adjustment;

                Assert.AreEqual(total + adjustment, studentAwardYear.EstimatedCostOfAttendance.Value);
            }

            [TestMethod]
            public void EstimatedCostOfAttendance_TotalExpensesIsNullTest()
            {
                decimal? total = null;
                decimal? adjustment = 66666;
                studentAwardYear.TotalEstimatedExpenses = total;
                studentAwardYear.EstimatedExpensesAdjustment = adjustment;

                Assert.AreEqual(null, studentAwardYear.EstimatedCostOfAttendance);
            }

            [TestMethod]
            public void EstimatedCostOfAttendance_AdjustmentIsNullTest()
            {
                decimal? total = 55555;
                decimal? adjustment = null;
                studentAwardYear.TotalEstimatedExpenses = total;
                studentAwardYear.EstimatedExpensesAdjustment = adjustment;

                Assert.AreEqual(total, studentAwardYear.EstimatedCostOfAttendance);
            }

            [TestMethod]
            public void EstimatedCostOfAttendance_TotalExpensesAndAdjustmentAreNullTest()
            {
                decimal? total = null;
                decimal? adjustment = null;
                studentAwardYear.TotalEstimatedExpenses = total;
                studentAwardYear.EstimatedExpensesAdjustment = adjustment;

                Assert.AreEqual(null, studentAwardYear.EstimatedCostOfAttendance);
            }

            [TestMethod]
            public void FederallyFlaggedIsirId_InitializedNullTest()
            {
                Assert.AreEqual(null, studentAwardYear.FederallyFlaggedIsirId);
            }

            [TestMethod]
            public void FederallyFlaggedIsirId_GetSetTest()
            {
                var isirId = "53647";
                studentAwardYear.FederallyFlaggedIsirId = isirId;
                Assert.AreEqual(isirId, studentAwardYear.FederallyFlaggedIsirId);
            }

            [TestMethod]
            public void AwardLetterHistoryItemsForYear_InitializedEmptyListTest()
            {
                Assert.IsNotNull(studentAwardYear.AwardLetterHistoryItemsForYear);
                Assert.IsFalse(studentAwardYear.AwardLetterHistoryItemsForYear.Any());
            }

            [TestMethod]
            public void NullCurrentConfiguration_SuppressDisbursementInfoDisplay()
            {
                currentOffice = new FinancialAidOffice("Office");
                Assert.IsTrue(studentAwardYear.SuppressDisbursementInfoDisplay);
            }

            [TestMethod]
            public void ConfigurationDrivesSuppressDisbursementInfoDisplay()
            {
                var configuration = new FinancialAidConfiguration(currentOffice.Id, studentAwardYear.Code)
                {
                    SuppressDisbursementInfoDisplay = false
                };
                currentOffice.AddConfiguration(configuration);

                Assert.IsFalse(studentAwardYear.SuppressDisbursementInfoDisplay);

                configuration.IsAwardingActive = true;
                Assert.IsTrue(studentAwardYear.IsAwardingActive);
            }
        }

        [TestClass]
        public class StudentAwardYearConstructorNoCurrentOfficeTests
        {
            private string studentId;
            private string awardYearCode;
            
            private StudentAwardYear studentAwardYear;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003914";
                awardYearCode = "2014";
                
                studentAwardYear = new StudentAwardYear(studentId, awardYearCode);
            }

            [TestMethod]
            public void ObjectIsNotNullTest()
            {
                Assert.IsNotNull(studentAwardYear);
            }

            [TestMethod]
            [ExpectedException (typeof(ArgumentNullException))]
            public void NullStudentId_ExceptionThrownTest()
            {
                studentAwardYear = new StudentAwardYear(null, awardYearCode);
            }

            [TestMethod]
            [ExpectedException (typeof(ArgumentNullException))]
            public void NullAwardYearCode_ExceptionThrownTest()
            {
                studentAwardYear = new StudentAwardYear(studentId, null);
            }

            [TestMethod]
            public void StudentId_EqualsTest()
            {
                Assert.AreEqual(studentId, studentAwardYear.StudentId);
            }

            [TestMethod]
            public void AwardYearCode_EqualsTest()
            {
                Assert.AreEqual(awardYearCode, studentAwardYear.Code);
            }

            [TestMethod]
            public void NullCurrentOffice_OfficeIdIsNullTest()
            {
                Assert.IsNull(studentAwardYear.FinancialAidOfficeId);
            }

            [TestMethod]
            public void AwardLetterHistoryItemsForYear_InitializedEmptyListTest()
            {
                Assert.IsNotNull(studentAwardYear.AwardLetterHistoryItemsForYear);
                Assert.IsFalse(studentAwardYear.AwardLetterHistoryItemsForYear.Any());
            }
           
        }

        [TestClass]
        public class StudentAwardYearEqualsTests
        {
            private string studentId;
            private string awardYearCode;
            private FinancialAidOffice currentOffice;
            private bool isApplicationReviewed;
            private bool isPaperCopyOptionSelected;
            private StudentAwardYear studentAwardYear;
            
            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003914";
                awardYearCode = "2014";
                currentOffice = new FinancialAidOffice("Office");
                currentOffice.AddConfigurationRange(new List<FinancialAidConfiguration>()
                {
                    new FinancialAid.Entities.FinancialAidConfiguration(currentOffice.Id, "2014"){ AwardYearDescription = "2014/2015 Academic year"}
                });
                isApplicationReviewed = true;
                isPaperCopyOptionSelected = true;
                studentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice);
            }

            [TestMethod]
            public void SameStudentIdAndAwardYearCode_EqualsTest()
            {
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice);
                Assert.AreEqual(studentAwardYear, testStudentAwardYear);
            }

            [TestMethod]
            public void DiffOffice_EqualsTest()
            {
                var testOffice = new FinancialAidOffice("foobar");
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, testOffice);
                Assert.AreEqual(studentAwardYear, testStudentAwardYear);
            }

            [TestMethod]
            public void DiffIsApplicationReviewed_EqualsTest()
            {
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice) { IsApplicationReviewed = isApplicationReviewed };
                Assert.AreEqual(studentAwardYear, testStudentAwardYear);
            }

            [TestMethod]
            public void DiffIsPaperCopyOptionSelected_EqualsTest()
            {
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice) { IsPaperCopyOptionSelected = isPaperCopyOptionSelected };
                Assert.AreEqual(studentAwardYear, testStudentAwardYear);
            }

            [TestMethod]
            public void DiffPaperCopyOptionText_EqualsTest()
            {
                currentOffice.AddConfiguration(new FinancialAidConfiguration(currentOffice.Id, studentAwardYear.Code)
                {
                    PaperCopyOptionText = "Different text"
                });
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice);
                Assert.AreEqual(studentAwardYear, testStudentAwardYear);
            }

            [TestMethod]
            public void DiffStudentId_NotEqualTest()
            {
                var testStudentAwardYear = new StudentAwardYear("foobar", awardYearCode, currentOffice);
                Assert.AreNotEqual(studentAwardYear, testStudentAwardYear);
            }

            [TestMethod]
            public void DiffAwardYearCode_NotEqualTest()
            {
                var testStudentAwardYear = new StudentAwardYear(studentId, "foobar", currentOffice);
                Assert.AreNotEqual(studentAwardYear, testStudentAwardYear);
            }

            [TestMethod]
            public void StudentAwardYearDescription_EqualsTest()
            {
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice);
                Assert.AreEqual(currentOffice.Configurations.FirstOrDefault().AwardYearDescription, testStudentAwardYear.Description);
            }
        }

        [TestClass]
        public class StudentAwardYearHashCodeTests
        {
            private string studentId;
            private string awardYearCode;
            private FinancialAidOffice currentOffice;
            private bool isApplicationReviewed;
            private bool isPaperCopyOptionSelected;
            private StudentAwardYear studentAwardYear;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "0003914";
                awardYearCode = "2014";
                currentOffice = new FinancialAidOffice("Office");
                isApplicationReviewed = true;
                isPaperCopyOptionSelected = true;
                studentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice);
            }

            [TestMethod]
            public void SameStudentIdAndAwardYearCode_SameHashCodeTest()
            {
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice);
                Assert.AreEqual(studentAwardYear.GetHashCode(), testStudentAwardYear.GetHashCode());
            }

            [TestMethod]
            public void DiffOffice_SameHashCodeTest()
            {
                var testOffice = new FinancialAidOffice("foobar");
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice);
                Assert.AreEqual(studentAwardYear.GetHashCode(), testStudentAwardYear.GetHashCode());
            }

            [TestMethod]
            public void DiffIsApplicationReviewed_EqualsTest()
            {
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice) { IsApplicationReviewed = isApplicationReviewed };
                Assert.AreEqual(studentAwardYear.GetHashCode(), testStudentAwardYear.GetHashCode());
            }

            [TestMethod]
            public void DiffIsPaperCopyOptionSelected_SameHashCodeTest()
            {
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice) { IsPaperCopyOptionSelected = isPaperCopyOptionSelected };
                Assert.AreEqual(studentAwardYear.GetHashCode(), testStudentAwardYear.GetHashCode());
            }

            [TestMethod]
            public void DiffStudentId_NotEqualTest()
            {
                var testStudentAwardYear = new StudentAwardYear("foobar", awardYearCode, currentOffice);
                Assert.AreNotEqual(studentAwardYear.GetHashCode(), testStudentAwardYear.GetHashCode());
            }

            [TestMethod]
            public void DiffAwardYearCode_NotEqualTest()
            {
                var testStudentAwardYear = new StudentAwardYear(studentId, "foobar", currentOffice);
                Assert.AreNotEqual(studentAwardYear.GetHashCode(), testStudentAwardYear.GetHashCode());
            }

            [TestMethod]
            public void DiffPaperCopyOptionText_SameHashCodeTest()
            {
                currentOffice.AddConfiguration(new FinancialAidConfiguration(currentOffice.Id, studentAwardYear.Code)
                {
                    PaperCopyOptionText = "Another different text"
                });
                var testStudentAwardYear = new StudentAwardYear(studentId, awardYearCode, currentOffice);
                Assert.AreEqual(studentAwardYear.GetHashCode(), testStudentAwardYear.GetHashCode());
            }
        }
    }
}
