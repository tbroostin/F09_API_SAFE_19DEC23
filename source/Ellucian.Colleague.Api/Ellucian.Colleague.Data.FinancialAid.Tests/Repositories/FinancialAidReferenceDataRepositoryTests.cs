/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Http.Configuration;
using System.Threading;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    /// <summary>
    /// Test class for all repository items in FinancialAidReferenceDataRepository
    /// </summary>
    [TestClass]
    public class FinancialAidReferenceDataRepositoryTests
    {
        #region AwardTests
        /// <summary>
        /// Tests the Award section of the Repository
        /// </summary>
        [TestClass]
        public class AwardTests : BaseRepositorySetup
        {
            private TestFinancialAidReferenceDataRepository expectedRepository;
            private IEnumerable<Award> expectedAwards
            {
                get
                {
                    return expectedRepository.Awards;
                }
            }

            private FinancialAidReferenceDataRepository actualRepository;
            private IEnumerable<Award> actualAwards
            {
                get
                {
                    return actualRepository.Awards;
                }
            }

            private char _VM;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                _VM = Convert.ToChar(DynamicArray.VM);

                expectedRepository = new TestFinancialAidReferenceDataRepository();
                actualRepository = BuildValidReferenceDataRepository();
            }

            private FinancialAidReferenceDataRepository BuildValidReferenceDataRepository()
            {
                dataReaderMock.Setup(reader => reader.BulkReadRecord<Awards>("", false))
                    .Returns<string, bool>((s, b) =>
                    {
                        return new Collection<Awards>(expectedRepository.awardRecordData.Select(award =>
                            new Awards()
                            {
                                Recordkey = award.Code,
                                AwDescription = award.Description,
                                AwExplanationText = award.Explanation,
                                AwCategory = award.Category,
                                AwDlLoanType = award.LoanType,
                                AwShopsheetGroup = award.ShoppingSheetGroup
                            }).ToList());
                    });

                dataReaderMock.Setup(reader => reader.BulkReadRecord<AwardCategories>("AWARD.CATEGORIES", "", true))
                    .Returns<string, string, bool>((s1, s2, b) => BuildAwardCategoriesRecords(expectedRepository.awardCategoryData));

                dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<AwardCategories>("AWARD.CATEGORIES", "", true))
                    .Returns<string, string, bool>((s1, s2, b) => Task.FromResult(BuildAwardCategoriesRecords(expectedRepository.awardCategoryData)));


                return new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public void ActualAwardsExist()
            {
                Assert.IsTrue(expectedAwards.Count() > 0);
                Assert.IsTrue(actualAwards.Count() > 0);
                Assert.AreEqual(expectedAwards.Count(), actualAwards.Count());
            }

            /// <summary>
            /// Test that the repository created Award domain objects correctly by checking
            /// that the Codes are equal.
            /// </summary>
            [TestMethod]
            public void AwardsAreEqualTest()
            {
                foreach (var expectedAward in expectedAwards)
                {
                    var actualAward = actualAwards.FirstOrDefault(a => a.Code == expectedAward.Code);
                    Assert.IsNotNull(actualAward);
                    Assert.AreEqual(expectedAward.Description, actualAward.Description);
                    Assert.AreEqual(expectedAward.Explanation, actualAward.Explanation);
                    Assert.AreEqual(expectedAward.LoanType, actualAward.LoanType);
                    Assert.AreEqual(expectedAward.IsFederalDirectLoan, actualAward.IsFederalDirectLoan);
                }
            }

            [TestMethod]
            public void NoExplanationTest()
            {
                var testAward = expectedRepository.awardRecordData.First();
                testAward.Explanation = string.Empty;

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAward.Code);

                Assert.IsNotNull(actualAward);
                Assert.AreEqual(testAward.Explanation, actualAward.Explanation);
            }

            [TestMethod]
            public void DoubleValueMarksReplacedWithDoubleNewLinesTest()
            {
                var testAward = expectedRepository.awardRecordData.First();
                testAward.Explanation = "textBefore" + _VM + _VM + "textAfter";
                var expectedExplanation = testAward.Explanation.Replace("" + _VM + _VM, Environment.NewLine + Environment.NewLine);

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAward.Code);
                Assert.IsNotNull(actualAward);

                Assert.AreEqual(expectedExplanation, actualAward.Explanation);
            }

            [TestMethod]
            public void SingleValueMarkReplacedWithSpaceTest()
            {
                var testAward = expectedRepository.awardRecordData.First();
                testAward.Explanation = "textBefore" + _VM + "textAfter";
                var expectedExplanation = testAward.Explanation.Replace(_VM, ' ');

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAward.Code);
                Assert.IsNotNull(actualAward);

                Assert.AreEqual(expectedExplanation, actualAward.Explanation);
            }

            [TestMethod]
            public void SpacesInsideExplanationLink_ExplanationFormattedAsExpectedTest()
            {
                var testAward = expectedRepository.awardRecordData.First();
                testAward.Explanation = "<a href=' http: // www.goog le.c   om'>hello</a>";
                var expectedExplanation = "<a href='http://www.google.com'>hello</a>";
                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAward.Code);
                
                Assert.AreEqual(expectedExplanation, actualAward.Explanation);
            }

            [TestMethod]
            public void NewLinesAndSpacesInsideExplanationLink_ExplanationFormattedAsExpectedTest()
            {
                var testAward = expectedRepository.awardRecordData.First();
                testAward.Explanation = "<a href='"+Environment.NewLine+" http: // www.goog le.c"+Environment.NewLine+"   om'>hello</a>";
                var expectedExplanation = "<a href='http://www.google.com'>hello</a>";
                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAward.Code);

                Assert.AreEqual(expectedExplanation, actualAward.Explanation);
            }

            [TestMethod]
            public void ValidAwardCategoryWithLoanFlagSetsLoanTypeTest()
            {
                var loanCategories = expectedRepository.awardCategoryData.Where(c => !string.IsNullOrEmpty(c.LoanFlag) && c.LoanFlag.ToUpper() == "Y");
                var awardsWithLoanCategories = expectedRepository.awardRecordData.Where(a => loanCategories.Select(c => c.Code).Contains(a.Category));

                var actualAwardsWithNonNullLoanTypes = actualAwards.Where(a => a.LoanType.HasValue);

                Assert.AreEqual(awardsWithLoanCategories.Count(), actualAwardsWithNonNullLoanTypes.Count());

                foreach (var expectedLoan in awardsWithLoanCategories)
                {
                    var actualLoan = actualAwards.FirstOrDefault(a => a.Code == expectedLoan.Code);
                    Assert.IsNotNull(actualLoan);
                    Assert.IsTrue(actualLoan.LoanType.HasValue);
                }
            }

            [TestMethod]
            public void NullAwardCategoryDoesBuildAwardTest()
            {
                var testAward = expectedRepository.awardRecordData.First();
                testAward.Category = "foobar";

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAward.Code);
                // Since Category is no longer required, we will always return an award
                // Assert.IsNull(actualAward);
                Assert.IsNotNull(actualAward);
            }

            [TestMethod]
            public void NoLoanFlagDoesNotSetLoanTypeTest()
            {
                var testAward = expectedRepository.awardRecordData.First();
                var testCategory = expectedRepository.awardCategoryData.First(ac => ac.Code == testAward.Category);
                testCategory.LoanFlag = "";

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAward.Code);

                Assert.IsFalse(actualAward.LoanType.HasValue);
            }

            [TestMethod]
            public void LoanTypeValueSetsIsFederalDirectLoanFlagTrueTest()
            {
                var testAward = expectedRepository.awardRecordData.First();
                testAward.LoanType = "foobar";
                var testCategory = expectedRepository.awardCategoryData.First(ac => ac.Code == testAward.Category);
                testCategory.LoanFlag = "Y";

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAward.Code);

                Assert.IsTrue(actualAward.IsFederalDirectLoan);
            }

            [TestMethod]
            public void NoLoanTypeValueSetsIsFederalDirectLoanFlagFalseTest()
            {
                var testAward = expectedRepository.awardRecordData.First();
                testAward.LoanType = string.Empty;
                var testCategory = expectedRepository.awardCategoryData.First(ac => ac.Code == testAward.Category);
                testCategory.LoanFlag = "Y";

                actualRepository = BuildValidReferenceDataRepository();
                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAward.Code);

                Assert.IsFalse(actualAward.IsFederalDirectLoan);
            }

            [TestMethod]
            public void SubsidizedLoanTypeTest()
            {
                var loanCategories = expectedRepository.awardCategoryData.Where(c => !string.IsNullOrEmpty(c.LoanFlag) && c.LoanFlag.ToUpper() == "Y");
                var gslAwards = expectedRepository.awardRecordData.Where(a => a.Category == "GSL" && loanCategories.Select(c => c.Code).Contains(a.Category));

                var actualGslAwards = actualAwards.Where(a => a.LoanType.HasValue && a.LoanType.Value == LoanType.SubsidizedLoan);

                Assert.AreEqual(gslAwards.Count(), actualGslAwards.Count());

                foreach (var expectedGsl in gslAwards)
                {
                    var actualGsl = actualGslAwards.FirstOrDefault(a => a.Code == expectedGsl.Code);
                    Assert.IsNotNull(actualGsl);
                }
            }

            [TestMethod]
            public void UnsubsidizedLoanTypeTest()
            {
                var loanCategories = expectedRepository.awardCategoryData.Where(c => !string.IsNullOrEmpty(c.LoanFlag) && c.LoanFlag.ToUpper() == "Y");
                var ustfAwards = expectedRepository.awardRecordData.Where(a => a.Category == "USTF" && loanCategories.Select(c => c.Code).Contains(a.Category));

                var actualUstfAwards = actualAwards.Where(a => a.LoanType.HasValue && a.LoanType.Value == LoanType.UnsubsidizedLoan);

                Assert.AreEqual(ustfAwards.Count(), actualUstfAwards.Count());

                foreach (var expectedUstf in ustfAwards)
                {
                    var actualUstf = actualUstfAwards.FirstOrDefault(a => a.Code == expectedUstf.Code);
                    Assert.IsNotNull(actualUstf);
                }
            }

            [TestMethod]
            public void GraduatePlusLoanTypeTest()
            {
                var loanCategories = expectedRepository.awardCategoryData.Where(c => !string.IsNullOrEmpty(c.LoanFlag) && c.LoanFlag.ToUpper() == "Y");
                var gplusAwards = expectedRepository.awardRecordData.Where(a => a.Category == "GPLUS" && loanCategories.Select(c => c.Code).Contains(a.Category));

                var actualGplusAwards = actualAwards.Where(a => a.LoanType.HasValue && a.LoanType.Value == LoanType.GraduatePlusLoan);

                Assert.AreEqual(gplusAwards.Count(), actualGplusAwards.Count());

                foreach (var expectedGplus in gplusAwards)
                {
                    var actualGplus = actualGplusAwards.FirstOrDefault(a => a.Code == expectedGplus.Code);
                    Assert.IsNotNull(actualGplus);
                }
            }

            [TestMethod]
            public void OtherLoanTypeTest()
            {
                var loanCategories = expectedRepository.awardCategoryData.Where(c => !string.IsNullOrEmpty(c.LoanFlag) && c.LoanFlag.ToUpper() == "Y");
                var otherLoanAwards = expectedRepository.awardRecordData.Where(a =>
                    a.Category != "GPLUS" &&
                    a.Category != "USTF" &&
                    a.Category != "GSL" &&
                    loanCategories.Select(c => c.Code).Contains(a.Category));

                var actualOtherLoanAwards = actualAwards.Where(a => a.LoanType.HasValue && a.LoanType.Value == LoanType.OtherLoan);

                Assert.AreEqual(otherLoanAwards.Count(), actualOtherLoanAwards.Count());

                foreach (var expectedOtherLoan in otherLoanAwards)
                {
                    var actualOtherLoan = actualOtherLoanAwards.FirstOrDefault(a => a.Code == expectedOtherLoan.Code);
                    Assert.IsNotNull(actualOtherLoan);
                }
            }

            [TestMethod]
            public void NullShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = string.Empty
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNull(actualAward.ShoppingSheetGroup);
            }

            [TestMethod]
            public void UnknownShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = "foobar"
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNull(actualAward.ShoppingSheetGroup);
            }

            [TestMethod]
            public void LowercaseShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = "sc"
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNotNull(actualAward.ShoppingSheetGroup);
                Assert.AreEqual(ShoppingSheetAwardGroup.SchoolGrants, actualAward.ShoppingSheetGroup.Value);
            }

            [TestMethod]
            public void SchoolGrantShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = "SC"
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNotNull(actualAward.ShoppingSheetGroup);
                Assert.AreEqual(ShoppingSheetAwardGroup.SchoolGrants, actualAward.ShoppingSheetGroup.Value);
            }

            [TestMethod]
            public void PellGrantShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = "PL"
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNotNull(actualAward.ShoppingSheetGroup);
                Assert.AreEqual(ShoppingSheetAwardGroup.PellGrants, actualAward.ShoppingSheetGroup.Value);
            }

            [TestMethod]
            public void StateGrantShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = "ST"
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNotNull(actualAward.ShoppingSheetGroup);
                Assert.AreEqual(ShoppingSheetAwardGroup.StateGrants, actualAward.ShoppingSheetGroup.Value);
            }

            [TestMethod]
            public void OtherGrantsShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = "OT"
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNotNull(actualAward.ShoppingSheetGroup);
                Assert.AreEqual(ShoppingSheetAwardGroup.OtherGrants, actualAward.ShoppingSheetGroup.Value);
            }

            [TestMethod]
            public void WorkStudyShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = "WS"
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNotNull(actualAward.ShoppingSheetGroup);
                Assert.AreEqual(ShoppingSheetAwardGroup.WorkStudy, actualAward.ShoppingSheetGroup.Value);
            }

            [TestMethod]
            public void PerkinsLoanShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = "PK"
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNotNull(actualAward.ShoppingSheetGroup);
                Assert.AreEqual(ShoppingSheetAwardGroup.PerkinsLoans, actualAward.ShoppingSheetGroup.Value);
            }

            [TestMethod]
            public void SubsidizedLoanShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = "DS"
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNotNull(actualAward.ShoppingSheetGroup);
                Assert.AreEqual(ShoppingSheetAwardGroup.SubsidizedLoans, actualAward.ShoppingSheetGroup.Value);
            }

            [TestMethod]
            public void UnsubsidizedLoanShoppingSheetGroupTest()
            {
                var testAwardCode = "foobar";
                expectedRepository.awardRecordData.Add(new TestFinancialAidReferenceDataRepository.AwardRecord()
                {
                    Code = testAwardCode,
                    Category = "DOG",
                    Description = "shoppingsheettest",
                    ShoppingSheetGroup = "DU"
                });

                var actualAward = actualAwards.FirstOrDefault(a => a.Code == testAwardCode);
                Assert.IsNotNull(actualAward.ShoppingSheetGroup);
                Assert.AreEqual(ShoppingSheetAwardGroup.UnsubsidizedLoans, actualAward.ShoppingSheetGroup.Value);
            }
        }

        #endregion

        #region AwardStatusTests
        /// <summary>
        /// Test the AwardStatus repository
        /// </summary>
        [TestClass]
        public class AwardStatusTests : BaseRepositorySetup
        {
            /// <summary>
            /// Collection of AwardsActions Data Contract objects to be retreived by the
            /// repo.
            /// </summary>
            Collection<AwardActions> awardStatusResponseData;

            /// <summary>
            /// List of AwardStatus Entity Objects used as the control to compare to the objects
            /// the repository returns
            /// </summary>
            IEnumerable<AwardStatus> expectedAwardStatuses;
            IEnumerable<AwardStatus> actualAwardStatuses;

            TestFinancialAidReferenceDataRepository expectedRepository;
            FinancialAidReferenceDataRepository actualRepository;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestFinancialAidReferenceDataRepository();

                // Get  Award Status Objects
                expectedAwardStatuses = expectedRepository.AwardStatuses;

                // Build the awardPeriodResponseData
                awardStatusResponseData = BuildAwardActionResponse(expectedRepository.AwardActionData);

                // Build award period repository
                actualRepository = BuildValidReferenceDataRepository();
                actualAwardStatuses = actualRepository.AwardStatuses;
            }

            /// <summary>
            /// This BuildResponse method is a bit different. Since we translate the AA.CATEGORY
            /// to an enum, we need the test AwardAction data array, instead of using the AwardStatus
            /// domain objects.
            /// </summary>
            /// <param name="awardActionData">Array of test data that could come from Colleague</param>
            /// <returns>A collection of AwardAction DataContract objects</returns>
            private Collection<AwardActions> BuildAwardActionResponse(string[,] awardActionData)
            {
                Collection<AwardActions> repoAwardActions = new Collection<AwardActions>();
                var numItems = awardActionData.Length / 3;
                for (int i = 0; i < numItems; i++)
                {
                    var action = new AwardActions();
                    action.Recordkey = awardActionData[i, 0];
                    action.AaDescription = awardActionData[i, 1];
                    action.AaCategory = awardActionData[i, 2];

                    repoAwardActions.Add(action);
                }

                return repoAwardActions;
            }

            /// <summary>
            /// Builds a FianncialAidReferenceDataRepository object so that the BulkdReadRecord method returns
            /// our AwardActionResponse
            /// </summary>
            /// <returns></returns>
            private FinancialAidReferenceDataRepository BuildValidReferenceDataRepository()
            {

                dataReaderMock.Setup<Collection<AwardActions>>(apc => apc.BulkReadRecord<AwardActions>("AWARD.ACTIONS", "", true)).Returns(awardStatusResponseData);
                actualRepository = new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return actualRepository;
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                expectedAwardStatuses = null;
                actualRepository = null;
            }

            /// <summary>
            /// Test that the repository created AwardStatus domain objects correctly by checking
            /// that the Codes are equal.
            /// </summary>
            [TestMethod]
            public void AwardStatusCodeTest()
            {
                for (int i = 0; i < expectedAwardStatuses.Count(); i++)
                {
                    Assert.AreEqual(expectedAwardStatuses.ElementAt(i).Code, actualAwardStatuses.ElementAt(i).Code);
                }
            }

            /// <summary>
            /// Test that the repository created AwardStatus domain objects correctly by checking
            /// that the Descriptions are equal.
            /// </summary>
            [TestMethod]
            public void AwardStatusDescriptionTest()
            {
                for (int i = 0; i < expectedAwardStatuses.Count(); i++)
                {
                    Assert.AreEqual(expectedAwardStatuses.ElementAt(i).Description, actualAwardStatuses.ElementAt(i).Description);
                }
            }

            /// <summary>
            /// Test that the repository created AwardStatus domain objects correctly by checking
            /// that the Categories are equal.
            /// </summary>
            [TestMethod]
            public void AwardStatusCategoryTest()
            {
                for (int i = 0; i < expectedAwardStatuses.Count(); i++)
                {
                    Assert.AreEqual(expectedAwardStatuses.ElementAt(i).Category, actualAwardStatuses.ElementAt(i).Category);
                }
            }

            [TestMethod]
            public void BogusAwardStatusCategory_RejectedTest()
            {
                var badCode = "FOO";
                awardStatusResponseData.Add(new AwardActions()
                {
                    Recordkey = badCode,
                    AaDescription = "BAR",
                    AaCategory = "FOOBAR"
                });

                actualRepository = BuildValidReferenceDataRepository();
                actualAwardStatuses = actualRepository.AwardStatuses;

                var foobarStatus = actualAwardStatuses.First(a => a.Code == badCode);
                Assert.AreEqual(AwardStatusCategory.Rejected, foobarStatus.Category);
            }
        }
        #endregion

        #region AwardYearTests

        /// <summary>
        /// Test the AwardYear repository
        /// </summary>
        [TestClass]
        public class AwardYearTests : BaseRepositorySetup
        {

            public TestFinancialAidReferenceDataRepository expectedRepository;
            public IEnumerable<AwardYear> expectedAwardYears
            {
                get
                {
                    return expectedRepository.AwardYears;
                }
            }

            public FinancialAidReferenceDataRepository actualRepository;
            public IEnumerable<AwardYear> actualAwardYears
            {
                get
                {
                    return actualRepository.AwardYears;
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestFinancialAidReferenceDataRepository();
                actualRepository = BuildValidReferenceDataRepository();

            }

            /// <summary>
            /// Build a ReferenceDataRepository to return test data
            /// </summary>
            /// <returns>FinancialAidReferenceDataRepository setup for tests</returns>
            private FinancialAidReferenceDataRepository BuildValidReferenceDataRepository()
            {
                //Setup dataReader to return test DataContract objects instead of db data.
                dataReaderMock.Setup(apc => apc.BulkReadRecord<FaSuites>("", true))
                    .Returns<string, bool>((s, b) => BuildFaSuitesResponse(expectedRepository.SuiteYears));

                // Construct repository
                return new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }


            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
            }

            /// <summary>
            /// Test that the repository created AwardYear domain objects correctly by checking
            /// that the Codes are equal.
            /// </summary>
            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(expectedAwardYears.ToList(), actualAwardYears.ToList());
            }

            [TestMethod]
            public void DataReaderReturnsNullFaSuites_ReturnEmptyListTest()
            {
                expectedRepository.SuiteYears = null;
                Assert.AreEqual(0, actualAwardYears.Count());
            }

            [TestMethod]
            public void DataReaderReturnsEmptyFaSuites_ReturnEmptyListTest()
            {
                expectedRepository.SuiteYears = new List<string>();
                Assert.AreEqual(0, actualAwardYears.Count());
            }

            [TestMethod]
            public void ExceptionCreatingAwardYear_AwardYearNotAddedLogErrorTest()
            {
                expectedRepository.SuiteYears.Add(string.Empty);
                Assert.IsNull(actualAwardYears.FirstOrDefault(y => y.Code == string.Empty));

                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }

            [TestMethod]
            public void AwardYearsAreOrderedTest()
            {
                expectedRepository.SuiteYears.Add("0001");
                var expectedSortedYearCodes = expectedRepository.SuiteYears.OrderBy(y => y);

                var actualSortedYearCodes = actualAwardYears.Select(y => y.Code);

                CollectionAssert.AreEqual(expectedSortedYearCodes.ToList(), actualSortedYearCodes.ToList());
            }

            [TestMethod]
            public void DeletedAwardYearsGetFilteredOutTest()
            {
                var someDeletedYears = BuildFaSuitesResponse(expectedRepository.SuiteYears);
                someDeletedYears.First().FaSuitesStatus = "D";
                dataReaderMock.Setup(apc => apc.BulkReadRecord<FaSuites>("", true))
                    .Returns(someDeletedYears);
                Assert.AreNotEqual(expectedAwardYears.Count(), actualAwardYears.Count());
            }

            [TestMethod]
            public void ArchivedAwardYearsGetFilteredOutTest()
            {
                var someArchivedYears = BuildFaSuitesResponse(expectedRepository.SuiteYears);
                someArchivedYears.Last().FaSuitesStatus = "A";
                dataReaderMock.Setup(apc => apc.BulkReadRecord<FaSuites>("", true))
                    .Returns(someArchivedYears);
                Assert.AreNotEqual(expectedAwardYears.Count(), actualAwardYears.Count());
            }

            [TestMethod]
            public void ObsoletedAwardYearsGetFilteredOutTest()
            {
                var someObsoletedYears = BuildFaSuitesResponse(expectedRepository.SuiteYears);
                someObsoletedYears.Last().FaSuitesStatus = "O";
                dataReaderMock.Setup(apc => apc.BulkReadRecord<FaSuites>("", true))
                    .Returns(someObsoletedYears);
                Assert.AreNotEqual(expectedAwardYears.Count(), actualAwardYears.Count());
            }
        }

        /// <summary>
        /// Helper to build a collection of FaSuites objects for use as a response from the DataReader
        /// Also used by BudgetComponents
        /// </summary>
        /// <param name="suiteYears">Array of test data</param>
        /// <returns>Collection of FaSuites DataContract objects</returns>
        public static Collection<FaSuites> BuildFaSuitesResponse(List<string> suiteYears)
        {
            var faSuiteData = new Collection<FaSuites>();
            if (suiteYears != null)
            {
                for (int i = 0; i < suiteYears.Count(); i++)
                {
                    var suiteYear = new FaSuites();
                    suiteYear.Recordkey = suiteYears[i];
                    faSuiteData.Add(suiteYear);
                }
            }

            return faSuiteData;
        }

        #endregion

        #region AwardPeriodTests
        /// <summary>
        /// Tests of the Award Period with mocked data
        /// </summary>
        [TestClass]
        public class AwardPeriodTests : BaseRepositorySetup
        {
            Collection<AwardPeriods> awardPeriodResponseData;

            IEnumerable<AwardPeriod> allAwardPeriods;

            FinancialAidReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                // Build AwardPeriod from Test FA Ref Data Repo
                allAwardPeriods = new TestFinancialAidReferenceDataRepository().AwardPeriods;

                // Build the awardPeriodResponseData
                awardPeriodResponseData = BuildAwardPeriodResponse(allAwardPeriods);

                // Build award period repository
                referenceDataRepo = BuildValidReferenceDataRepository();
            }

            /// <summary>
            /// Method to build the allAwardPeriods 
            /// </summary>
            /// <param name="allAwardPeriods"></param>
            /// <returns></returns>
            private Collection<AwardPeriods> BuildAwardPeriodResponse(IEnumerable<AwardPeriod> allAwardPeriods)
            {
                Collection<AwardPeriods> repoAwardPeriods = new Collection<AwardPeriods>();
                foreach (var awardPeriod in allAwardPeriods)
                {
                    var AwardPeriodData = new AwardPeriods();
                    AwardPeriodData.Recordkey = awardPeriod.Code;
                    AwardPeriodData.AwdpDesc = awardPeriod.Description;
                    AwardPeriodData.AwdpStartDate = awardPeriod.StartDate;
                    repoAwardPeriods.Add(AwardPeriodData);
                }

                return repoAwardPeriods;
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                allAwardPeriods = null;
                referenceDataRepo = null;
            }

            /// <summary>
            /// Tests that the Award Period code was created correctly
            /// </summary>
            [TestMethod]
            public void GetAwardPeriods()
            {
                for (int i = 0; i < allAwardPeriods.Count(); i++)
                {
                    Assert.AreEqual(allAwardPeriods.ElementAt(i).Code, referenceDataRepo.AwardPeriods.ElementAt(i).Code);
                }
            }

            /// <summary>
            /// Test to make sure the Award Period Start Date get set the way we defined the data to be
            /// </summary>
            [TestMethod]
            public void TestAwdPdStartDt()
            {
                for (int i = 0; i < allAwardPeriods.Count(); i++)
                {
                    Assert.AreEqual(allAwardPeriods.ElementAt(i).StartDate, referenceDataRepo.AwardPeriods.ElementAt(i).StartDate);
                }
            }

            /// <summary>
            /// Tests that the AwardPeriod Description was created correctly
            /// </summary>
            [TestMethod]
            public void Description()
            {
                for (int i = 0; i < allAwardPeriods.Count(); i++)
                {
                    Assert.AreEqual(allAwardPeriods.ElementAt(i).Description, referenceDataRepo.AwardPeriods.ElementAt(i).Description);
                }
            }

            /// <summary>
            /// Build test data from the Test Repo
            /// </summary>
            /// <returns></returns>
            private FinancialAidReferenceDataRepository BuildValidReferenceDataRepository()
            {
                //set up repo response for "all" award period requests

                dataReaderMock.Setup<Collection<AwardPeriods>>(apc => apc.BulkReadRecord<AwardPeriods>("AWARD.PERIODS", "", true)).Returns(awardPeriodResponseData);
                // Construct course repository
                referenceDataRepo = new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }

        }
        #endregion

        #region AwardCategoryTests

        /// <summary>
        /// Helper method to build AwardCategory records. Needed for both AwardCategories and Awards
        /// </summary>
        /// <param name="awardCategoryRecords"></param>
        /// <returns></returns>
        public static Collection<AwardCategories> BuildAwardCategoriesRecords(List<TestFinancialAidReferenceDataRepository.AwardCategoryRecord> awardCategoryRecords)
        {
            return new Collection<AwardCategories>(awardCategoryRecords.Select(category =>
                new AwardCategories()
                {
                    Recordkey = category.Code,
                    AcDescription = category.Description,
                    AcGrantFlag = category.GrantFlag,
                    AcLoanFlag = category.LoanFlag,
                    AcScholarshipFlag = category.ScholarshipFlag,
                    AcWorkFlag = category.WorkStudyFlag
                }).ToList());
        }

        [TestClass]
        public class AwardCategoryTests : BaseRepositorySetup
        {

            private Collection<AwardCategories> awardCategoryResponseData;

            private IEnumerable<AwardCategory> expectedAwardCategories;
            private IEnumerable<AwardCategory> actualAwardCategories;

            private TestFinancialAidReferenceDataRepository expectedRepository;
            private FinancialAidReferenceDataRepository actualRepository;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestFinancialAidReferenceDataRepository();
                expectedAwardCategories = expectedRepository.AwardCategories;

                awardCategoryResponseData = BuildAwardCategoriesRecords(expectedRepository.awardCategoryData);

                actualRepository = BuildRepository();
                //actualAwardCategories = actualRepository.AwardCategories;
                actualAwardCategories = actualRepository.GetAwardCategoriesAsync().Result;
            }

            private FinancialAidReferenceDataRepository BuildRepository()
            {
                dataReaderMock.Setup(dr => dr.BulkReadRecord<AwardCategories>("AWARD.CATEGORIES", "", true)).Returns(awardCategoryResponseData);

                dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<AwardCategories>("AWARD.CATEGORIES", "", true))
                    .Returns<string, string, bool>((s1, s2, b) => Task.FromResult(BuildAwardCategoriesRecords(expectedRepository.awardCategoryData)));

                return new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public void AttributesAreEqualTest()
            {
                Assert.IsNotNull(expectedAwardCategories);
                Assert.IsNotNull(actualAwardCategories);
                Assert.IsTrue(expectedAwardCategories.Count() > 0);
                Assert.IsTrue(actualAwardCategories.Count() > 0);
                Assert.AreEqual(expectedAwardCategories.Count(), actualAwardCategories.Count());

                foreach (var expectedCategory in expectedAwardCategories)
                {
                    var actualCategory = actualAwardCategories.First(ac => ac.Equals(expectedCategory));
                    Assert.AreEqual(expectedCategory.Description, actualCategory.Description);
                    Assert.AreEqual(expectedCategory.AwardCategoryType, actualCategory.AwardCategoryType);
                    Assert.AreEqual(expectedCategory.CategoryLoanType, actualCategory.CategoryLoanType);
                }
            }

            [TestMethod]
            public void AwardCategoryType_LoanTest()
            {
                var loanCategoryRecord = awardCategoryResponseData.First(ac => !string.IsNullOrEmpty(ac.AcLoanFlag) && ac.AcLoanFlag.ToUpper() == "Y");

                var actualCategory = actualAwardCategories.First(ac => ac.Code == loanCategoryRecord.Recordkey);

                Assert.AreEqual(AwardCategoryType.Loan, actualCategory.AwardCategoryType);
            }

            [TestMethod]
            public void AwardCategoryType_GrantTest()
            {
                var loanCategoryRecord = awardCategoryResponseData.First(ac => !string.IsNullOrEmpty(ac.AcGrantFlag) && ac.AcGrantFlag.ToUpper() == "Y");

                var actualCategory = actualAwardCategories.First(ac => ac.Code == loanCategoryRecord.Recordkey);

                Assert.AreEqual(AwardCategoryType.Grant, actualCategory.AwardCategoryType);
            }

            [TestMethod]
            public void AwardCategoryType_ScholarshipTest()
            {
                var loanCategoryRecord = awardCategoryResponseData.First(ac => !string.IsNullOrEmpty(ac.AcScholarshipFlag) && ac.AcScholarshipFlag.ToUpper() == "Y");

                var actualCategory = actualAwardCategories.First(ac => ac.Code == loanCategoryRecord.Recordkey);

                Assert.AreEqual(AwardCategoryType.Scholarship, actualCategory.AwardCategoryType);
            }

            [TestMethod]
            public void AwardCategoryType_WorkTest()
            {
                var loanCategoryRecord = awardCategoryResponseData.First(ac => !string.IsNullOrEmpty(ac.AcWorkFlag) && ac.AcWorkFlag.ToUpper() == "Y");

                var actualCategory = actualAwardCategories.First(ac => ac.Code == loanCategoryRecord.Recordkey);

                Assert.AreEqual(AwardCategoryType.Work, actualCategory.AwardCategoryType);
            }

            [TestMethod]
            public void AwardCategoryType_NoFlagsSetToYes_NullTest()
            {
                var loanCategoryRecord = awardCategoryResponseData.First();
                loanCategoryRecord.AcLoanFlag = string.Empty;
                loanCategoryRecord.AcGrantFlag = "N";
                loanCategoryRecord.AcScholarshipFlag = "foobar";
                loanCategoryRecord.AcWorkFlag = null;

                actualRepository = BuildRepository();
                actualAwardCategories = actualRepository.AwardCategories;

                var actualCategory = actualAwardCategories.First(ac => ac.Code == loanCategoryRecord.Recordkey);

                Assert.AreEqual(null, actualCategory.AwardCategoryType);
            }

            [TestMethod]
            public void AwardCategoryType_MultipleFlagsSetToYes_NullTest()
            {
                var loanCategoryRecord = awardCategoryResponseData.First();
                loanCategoryRecord.AcLoanFlag = "Y";
                loanCategoryRecord.AcGrantFlag = "y";
                loanCategoryRecord.AcScholarshipFlag = "N";
                loanCategoryRecord.AcWorkFlag = null;

                actualRepository = BuildRepository();
                actualAwardCategories = actualRepository.AwardCategories;

                var actualCategory = actualAwardCategories.First(ac => ac.Code == loanCategoryRecord.Recordkey);

                Assert.AreEqual(null, actualCategory.AwardCategoryType);
            }

        }

        #endregion

        #region LinkTests

        /// <summary>
        /// Tests for the Link
        /// </summary>
        [TestClass]
        public class LinkTests : BaseRepositorySetup
        {
            private Collection<FahubLinks> linksResponseData;
            //private FahubParams fahubParamsResponseData;

            private IEnumerable<Link> expectedLinks;
            private List<Link> actualLinks;

            private TestFinancialAidReferenceDataRepository expectedRepository;
            private FinancialAidReferenceDataRepository actualRepository;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                // Build Links from Test FA Ref Data Repo
                expectedRepository = new TestFinancialAidReferenceDataRepository();
                expectedLinks = expectedRepository.Links;

                // Build the linksResponseData
                linksResponseData = BuildLinksResponse(expectedRepository.LinkRecordData.OrderBy(l => l.Position).ToList());

                // Build links repository
                actualRepository = BuildRepository();
                actualLinks = actualRepository.Links.ToList();

            }

            private Collection<FahubLinks> BuildLinksResponse(List<TestFinancialAidReferenceDataRepository.LinkRecord> linkRecordData)
            {
                var links = new Collection<FahubLinks>();
                foreach (var linkRecord in linkRecordData)
                {
                    var link = new FahubLinks()
                    {
                        Recordkey = linkRecord.Id,
                        FahubLinkTitle = linkRecord.Description,
                        FahubLinkType = linkRecord.Type,
                        FahubLinkUrl = linkRecord.Url
                    };
                    links.Add(link);
                }

                return links;
            }

            /// <summary>
            /// Build test data from the Test Repo
            /// </summary>
            /// <returns></returns>
            private FinancialAidReferenceDataRepository BuildRepository()
            {
                //set up repo response for "all" links requests
                dataReaderMock.Setup(dr => dr.BulkReadRecord<FahubLinks>(It.IsAny<string>(), true)).Returns(linksResponseData);

                return new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public void LinkAttributesTest()
            {
                Assert.IsNotNull(expectedLinks);
                Assert.IsNotNull(actualLinks);

                Assert.IsTrue(expectedLinks.Count() > 0);
                Assert.IsTrue(actualLinks.Count() > 0);

                Assert.AreEqual(expectedLinks.Count(), actualLinks.Count());

                foreach (var expectedLink in expectedLinks)
                {
                    var actualLink = actualLinks.First(l => l.Title == expectedLink.Title);
                    Assert.AreEqual(expectedLink.LinkType, actualLink.LinkType);
                    Assert.AreEqual(expectedLink.LinkUrl, actualLink.LinkUrl);
                }
            }

            [TestMethod]
            public void LinksOrder_EqualsExpectedTest()
            {
                for (var i = 0; i < linksResponseData.Count; i++)
                {
                    Assert.AreEqual(linksResponseData[i].FahubLinkTitle, actualLinks[i].Title);
                    Assert.AreEqual(linksResponseData[i].FahubLinkUrl, actualLinks[i].LinkUrl);
                }
            }


            [TestMethod]
            public void NullLinkRecordsReturnsemptyListTest()
            {
                linksResponseData = null;
                actualRepository = BuildRepository();
                actualLinks = actualRepository.Links.ToList();

                Assert.IsNotNull(actualLinks);
                Assert.AreEqual(0, actualLinks.Count());
            }

            [TestMethod]
            public void EmptyLinkRecordsReturnsemptyListTest()
            {
                linksResponseData = new Collection<FahubLinks>();
                actualRepository = BuildRepository();
                actualLinks = actualRepository.Links.ToList();

                Assert.IsNotNull(actualLinks);
                Assert.AreEqual(0, actualLinks.Count());
            }

            [TestMethod]
            public void DefaultLinkTypeIsUserDefinedTest()
            {
                var fakeLink = new FahubLinks()
                {
                    Recordkey = "F",
                    FahubLinkTitle = "FAKE",
                    FahubLinkUrl = "www.fake.com",
                    FahubLinkType = "FOOBAR"
                };
                linksResponseData.Add(fakeLink);
                //fahubParamsResponseData.FahubHelpfulLinks.Add(fakeLink.Recordkey);

                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(l => l.Title == fakeLink.FahubLinkTitle);

                Assert.AreEqual(LinkTypes.User, actualLink.LinkType);
            }

            [TestMethod]
            public void SatisfactoryAcademicProgressLinkTest()
            {
                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(sl => sl.LinkType == LinkTypes.SatisfactoryAcademicProgress);

                Assert.AreEqual(LinkTypes.SatisfactoryAcademicProgress, actualLink.LinkType);
                Assert.IsNotNull(actualLink.Title);
                Assert.IsNotNull(actualLink.LinkUrl);
            }

            [TestMethod]
            public void FAFSALinkTest()
            {
                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(sl => sl.LinkType == LinkTypes.FAFSA);

                Assert.AreEqual(LinkTypes.FAFSA, actualLink.LinkType);
                Assert.IsNotNull(actualLink.Title);
                Assert.IsNotNull(actualLink.LinkUrl);
            }

            [TestMethod]
            public void PROFILELinkTest()
            {
                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(sl => sl.LinkType == LinkTypes.PROFILE);

                Assert.AreEqual(LinkTypes.PROFILE, actualLink.LinkType);
                Assert.IsNotNull(actualLink.Title);
                Assert.IsNotNull(actualLink.LinkUrl);
            }

            [TestMethod]
            public void MPNLinkTest()
            {
                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(sl => sl.LinkType == LinkTypes.MPN);

                Assert.AreEqual(LinkTypes.MPN, actualLink.LinkType);
                Assert.IsNotNull(actualLink.Title);
                Assert.IsNotNull(actualLink.LinkUrl);
            }

            [TestMethod]
            public void ForecasterLinkTest()
            {
                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(sl => sl.LinkType == LinkTypes.Forecaster);

                Assert.AreEqual(LinkTypes.Forecaster, actualLink.LinkType);
                Assert.IsNotNull(actualLink.Title);
                Assert.IsNotNull(actualLink.LinkUrl);
            }

            [TestMethod]
            public void PLUSLinkTest()
            {
                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(sl => sl.LinkType == LinkTypes.PLUS);

                Assert.AreEqual(LinkTypes.PLUS, actualLink.LinkType);
                Assert.IsNotNull(actualLink.Title);
                Assert.IsNotNull(actualLink.LinkUrl);
            }

            [TestMethod]
            public void NSLDSLinkTest()
            {
                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(sl => sl.LinkType == LinkTypes.NSLDS);

                Assert.AreEqual(LinkTypes.NSLDS, actualLink.LinkType);
                Assert.IsNotNull(actualLink.Title);
                Assert.IsNotNull(actualLink.LinkUrl);
            }

            [TestMethod]
            public void EntranceInterviewLinkTest()
            {
                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(sl => sl.LinkType == LinkTypes.EntranceInterview);

                Assert.AreEqual(LinkTypes.EntranceInterview, actualLink.LinkType);
                Assert.IsNotNull(actualLink.Title);
                Assert.IsNotNull(actualLink.LinkUrl);
            }

            [TestMethod]
            public void UserLinkTest()
            {
                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(sl => sl.LinkType == LinkTypes.User);

                Assert.AreEqual(LinkTypes.User, actualLink.LinkType);
                Assert.IsNotNull(actualLink.Title);
                Assert.IsNotNull(actualLink.LinkUrl);
            }

            [TestMethod]
            public void FormLinkTest()
            {
                actualRepository = BuildRepository();
                var actualLink = actualRepository.Links.First(sl => sl.LinkType == LinkTypes.User);

                Assert.AreEqual(LinkTypes.User, actualLink.LinkType);
                Assert.IsNotNull(actualLink.Title);
                Assert.IsNotNull(actualLink.LinkUrl);
            }


        }
        #endregion

        #region BudgetComponentTests

        [TestClass]
        public class BudgetComponentTests : BaseRepositorySetup
        {
            public TestFinancialAidReferenceDataRepository expectedRepository;
            public IEnumerable<BudgetComponent> expectedBudgetComponents
            {
                get
                {
                    return expectedRepository.BudgetComponents;
                }
            }

            public FinancialAidReferenceDataRepository actualRepository;
            public IEnumerable<BudgetComponent> actualBudgetComponenets
            {
                get
                {
                    return actualRepository.BudgetComponents;
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                expectedRepository = new TestFinancialAidReferenceDataRepository();
                actualRepository = BuildFinancialAidReferenceDataRepository();
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                Assert.IsTrue(expectedBudgetComponents.Count() > 0);
                Assert.IsTrue(actualBudgetComponenets.Count() > 0);
                Assert.AreEqual(expectedBudgetComponents.Count(), actualBudgetComponenets.Count());

                CollectionAssert.AreEqual(expectedBudgetComponents.ToList(), actualBudgetComponenets.ToList());
            }

            [TestMethod]
            public void NoAwardYearsNoBudgetsTest()
            {
                expectedRepository.SuiteYears = new List<string>();

                Assert.IsNotNull(actualBudgetComponenets);
                Assert.AreEqual(0, actualBudgetComponenets.Count());
            }

            [TestMethod]
            public void BudgetsOnlyRetrievedFor2011AndLaterTest()
            {
                expectedRepository.SuiteYears.Add("2010");
                expectedRepository.BudgetComponentData.Add(new TestFinancialAidReferenceDataRepository.BudgetComponentRecord()
                    {
                        Code = "foobar",
                        AwardYear = "2010",
                        Description = "description"
                    });

                Assert.IsNull(actualBudgetComponenets.FirstOrDefault(b => b.AwardYear == "2010"));
                Assert.IsNull(actualBudgetComponenets.FirstOrDefault(b => b.Code == "foobar"));
            }

            [TestMethod]
            public void NoBudgetComponenetsWithAwardYearTest()
            {
                expectedRepository.SuiteYears.Add("foobar");

                Assert.AreEqual(0, actualBudgetComponenets.Where(b => b.AwardYear == "foobar").Count());
            }

            [TestMethod]
            public void DataReaderReturnsNullFbcAcyrCollection_EmptyBudgetComponentListTest()
            {
                expectedRepository.BudgetComponentData = null;
                Assert.AreEqual(0, actualBudgetComponenets.Count());
            }

            [TestMethod]
            public void DataReaderReturnsEmptyFbcAcyrCollection_EmptyBudgetComponentListTest()
            {
                expectedRepository.BudgetComponentData = new List<TestFinancialAidReferenceDataRepository.BudgetComponentRecord>();
                Assert.AreEqual(0, actualBudgetComponenets.Count());
            }

            [TestMethod]
            public void EmptyShoppingSheetGroupTest()
            {
                expectedRepository.BudgetComponentData.ForEach(budget => budget.ShoppingSheetGroupCode = string.Empty);
                Assert.IsTrue(actualBudgetComponenets.All(budget => !budget.ShoppingSheetGroup.HasValue));
            }

            [TestMethod]
            public void UnknownShoppingSheetGroupTest()
            {
                expectedRepository.BudgetComponentData.ForEach(budget => budget.ShoppingSheetGroupCode = "foobar");
                Assert.IsTrue(actualBudgetComponenets.All(budget => !budget.ShoppingSheetGroup.HasValue));
            }

            [TestMethod]
            public void LowercaseShoppingSheetGroupTest()
            {
                expectedRepository.BudgetComponentData.ForEach(budget => budget.ShoppingSheetGroupCode = "tf");
                Assert.IsTrue(actualBudgetComponenets.All(budget => budget.ShoppingSheetGroup.Value == ShoppingSheetBudgetGroup.TuitionAndFees));
            }

            [TestMethod]
            public void TuitionAndFeesShoppingSheetGroupTest()
            {
                expectedRepository.BudgetComponentData.ForEach(budget => budget.ShoppingSheetGroupCode = "TF");
                Assert.IsTrue(actualBudgetComponenets.All(budget => budget.ShoppingSheetGroup.Value == ShoppingSheetBudgetGroup.TuitionAndFees));
            }

            [TestMethod]
            public void HousingAndMealsShoppingSheetGroupTest()
            {
                expectedRepository.BudgetComponentData.ForEach(budget => budget.ShoppingSheetGroupCode = "HM");
                Assert.IsTrue(actualBudgetComponenets.All(budget => budget.ShoppingSheetGroup.Value == ShoppingSheetBudgetGroup.HousingAndMeals));
            }

            [TestMethod]
            public void BooksAndSuppliesShoppingSheetGroupTest()
            {
                expectedRepository.BudgetComponentData.ForEach(budget => budget.ShoppingSheetGroupCode = "BS");
                Assert.IsTrue(actualBudgetComponenets.All(budget => budget.ShoppingSheetGroup.Value == ShoppingSheetBudgetGroup.BooksAndSupplies));
            }

            [TestMethod]
            public void TransportationShoppingSheetGroupTest()
            {
                expectedRepository.BudgetComponentData.ForEach(budget => budget.ShoppingSheetGroupCode = "TP");
                Assert.IsTrue(actualBudgetComponenets.All(budget => budget.ShoppingSheetGroup.Value == ShoppingSheetBudgetGroup.Transportation));
            }

            [TestMethod]
            public void OtherCostsShoppingSheetGroupTest()
            {
                expectedRepository.BudgetComponentData.ForEach(budget => budget.ShoppingSheetGroupCode = "OC");
                Assert.IsTrue(actualBudgetComponenets.All(budget => budget.ShoppingSheetGroup.Value == ShoppingSheetBudgetGroup.OtherCosts));
            }

            [TestMethod]
            public void ExceptionCreatingBudgetComponent_BudgetNotAddedLogErrorTest()
            {
                expectedRepository.BudgetComponentData.Add(new TestFinancialAidReferenceDataRepository.BudgetComponentRecord()
                    {
                        AwardYear = "2015",
                        Code = "foobar",
                        Description = string.Empty
                    });

                Assert.IsNull(actualBudgetComponenets.FirstOrDefault(budget => budget.Code == "foobar"));

                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }

            private FinancialAidReferenceDataRepository BuildFinancialAidReferenceDataRepository()
            {
                dataReaderMock.Setup(r => r.BulkReadRecord<FaSuites>("", true))
                    .Returns<string, bool>((s, b) => BuildFaSuitesResponse(expectedRepository.SuiteYears));

                dataReaderMock.Setup(r => r.BulkReadRecord<FbcAcyr>(It.IsAny<string>(), "", true))
                    .Returns<string, string, bool>((acyrFile, s, b) =>
                        {
                            if (expectedRepository.BudgetComponentData == null) return null;
                            return new Collection<FbcAcyr>(expectedRepository.BudgetComponentData
                            .Where(budget => acyrFile.EndsWith(budget.AwardYear))
                            .Select(budget =>
                                new FbcAcyr()
                                {
                                    Recordkey = budget.Code,
                                    FbcDesc = budget.Description,
                                    FbcShopsheetGroup = budget.ShoppingSheetGroupCode
                                }
                            ).ToList());
                        }
                    );

                return new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }
        }

        #endregion

        #region Checklist Item Tests

        /// <summary>
        /// Tests for the Checklist Items
        /// </summary>
        [TestClass]
        public class ChecklistItemTests : BaseRepositorySetup
        {

            public List<ChecklistItem> expectedItems
            { get { return expectedRepository.ChecklistItems.ToList(); } }
            public List<ChecklistItem> actualItems
            { get { return actualRepository.ChecklistItems.ToList(); } }

            public TestFinancialAidReferenceDataRepository expectedRepository;
            public FinancialAidReferenceDataRepository actualRepository;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                // Build Links from Test FA Ref Data Repo
                expectedRepository = new TestFinancialAidReferenceDataRepository();

                // Build links repository
                actualRepository = BuildRepository();

            }

            /// <summary>
            /// Build test data from the Test Repo
            /// </summary>
            /// <returns></returns>
            private FinancialAidReferenceDataRepository BuildRepository()
            {
                //set up repo response for checklist items request

                dataReaderMock.Setup(dr => dr.BulkReadRecord<FaChecklistItems>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((c, b) => (expectedRepository.checklistItemData == null) ? null : new Collection<FaChecklistItems>(
                        expectedRepository.checklistItemData.Select(record =>
                        new FaChecklistItems()
                        {
                            Recordkey = record.Code,
                            FciDisplayPosition = record.SortNumber,
                            FciSelfServiceDescription = record.ItemDescription
                        }).ToList()));

                loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);

                return new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public void ItemAttributesTest()
            {
                Assert.IsNotNull(expectedItems);
                Assert.IsNotNull(actualItems);

                Assert.IsTrue(expectedItems.Count() > 0);
                Assert.IsTrue(actualItems.Count() > 0);

                Assert.AreEqual(expectedItems.Count(), actualItems.Count());

                CollectionAssert.AreEqual(expectedItems, actualItems);
            }

            [TestMethod]
            public void NullItemRecordsReturnsemptyListTest()
            {
                expectedRepository.checklistItemData = null;

                Assert.IsNotNull(actualItems);
                Assert.AreEqual(0, actualItems.Count());
            }

            [TestMethod]
            public void EmptyItemRecordsReturnsemptyListTest()
            {
                expectedRepository.checklistItemData = new List<TestFinancialAidReferenceDataRepository.FAChecklistItem>();
                Assert.IsNotNull(actualItems);
                Assert.AreEqual(0, actualItems.Count());
            }

            [TestMethod]
            public void FafsaItemTypeTest()
            {
                Assert.AreEqual("FAFSA", actualItems.First(i => i.ChecklistItemType == ChecklistItemType.FAFSA).ChecklistItemCode);
            }

            [TestMethod]
            public void ProfileItemTypeTest()
            {
                Assert.AreEqual("PROFILE", actualItems.First(i => i.ChecklistItemType == ChecklistItemType.PROFILE).ChecklistItemCode);
            }

            [TestMethod]
            public void ReviewAwardPackageItemTypeTest()
            {
                Assert.AreEqual("ACCAWDPKG", actualItems.First(i => i.ChecklistItemType == ChecklistItemType.ReviewAwardPackage).ChecklistItemCode);
            }

            [TestMethod]
            public void ApplicationReviewItemTypeTest()
            {
                Assert.AreEqual("APPLRVW", actualItems.First(i => i.ChecklistItemType == ChecklistItemType.ApplicationReview).ChecklistItemCode);
            }

            [TestMethod]
            public void CompletedDocumentsItemTypeTest()
            {
                Assert.AreEqual("CMPLREQDOC", actualItems.First(i => i.ChecklistItemType == ChecklistItemType.CompletedDocuments).ChecklistItemCode);
            }

            [TestMethod]
            public void ReviewAwardLetterItemTypeTest()
            {
                Assert.AreEqual("SIGNAWDLTR", actualItems.First(i => i.ChecklistItemType == ChecklistItemType.ReviewAwardLetter).ChecklistItemCode);
            }

            [TestMethod]
            public void UnknownChecklistItemTypeTest()
            {
                expectedRepository.checklistItemData.Add(new TestFinancialAidReferenceDataRepository.FAChecklistItem()
                    {
                        Code = "FOOBAR",
                        ItemDescription = "foo",
                        SortNumber = 1
                    });

                Assert.IsNull(actualItems.FirstOrDefault(i => i.ChecklistItemCode == "FOOBAR"));
                loggerMock.Verify(l => l.Info(It.IsAny<string>(), It.IsAny<object[]>()));
            }

            [TestMethod]
            public void NoDisplayPositionTest()
            {
                var firstItem = expectedRepository.checklistItemData.First();
                firstItem.SortNumber = null;

                Assert.IsNull(actualItems.FirstOrDefault(i => i.ChecklistItemCode == firstItem.Code));
                loggerMock.Verify(l => l.Info(It.IsAny<string>(), It.IsAny<object[]>()));
            }
        }

        #endregion

        #region AcademicProgressStatusesTests

        [TestClass]
        public class AcademicProgressStatusesTest : BaseRepositorySetup
        {
            public TestFinancialAidReferenceDataRepository expectedRepository;
            public IEnumerable<AcademicProgressStatus> expectedStatuses
            {
                get
                {
                    return expectedRepository.GetAcademicProgressStatusesAsync().Result;
                }
            }
            public FinancialAidReferenceDataRepository actualRepository;
            public IEnumerable<AcademicProgressStatus> actualStatuses;
            

            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();
                expectedRepository = new TestFinancialAidReferenceDataRepository();
                actualRepository = BuildActualRepository();
            }

            private FinancialAidReferenceDataRepository BuildActualRepository()
            {
                dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SAP.STATUSES", true))
                    .Returns<string, string, bool>((x, y, z) =>
                    {
                        return Task.FromResult(new ApplValcodes()
                        {
                            Recordkey = "SAP.STATUSES",
                            ValsEntityAssociation = expectedRepository.SapStatusValcodeData.Select(s =>
                                new ApplValcodesVals()
                                {
                                    ValInternalCodeAssocMember = s.Code,
                                    ValExternalRepresentationAssocMember = s.Description
                                }).ToList()
                        });
                    });
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<FaSapStatusInfo>("", true))
                    .Returns<string, bool>((x, y) =>
                    {
                        return Task.FromResult(expectedRepository.SapStatusInfoData == null ? null :
                            new Collection<FaSapStatusInfo>(expectedRepository.SapStatusInfoData
                                .Select(record => new FaSapStatusInfo()
                                {
                                    FssiCategory = record.Category,
                                    FssiStatus = record.Code,
                                    FssiExplained = record.Explained,
                                }).ToList()));
                    });
                loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);

                return new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                CollectionAssert.AreEqual(expectedStatuses.ToList(), actualStatuses.ToList());
            }

            [TestMethod]
            public async Task NullApplValcodesReturnsEmptyListTest()
            {
                ApplValcodes nullValcode = null;
                dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SAP.STATUSES", true)).ReturnsAsync(nullValcode);

                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.AreEqual(0, actualStatuses.Count());
                loggerMock.Verify(l => l.Info("Read Record on the Sap Status Valcode returned null"));
            }

            [TestMethod]
            public async Task DescriptionTest()
            {
                var expectedDescription = "foobar";
                expectedRepository.SapStatusValcodeData.ForEach(v => v.Description = expectedDescription);

                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.IsTrue(actualStatuses.All(s => s.Description == expectedDescription));
            }

            [TestMethod]
            public async Task S_TranslatedToSatisfactoryStatusTest()
            {
                var expectedCategory = AcademicProgressStatusCategory.Satisfactory;
                expectedRepository.SapStatusInfoData.ForEach(s => s.Category = "S");

                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.IsTrue(actualStatuses.All(s => s.Category == expectedCategory));
            }

            [TestMethod]
            public async Task W_TranslatedToSatisfactoryStatusTest()
            {
                var expectedCategory = AcademicProgressStatusCategory.Warning;
                expectedRepository.SapStatusInfoData.ForEach(s => s.Category = "W");

                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.IsTrue(actualStatuses.All(s => s.Category == expectedCategory));
            }

            [TestMethod]
            public async Task U_TranslatedToSatisfactoryStatusTest()
            {
                var expectedCategory = AcademicProgressStatusCategory.Unsatisfactory;
                expectedRepository.SapStatusInfoData.ForEach(s => s.Category = "U");

                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.IsTrue(actualStatuses.All(s => s.Category == expectedCategory));
            }

            [TestMethod]
            public async Task D_TranslatedToDoNotDisplayStatusTest()
            {
                var expectedCategory = AcademicProgressStatusCategory.DoNotDisplay;
                expectedRepository.SapStatusInfoData.ForEach(s => s.Category = "D");

                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.IsTrue(actualStatuses.All(s => s.Category == expectedCategory));
            }

            [TestMethod]
            public async Task NullCategoryStatusTest()
            {
                expectedRepository.SapStatusInfoData.ForEach(s => s.Category = null);
                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.IsTrue(actualStatuses.All(s => !s.Category.HasValue));
            }

            [TestMethod]
            public async Task UnknownCategorytoNullTest()
            {
                expectedRepository.SapStatusInfoData.ForEach(s => s.Category = "foobar");
                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.IsTrue(actualStatuses.All(s => !s.Category.HasValue));
            }

            [TestMethod]
            public async Task ExplanationTest()
            {
                var expectedExplanation = "This is the explanation";
                expectedRepository.SapStatusInfoData.ForEach(s => s.Explained = expectedExplanation);
                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.IsTrue(actualStatuses.All(s => s.Explanation == expectedExplanation));
            }            

            [TestMethod]
            public async Task NullSapStatusInfoDataResultsinNullCategoryAndExplanation()
            {
                expectedRepository.SapStatusInfoData = null;
                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.IsTrue(actualStatuses.All(s => !s.Category.HasValue && string.IsNullOrEmpty(s.Explanation)));
            }

            [TestMethod]
            public async Task SapStatusValueNotFoundInFaSapStatusInfoResultsinNullCategoryandExplanation()
            {

                var bogusSapStatus = "K";
                expectedRepository.SapStatusValcodeData.Add(new TestFinancialAidReferenceDataRepository.SapStatusValcodeVals()
                {
                    Code = bogusSapStatus,
                    Description = "Bogus Description"

                });

                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                var unknownSapStatus = actualStatuses.FirstOrDefault(s => s.Code == bogusSapStatus);
                Assert.IsTrue(!unknownSapStatus.Category.HasValue);
                Assert.IsTrue(string.IsNullOrEmpty(unknownSapStatus.Explanation));
            }

            [TestMethod]
            public async Task TestCatchforNullException()
            {
                var missingCodeValue = expectedRepository.SapStatusValcodeData.First();
                missingCodeValue.Code = null;

                actualStatuses = await actualRepository.GetAcademicProgressStatusesAsync();
                Assert.IsNull(actualStatuses.FirstOrDefault(c => c.Code == missingCodeValue.Code));
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<String>(), It.IsAny<Object[]>()));
            }
        }
        #endregion

        #region AcademicProgressAppealCodes

        [TestClass]
        public class AcademicProgressAppealCodeTests : BaseRepositorySetup
        {
            public TestFinancialAidReferenceDataRepository expectedRepository;
            public IEnumerable<AcademicProgressAppealCode> expectedAppeals
            {
                get { return expectedRepository.GetAcademicProgressAppealCodesAsync().Result; }                
            }

            public FinancialAidReferenceDataRepository actualRepository;
            public IEnumerable<AcademicProgressAppealCode> actualAppeals;
            

            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();
                expectedRepository = new TestFinancialAidReferenceDataRepository();
                actualRepository = BuildActualRepository();
                actualAppeals = await actualRepository.GetAcademicProgressAppealCodesAsync();
            }

            private FinancialAidReferenceDataRepository BuildActualRepository()
            {
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<SapAppealsCodes>(It.IsAny<string>(), It.IsAny<string>(), true))
                    .Returns<string, string, bool>((c, d, b) => (expectedRepository.SapAppealInfoRecords == null) ? null : Task.FromResult(new Collection<SapAppealsCodes>(
                        expectedRepository.SapAppealInfoRecords.Select(record =>
                        new SapAppealsCodes()
                        {
                            Recordkey = record.Code,
                            SapacDesc = record.Description
                        }).ToList())));

                loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);


                return new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }
 

            [TestMethod]
            public void AcademicProgressAppealCodes_EqualExpectedTest()
            {
                CollectionAssert.AreEqual(expectedAppeals.ToList(), actualAppeals.ToList());
            }

            [TestMethod]
            public void AcademicProgressAppealCodes_NotNullOrEmptyTest()
            {
                Assert.IsTrue(actualAppeals != null);
                Assert.IsTrue(actualAppeals.Any());
            }

        }
        #endregion

        #region AwardLetterConfigurations

        [TestClass]
        public class AwardLetterConfigurationsTests : BaseRepositorySetup
        {
            public FunctionEqualityComparer<AwardLetterConfiguration> comparer;
            public TestFinancialAidReferenceDataRepository expectedRepository;
            public List<AwardLetterConfiguration> expectedConfigurations
            {
                get
                {
                    return expectedRepository.GetAwardLetterConfigurationsAsync().Result.ToList();
                }
            }

            public FinancialAidReferenceDataRepository actualRepository;
            public List<AwardLetterConfiguration> actualConfigurations
            {
                get
                {
                    var configurations = actualRepository.GetAwardLetterConfigurationsAsync().Result.ToList();
                    return configurations;
                }
            }

            [TestInitialize]
            public void Initalize(){
                MockInitialize();
                expectedRepository = new TestFinancialAidReferenceDataRepository();
                actualRepository = BuildFinancialAidReferenceDataRepository();
                comparer = new FunctionEqualityComparer<AwardLetterConfiguration>((c1, c2) => (c1.Id == c2.Id &&
                    c1.AwardTableTitle == c2.AwardTableTitle && c1.AwardTotalTitle == c2.AwardTotalTitle &&
                    c1.IsContactBlockActive == c2.IsContactBlockActive && c1.IsHousingBlockActive == c2.IsHousingBlockActive &&
                    c1.IsNeedBlockActive == c2.IsNeedBlockActive && c1.ParagraphSpacing == c2.ParagraphSpacing), (c) => c.Id.GetHashCode());
            }

            [TestCleanup]
            public void Cleanup()
            {
                expectedRepository = null;
                actualRepository = null;
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                Assert.AreEqual(expectedConfigurations.Count, actualConfigurations.Count);
                CollectionAssert.AreEqual(expectedConfigurations, actualConfigurations, comparer);
            }

            [TestMethod]
            public void ConfigurationRecordsListEmpty_ReturnsEmptyListTest()
            {
                expectedRepository.awardLetterParameterData = new List<TestFinancialAidReferenceDataRepository.awardLetterParamRecord>();
                BuildFinancialAidReferenceDataRepository();
                Assert.IsTrue(!actualConfigurations.Any());
            }

            [TestMethod]
            public void NullConfigurations_ReturnsEmptyListTest()
            {
                expectedRepository.awardLetterParameterData = null;
                BuildFinancialAidReferenceDataRepository();
                Assert.IsTrue(!actualConfigurations.Any());
            }

            [TestMethod]
            public void DataReaderResultIsNull_ReturnsEmptyListLogsMessageTest()
            {
                expectedRepository.awardLetterParameterData = null;
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<AltrParameters>("", false))
                   .ReturnsAsync(null);
                Assert.IsTrue(!actualConfigurations.Any());
                loggerMock.Verify(l => l.Info("Null AltrParameters returned from database"));
            }

            [TestMethod]
            public void EachAwardLetterConfigurationDto_ContainsCorrectNumberOfGroupsTest()
            {
                var expectedAwardCategoryGroupsCount = 3;
                var expectedAwardPeriodColumnGroupsCount = 6;
                foreach (var configurationDto in actualConfigurations)
                {
                    Assert.AreEqual(expectedAwardCategoryGroupsCount, configurationDto.AwardCategoriesGroups.Count);
                    Assert.AreEqual(expectedAwardPeriodColumnGroupsCount, configurationDto.AwardPeriodsGroups.Count);
                }
            }

            private FinancialAidReferenceDataRepository BuildFinancialAidReferenceDataRepository()
            {
                dataReaderMock.Setup<Task<Collection<AltrParameters>>>(dr => dr.BulkReadRecordAsync<AltrParameters>("", false))
                    .Returns<string, bool>((x, y) =>
                    {
                        return Task.FromResult(new Collection<AltrParameters>
                            (expectedRepository.GetAwardLetterConfigurationsAsync().Result
                                .Select(record => new AltrParameters()
                                {
                                    Recordkey = record.Id,
                                    AltrNeedBlock = record.IsNeedBlockActive ? "Y" : "N",
                                    AltrHousingCode = record.IsHousingBlockActive ? "Y" : "N",
                                    AltrOfficeBlock = record.IsContactBlockActive ? "Y" : "N",
                                    AltrParaSpacing = record.ParagraphSpacing,
                                    AltrTitleAwdName = record.AwardTableTitle,
                                    AltrTitleAwdTotal = record.AwardTotalTitle,
                                    AltrTitleColumn1 = record.AwardPeriodsGroups[0].GroupName,
                                    AltrTitleColumn2 = record.AwardPeriodsGroups[1].GroupName,
                                    AltrTitleColumn3 = record.AwardPeriodsGroups[2].GroupName,
                                    AltrTitleColumn4 = record.AwardPeriodsGroups[3].GroupName,
                                    AltrTitleColumn5 = record.AwardPeriodsGroups[4].GroupName,
                                    AltrTitleColumn6 = record.AwardPeriodsGroups[5].GroupName,
                                    AltrTitleGroup1 = record.AwardCategoriesGroups[0].GroupName,
                                    AltrTitleGroup2 = record.AwardCategoriesGroups[1].GroupName,
                                    AltrTitleGroup3 = record.AwardCategoriesGroups[2].GroupName

                                }).ToList()));
                    });
                
                loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);
                return new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }            
        }
        #endregion

        [TestClass]
        public class FinancialAidAwardPeriods
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<FinancialAidAwardPeriod> allFinancialAidAwardPeriods;
            string valcodeName;
            ApiSettings apiSettings;

            FinancialAidReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allFinancialAidAwardPeriods = new TestFinancialAidReferenceDataRepository().GetFinancialAidAwardPeriodsAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllFinancialAidAwardPeriods");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allFinancialAidAwardPeriods = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidAwardPeriodsAsync_False()
            {
                var results = await referenceDataRepo.GetFinancialAidAwardPeriodsAsync(false);
                Assert.AreEqual(allFinancialAidAwardPeriods.Count(), results.Count());

                foreach (var financialAidAwardPeriod in allFinancialAidAwardPeriods)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidAwardPeriod.Guid);

                    Assert.AreEqual(financialAidAwardPeriod.Code, result.Code);
                    Assert.AreEqual(financialAidAwardPeriod.Description, result.Description);
                    Assert.AreEqual(financialAidAwardPeriod.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidAwardPeriodsAsync_True()
            {
                var results = await referenceDataRepo.GetFinancialAidAwardPeriodsAsync(true);
                Assert.AreEqual(allFinancialAidAwardPeriods.Count(), results.Count());

                foreach (var financialAidAwardPeriod in allFinancialAidAwardPeriods)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidAwardPeriod.Guid);

                    Assert.AreEqual(financialAidAwardPeriod.Code, result.Code);
                    Assert.AreEqual(financialAidAwardPeriod.Description, result.Description);
                    Assert.AreEqual(financialAidAwardPeriod.Guid, result.Guid);
                }

            }

            private FinancialAidReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.AwardPeriods>();
                foreach (var item in allFinancialAidAwardPeriods)
                {
                    DataContracts.AwardPeriods record = new DataContracts.AwardPeriods();
                    record.RecordGuid = item.Guid;
                    record.AwdpDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.AwardPeriods>("AWARD.PERIODS", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allFinancialAidAwardPeriods.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "AWARD.PERIODS", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class FinancialAidFundCategories
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<FinancialAidFundCategory> allFinancialAidFundCategories;
            string valcodeName;
            ApiSettings apiSettings;

            FinancialAidReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allFinancialAidFundCategories = new TestFinancialAidReferenceDataRepository().GetFinancialAidFundCategoriesAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllFinancialAidFundCategories");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allFinancialAidFundCategories = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidFundCategoriesAsync_False()
            {
                var results = await referenceDataRepo.GetFinancialAidFundCategoriesAsync(false);
                Assert.AreEqual(allFinancialAidFundCategories.Count(), results.Count());

                foreach (var financialAidFundCategory in allFinancialAidFundCategories)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidFundCategory.Guid);

                    Assert.AreEqual(financialAidFundCategory.Code, result.Code);
                    Assert.AreEqual(financialAidFundCategory.Description, result.Description);
                    Assert.AreEqual(financialAidFundCategory.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidFundCategoriesAsync_True()
            {
                var results = await referenceDataRepo.GetFinancialAidFundCategoriesAsync(true);
                Assert.AreEqual(allFinancialAidFundCategories.Count(), results.Count());

                foreach (var financialAidFundCategory in allFinancialAidFundCategories)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidFundCategory.Guid);

                    Assert.AreEqual(financialAidFundCategory.Code, result.Code);
                    Assert.AreEqual(financialAidFundCategory.Description, result.Description);
                    Assert.AreEqual(financialAidFundCategory.Guid, result.Guid);
                }

            }

            private FinancialAidReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.AwardCategories>();
                foreach (var item in allFinancialAidFundCategories)
                {
                    DataContracts.AwardCategories record = new DataContracts.AwardCategories();
                    record.RecordGuid = item.Guid;
                    record.AcDescription = item.Description;
                    record.Recordkey = item.Code;
                    record.AcIntgRestricted = item.restrictedFlag ? "Y" : "N";
                    record.AcGrantFlag =  item.AwardCategoryType == AwardCategoryType.Grant ? "Y" : "N"; 
                    record.AcLoanFlag = item.AwardCategoryType == AwardCategoryType.Loan ? "Y" : "N";
                    record.AcScholarshipFlag = item.AwardCategoryType == AwardCategoryType.Scholarship ? "Y" : "N";
                    record.AcWorkFlag = item.AwardCategoryType == AwardCategoryType.Work ? "Y" : "N";
                    record.AcIntgName = ConvertCategoryName(item.AwardCategoryName);
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.AwardCategories>("AWARD.CATEGORIES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allFinancialAidFundCategories.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "AWARD.CATEGORIES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a FinancialAidFundsSource domain enumeration value to its corresponding FinancialAidFundsSource DTO enumeration value
            /// </summary>
            /// <param name="source">FinancialAidFundsSource domain enumeration value</param>
            /// <returns>FinancialAidFundsSource DTO enumeration value</returns>
            private string ConvertCategoryName(Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType? source)
            {
                switch (source)
                {

                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.academicCompetitivenessGrant:
                        return "1";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.bureauOfIndianAffairsFederalGrant:
                        return "2";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.federalPerkinsLoan:
                        return "3";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.federalSubsidizedLoan:
                        return "4";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.federalSupplementaryEducationalOpportunityGrant:
                        return "5";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.federalSupplementaryLoanForParent:
                        return "6";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.federalUnsubsidizedLoan:
                        return "7";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.federalWorkStudyProgram:
                        return "8";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.generalTitleIVloan:
                        return "9";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.graduatePlusLoan:
                        return "10";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.graduateTeachingGrant:
                        return "11";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.healthEducationAssistanceLoan:
                        return "12";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.healthProfessionalStudentLoan:
                        return "13";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.incomeContingentLoan:
                        return "14";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.iraqAfghanistanServiceGrant:
                        return "15";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.leveragingEducationalAssistancePartnership:
                        return "16";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.loanForDisadvantagesStudent:
                        return "17";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.nationalHealthServicesCorpsScholarship:
                        return "18";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.nationalSmartGrant:
                        return "19";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.NotSet:
                        return "20";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.nursingStudentLoan:
                        return "21";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.parentPlusLoan:
                        return "22";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.paulDouglasTeacherScholarship:
                        return "23";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.pellGrant:
                        return "24";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.primaryCareLoan:
                        return "25";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.robertCByrdScholarshipProgram:
                        return "26";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.rotcScholarship:
                        return "27";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.stateStudentIncentiveGrant:
                        return "28";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.stayInSchoolProgram:
                        return "29";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.undergraduateTeachingGrant:
                        return "30";
                    case Domain.FinancialAid.Entities.FinancialAidFundAidCategoryType.vaHealthProfessionsScholarship:
                        return "31";

                    default:
                        return "32";
                }
            }
        }

        [TestClass]
        public class FinancialAidFundClassifications
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<FinancialAidFundClassification> allFinancialAidFundClassifications;
            string valcodeName;
            ApiSettings apiSettings;

            FinancialAidReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allFinancialAidFundClassifications = new TestFinancialAidReferenceDataRepository().GetFinancialAidFundClassificationsAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllFinancialAidFundClassifications");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allFinancialAidFundClassifications = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidFundClassificationsAsync_False()
            {
                var results = await referenceDataRepo.GetFinancialAidFundClassificationsAsync(false);
                Assert.AreEqual(allFinancialAidFundClassifications.Count(), results.Count());

                foreach (var financialAidFundClassification in allFinancialAidFundClassifications)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidFundClassification.Guid);

                    Assert.AreEqual(financialAidFundClassification.Code, result.Code);
                    //Assert.AreEqual(financialAidFundClassification.Description, result.Description);
                    Assert.AreEqual(financialAidFundClassification.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidFundClassificationsAsync_True()
            {
                var results = await referenceDataRepo.GetFinancialAidFundClassificationsAsync(true);
                Assert.AreEqual(allFinancialAidFundClassifications.Count(), results.Count());

                foreach (var financialAidFundClassification in allFinancialAidFundClassifications)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidFundClassification.Guid);

                    Assert.AreEqual(financialAidFundClassification.Code, result.Code);
                    //Assert.AreEqual(financialAidFundClassification.Description, result.Description);
                    Assert.AreEqual(financialAidFundClassification.Guid, result.Guid);
                }

            }

            private FinancialAidReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.ReportFundTypes>();
                foreach (var item in allFinancialAidFundClassifications)
                {
                    DataContracts.ReportFundTypes record = new DataContracts.ReportFundTypes();
                    record.RecordGuid = item.Guid;
                    record.RftDesc = item.Description;
                    record.RftFundTypeCode = item.Code;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.ReportFundTypes>("REPORT.FUND.TYPES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allFinancialAidFundClassifications.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "REPORT.FUND.TYPES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class FinancialAidYears
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<FinancialAidYear> allFinancialAidYears;
            string valcodeName;
            ApiSettings apiSettings;

            FinancialAidReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allFinancialAidYears = new TestFinancialAidReferenceDataRepository().GetFinancialAidYearsAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllFinancialAidYears");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allFinancialAidYears = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidYearsAsync_False()
            {
                var results = await referenceDataRepo.GetFinancialAidYearsAsync(false);
                Assert.AreEqual(allFinancialAidYears.Count(), results.Count());

                foreach (var financialAidYear in allFinancialAidYears)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidYear.Guid);

                    Assert.AreEqual(financialAidYear.Code, result.Code);
                    Assert.AreEqual(financialAidYear.Code, result.Description);
                    Assert.AreEqual(financialAidYear.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidYearsAsync_True()
            {
                var results = await referenceDataRepo.GetFinancialAidYearsAsync(true);
                Assert.AreEqual(allFinancialAidYears.Count(), results.Count());

                foreach (var financialAidYear in allFinancialAidYears)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidYear.Guid);

                    Assert.AreEqual(financialAidYear.Code, result.Code);
                    Assert.AreEqual(financialAidYear.Code, result.Description);
                    Assert.AreEqual(financialAidYear.Guid, result.Guid);
                }

            }

            private FinancialAidReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.FaSuites>();
                foreach (var item in allFinancialAidYears)
                {
                    DataContracts.FaSuites record = new DataContracts.FaSuites();
                    record.RecordGuid = item.Guid;
                    //record = item.Description;
                    record.Recordkey = item.Code;
                    record.FaSuitesStatus = item.status;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.FaSuites>("FA.SUITES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allFinancialAidYears.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "FA.SUITES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }        

        [TestClass]
        public class GetFinancialAidExplanationTests : BaseRepositorySetup
        {
            private List<FinancialAidExplanation> expectedExplanations;
            private List<FinancialAidExplanation> actualExplanations;

            private TestFinancialAidReferenceDataRepository expectedRepository;
            private FinancialAidReferenceDataRepository actualRepository;

            private FaExplanations faExplanationsResponseData;

            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestFinancialAidReferenceDataRepository();
                expectedExplanations = (await expectedRepository.GetFinancialAidExplanationsAsync()).ToList();
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<FaExplanations>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns<string, string, bool>(
                    (f, rk, b) =>
                    {
                        var explanationRecord = expectedRepository.faExplanations;
                        return Task.FromResult(new FaExplanations() { Recordkey = explanationRecord.recordKey, FePellLeuExpl = explanationRecord.pellLeuExpl });
                    });

                BuildRepository();
                actualExplanations = (await actualRepository.GetFinancialAidExplanationsAsync()).ToList();
            }

            private void BuildRepository()
            {                
                actualRepository = new FinancialAidReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public void GetFinancialAidExplanationsAsync_ReturnsNonEmptyListTest()
            {               
                Assert.IsNotNull(actualExplanations);
                Assert.IsTrue(actualExplanations.Any());
            }

            [TestMethod]
            public void GetFinancialAidExplanationsAsync_ReturnsExpectedResultTest()
            {
                for(var i = 0; i < actualExplanations.Count; i++)
                {
                    Assert.AreEqual(expectedExplanations[i].ExplanationText, actualExplanations[i].ExplanationText);
                    Assert.AreEqual(expectedExplanations[i].ExplanationType, actualExplanations[i].ExplanationType);
                }
            }

            [TestMethod]
            public async Task GetFinancialAidExplanationsAsync_ReturnsEmptyTest()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<FaExplanations>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(null);
                BuildRepository();
                Assert.IsFalse((await actualRepository.GetFinancialAidExplanationsAsync()).Any());
            }
        }

    }
}