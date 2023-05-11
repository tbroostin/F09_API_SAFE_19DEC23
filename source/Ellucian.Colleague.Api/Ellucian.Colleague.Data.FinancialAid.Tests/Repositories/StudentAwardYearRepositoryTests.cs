//Copyright 2014-2017 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class StudentAwardYearRepositoryTests : BaseRepositorySetup
    {

        public string studentId;

        public UpdateCorrOptionFlagRequest actualUpdateRequest;

        public TestStudentAwardYearRepository expectedRepository;
        public StudentAwardYearRepository actualRepository;

        public TestLoanRequestRepository loanRequestRepository;
        public TestFinancialAidOfficeRepository officeRepository;

        public CurrentOfficeService currentOfficeService;

        public StudentAwardYearRepository BuildStudentAwardYearsRepository()
        {

            dataReaderMock.Setup(d => d.ReadRecord<FinAid>(It.IsAny<string>(), true))
                .Returns<string, bool>((id, b) => expectedRepository.FaStudentData == null ? null :
                    new FinAid()
                    {
                        Recordkey = expectedRepository.FaStudentData.StudentId,
                        FaCsYears = expectedRepository.FaStudentData.FaCsYears,
                        FaSaYears = expectedRepository.FaStudentData.FaSaYears,
                        FaYsYears = expectedRepository.FaStudentData.FaYsYears,
                        FaPaperCopyOptInFlag = expectedRepository.FaStudentData.FaPaperCopyOptInFlag,
                        FaPendingLoanRequestIds = expectedRepository.FaStudentData.PendingLoanRequestIds,
                    });

            transManagerMock.Setup(t =>
                t.Execute<UpdateCorrOptionFlagRequest, UpdateCorrOptionFlagResponse>(It.IsAny<UpdateCorrOptionFlagRequest>())) 
                .Callback<UpdateCorrOptionFlagRequest>((req) => actualUpdateRequest = req)
                .Returns<UpdateCorrOptionFlagRequest>((req) => expectedRepository.transactionResponseData == null ? null :
                    new UpdateCorrOptionFlagResponse()
                    {
                        ErrorMessage = expectedRepository.transactionResponseData.ErrorMessage
                    })
                .Verifiable();

            dataReaderMock.Setup(d => d.BulkReadRecord<NewLoanRequest>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((loanRequestIds, b) => loanRequestRepository.NewLoanRequestList == null ? null :
                    new Collection<NewLoanRequest>(loanRequestRepository.NewLoanRequestList
                        .Where(record => loanRequestIds.Contains(record.id))
                        .Select(record =>
                            new NewLoanRequest()
                            {
                                Recordkey = record.id,
                                NewLoanRequestAdddate = record.requestDate,
                                NlrAssignedToId = record.assignedToId,
                                NlrAwardYear = record.awardYear,
                                NlrCurrentStatus = record.statusCode,
                                NlrCurrentStatusDate = record.statusDate,
                                NlrModifierComments = record.modifierComments,
                                NlrModifierId = record.modifierId,
                                NlrStudentComments = record.studentComments,
                                NlrStudentId = studentId,
                                NlrTotalRequestAmount = record.totalRequestAmount
                            }).ToList()));

            dataReaderMock.Setup<Task<Collection<AwardLetterHistory>>>(d => d.BulkReadRecordAsync<AwardLetterHistory>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((criteria, b) => 
                {
                    return Task.FromResult(
                        new Collection<AwardLetterHistory> (expectedRepository.AwardLetterHistoryRecordsData == null ? null :
                        expectedRepository.AwardLetterHistoryRecordsData
                        .Select(record =>
                        new AwardLetterHistory()
                        {
                            Recordkey = record.recordKey,
                            AlhAwardLetterDate = record.createdDate,
                            AlhAwardYear = record.awardYearCode,
                            AwardLetterHistoryAddtime = record.addTime
                        }
                        ).ToList()));
                });

            dataReaderMock.Setup(d => d.ReadRecord<CsAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((acyrFile, id, b) => expectedRepository.CsStudentData == null ? null :
                    expectedRepository.CsStudentData
                    .Where(record => record.AwardYear == acyrFile.Split('.')[1])
                    .Select(record =>
                        new CsAcyr()
                        {
                            Recordkey = id,
                            CsLocation = record.LocationId,
                            CsBudgetAdj = record.ExpensesAdjustment,
                            CsStdTotalExpenses = record.TotalEstimatedExpenses,
                            CsFedIsirId = record.FedIsirId
                        }).FirstOrDefault());

            dataReaderMock.Setup(d => d.ReadRecord<SaAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((acyrFile, id, b) => expectedRepository.SaStudentData == null ? null :
                    expectedRepository.SaStudentData
                    .Where(record => record.AwardYear == acyrFile.Split('.')[1])
                    .Select(record =>
                        new SaAcyr()
                        {
                            Recordkey = id,
                            SaAwarded = record.TotalAwardedAmount,
                        }).FirstOrDefault());


            dataReaderMock.Setup(d => d.ReadRecord<YsAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((acyrFile, id, b) => expectedRepository.YsStudentData == null ? null :
                    expectedRepository.YsStudentData
                    .Where(record => record.awardYear == acyrFile.Split('.')[1])
                    .Select(record =>
                        new YsAcyr()
                        {
                            Recordkey = id,
                            YsApplCompleteDate = record.ApplicationReviewedDate
                        }).FirstOrDefault());

            //loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);

            return new StudentAwardYearRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        public StudentAwardYearRepository BuildStudentAwardYearsRepositoryAsync()
        {

            dataReaderMock.Setup(d => d.ReadRecordAsync<FinAid>(It.IsAny<string>(), true))
                .Returns<string, bool>((id, b) => {
                    return Task.FromResult(expectedRepository.FaStudentData == null ? null :
                    new FinAid()
                    {
                        Recordkey = expectedRepository.FaStudentData.StudentId,
                        FaCsYears = expectedRepository.FaStudentData.FaCsYears,
                        FaSaYears = expectedRepository.FaStudentData.FaSaYears,
                        FaYsYears = expectedRepository.FaStudentData.FaYsYears,
                        FaPaperCopyOptInFlag = expectedRepository.FaStudentData.FaPaperCopyOptInFlag,
                        FaPendingLoanRequestIds = expectedRepository.FaStudentData.PendingLoanRequestIds,
                    });
                });

            transManagerMock.Setup(t =>
                t.ExecuteAsync<UpdateCorrOptionFlagRequest, UpdateCorrOptionFlagResponse>(It.IsAny<UpdateCorrOptionFlagRequest>()))
                .Callback<UpdateCorrOptionFlagRequest>((req) => actualUpdateRequest = req)
                .Returns<UpdateCorrOptionFlagRequest>((req) => {
                    return Task.FromResult(expectedRepository.transactionResponseData == null ? null :
                    new UpdateCorrOptionFlagResponse()
                    {
                        ErrorMessage = expectedRepository.transactionResponseData.ErrorMessage
                    });
                })
                .Verifiable();

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<NewLoanRequest>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((loanRequestIds, b) =>
                {
                    return Task.FromResult(loanRequestRepository.NewLoanRequestList == null ? null :
                    new Collection<NewLoanRequest>(loanRequestRepository.NewLoanRequestList
                        .Where(record => loanRequestIds.Contains(record.id))
                        .Select(record =>
                            new NewLoanRequest()
                            {
                                Recordkey = record.id,
                                NewLoanRequestAdddate = record.requestDate,
                                NlrAssignedToId = record.assignedToId,
                                NlrAwardYear = record.awardYear,
                                NlrCurrentStatus = record.statusCode,
                                NlrCurrentStatusDate = record.statusDate,
                                NlrModifierComments = record.modifierComments,
                                NlrModifierId = record.modifierId,
                                NlrStudentComments = record.studentComments,
                                NlrStudentId = studentId,
                                NlrTotalRequestAmount = record.totalRequestAmount
                            }).ToList()));
                });

            dataReaderMock.Setup<Task<Collection<AwardLetterHistory>>>(d => d.BulkReadRecordAsync<AwardLetterHistory>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((criteria, b) =>
                {
                    return Task.FromResult(
                        new Collection<AwardLetterHistory>(expectedRepository.AwardLetterHistoryRecordsData == null ? null :
                        expectedRepository.AwardLetterHistoryRecordsData
                        .Select(record =>
                        new AwardLetterHistory()
                        {
                            Recordkey = record.recordKey,
                            AlhAwardLetterDate = record.createdDate,
                            AlhAwardYear = record.awardYearCode,
                            AwardLetterHistoryAddtime = record.addTime
                        }
                        ).ToList()));
                });

            dataReaderMock.Setup(d => d.ReadRecordAsync<CsAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((acyrFile, id, b) =>
                {
                    return Task.FromResult(expectedRepository.CsStudentData == null ? null :
                    expectedRepository.CsStudentData
                    .Where(record => record.AwardYear == acyrFile.Split('.')[1])
                    .Select(record =>
                        new CsAcyr()
                        {
                            Recordkey = id,
                            CsLocation = record.LocationId,
                            CsBudgetAdj = record.ExpensesAdjustment,
                            CsStdTotalExpenses = record.TotalEstimatedExpenses,
                            CsFedIsirId = record.FedIsirId
                        }).FirstOrDefault());
                });

            dataReaderMock.Setup(d => d.ReadRecordAsync<SaAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((acyrFile, id, b) =>
                {
                    return Task.FromResult(expectedRepository.SaStudentData == null ? null :
                    expectedRepository.SaStudentData
                    .Where(record => record.AwardYear == acyrFile.Split('.')[1])
                    .Select(record =>
                        new SaAcyr()
                        {
                            Recordkey = id,
                            SaAwarded = record.TotalAwardedAmount,
                        }).FirstOrDefault());
                });


            dataReaderMock.Setup(d => d.ReadRecordAsync<YsAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((acyrFile, id, b) =>
                {
                    return Task.FromResult(expectedRepository.YsStudentData == null ? null :
                    expectedRepository.YsStudentData
                    .Where(record => record.awardYear == acyrFile.Split('.')[1])
                    .Select(record =>
                        new YsAcyr()
                        {
                            Recordkey = id,
                            YsApplCompleteDate = record.ApplicationReviewedDate
                        }).FirstOrDefault());
                });

            return new StudentAwardYearRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        public void StudentAwardYearRepositoryTestsInitialize()
        {
            MockInitialize();
            studentId = "0003914";
            expectedRepository = new TestStudentAwardYearRepository();
            officeRepository = new TestFinancialAidOfficeRepository();
            loanRequestRepository = new TestLoanRequestRepository();

            currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

            actualRepository = BuildStudentAwardYearsRepository();
        }

        public void StudentAwardYearRepositoryTestsInitializeAsync()
        {
            MockInitialize();
            studentId = "0003914";
            expectedRepository = new TestStudentAwardYearRepository();
            officeRepository = new TestFinancialAidOfficeRepository();
            loanRequestRepository = new TestLoanRequestRepository();

            currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

            actualRepository = BuildStudentAwardYearsRepositoryAsync();
        }


        [TestClass]
        public class GetStudentAwardYearsAsyncTests : StudentAwardYearRepositoryTests
        {
            public List<StudentAwardYear> expectedStudentAwardYears
            { get { return expectedRepository.GetStudentAwardYears(studentId, currentOfficeService).ToList(); } }

            public List<StudentAwardYear> actualStudentAwardYears;             

            [TestInitialize]
            public async void Initialize()
            {
                StudentAwardYearRepositoryTestsInitializeAsync();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
            }

            [TestMethod]
            public void NumberOfStudentAwardYearsAreEqual()
            {
                Assert.IsTrue(actualStudentAwardYears.Count() > 0);
                Assert.AreEqual(expectedStudentAwardYears.Count(), actualStudentAwardYears.Count());
            }

            [TestMethod]
            public void StudentAwardYearsListsAreEqual()
            {
                CollectionAssert.AreEqual(expectedStudentAwardYears, actualStudentAwardYears);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentId_RequiredTest()
            {
                await actualRepository.GetStudentAwardYearsAsync("", currentOfficeService);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CurrentOfficeService_RequiredTest()
            {
                await actualRepository.GetStudentAwardYearsAsync(studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullFinAidRecord_ThrowsExceptionTest()
            {
                expectedRepository.FaStudentData = null;
                await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService);
            }

            [TestMethod]
            public async Task NullFinAidYears_ReturnEmptyList()
            {
                expectedRepository.FaStudentData.FaCsYears = null;
                expectedRepository.FaStudentData.FaSaYears = null;
                expectedRepository.FaStudentData.FaYsYears = null;
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.Count() == 0);
            }

            [TestMethod]
            public async Task NullFaCsYears_ReturnNonNullValue()
            {
                expectedRepository.FaStudentData.FaCsYears = null;

                var yearsCount = expectedRepository.FaStudentData.FaSaYears.Concat(expectedRepository.FaStudentData.FaYsYears)
                    .Where(year => !string.IsNullOrEmpty(year))
                    .Distinct()
                    .Count();

                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();

                Assert.IsNotNull(actualStudentAwardYears);

                Assert.AreEqual(yearsCount, actualStudentAwardYears.Count());
            }

            [TestMethod]
            public async Task NullFaSaYears_ReturnNonNullValue()
            {
                expectedRepository.FaStudentData.FaSaYears = null;

                var yearsCount = expectedRepository.FaStudentData.FaCsYears.Concat(expectedRepository.FaStudentData.FaYsYears)
                    .Where(year => !string.IsNullOrEmpty(year))
                    .Distinct()
                    .Count();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsNotNull(actualStudentAwardYears);
                Assert.AreEqual(yearsCount, actualStudentAwardYears.Count());
            }

            [TestMethod]
            public async Task NullFaYsYears_ReturnNonNullValue()
            {
                expectedRepository.FaStudentData.FaYsYears = null;

                var yearsCount = expectedRepository.FaStudentData.FaCsYears.Concat(expectedRepository.FaStudentData.FaSaYears)
                    .Where(year => !string.IsNullOrEmpty(year))
                    .Distinct()
                    .Count();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsNotNull(actualStudentAwardYears);
                Assert.AreEqual(yearsCount, actualStudentAwardYears.Count());
            }

            [TestMethod]
            public async Task NullAndEmptyYearFromRecordIsRemovedTest()
            {
                expectedRepository.FaStudentData.FaYsYears.Add("");
                expectedRepository.FaStudentData.FaCsYears.Insert(0, null);
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                CollectionAssert.AreEqual(expectedStudentAwardYears, actualStudentAwardYears);
            }

            [TestMethod]
            public async Task YearsReturnedInOrderedListTest()
            {
                var orderedList = expectedStudentAwardYears.OrderBy(y => y.Code).ToList();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                CollectionAssert.AreEqual(orderedList, actualStudentAwardYears);
            }

            [TestMethod]
            public async Task NullPendingLoanRequestIds_NoStudentAwardYearsWithPendingLoanRequestIdTest()
            {
                expectedRepository.FaStudentData.PendingLoanRequestIds = null;
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => string.IsNullOrEmpty(y.PendingLoanRequestId)));
            }

            [TestMethod]
            public async Task EmptyPendingLoanRequestIds_NoStudentAwardYearsWithPendingLoanRequestIdTest()
            {
                expectedRepository.FaStudentData.PendingLoanRequestIds = new List<string>();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => string.IsNullOrEmpty(y.PendingLoanRequestId)));
            }

            [TestMethod]
            public async Task NullNewLoanRequests_NoStudentAwardYearsWithPendingLoanRequestIdTest()
            {
                loanRequestRepository.NewLoanRequestList = null;
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => string.IsNullOrEmpty(y.PendingLoanRequestId)));
            }

            [TestMethod]
            public async Task EmptyNewLoanRequests_NoStudentAwardYearsWithPendingLoanRequestIdTest()
            {
                loanRequestRepository.NewLoanRequestList = new List<TestLoanRequestRepository.NewLoanRequestData>();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => string.IsNullOrEmpty(y.PendingLoanRequestId)));
            }

            [TestMethod]
            public async Task NoAwardLetterHistoryRecords_NoStudentAwardYearWithAwardLetterHistoryItemsTest()
            {
                expectedRepository.AwardLetterHistoryRecordsData = new List<TestStudentAwardYearRepository.AwardLetterHistoryRecord>();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => !y.AwardLetterHistoryItemsForYear.Any()));
                loggerMock.Verify(l => l.Debug(string.Format("Student Id {0} has no award letter history records", studentId)));
            }

            [TestMethod]
            public void AwardLetterHistoryRecordsForYearCount_EqualsExpectedTest()
            {
                foreach (var year in actualStudentAwardYears)
                {
                    var expectedCount = expectedRepository.AwardLetterHistoryRecordsData.Where(r => r.awardYearCode == year.Code).Count();
                    Assert.AreEqual(expectedCount, year.AwardLetterHistoryItemsForYear.Count);
                }
            }

            [TestMethod]
            public void AwardLetterHistoryItemsAreSortedByDateTimeInDescendingOrderTest()
            {
                foreach (var year in actualStudentAwardYears)
                {
                    var expectedAwardLetterHistoryItems = new List<AwardLetterHistoryItem>();
                    var recordsForTheYear = expectedRepository.AwardLetterHistoryRecordsData.Where(l => l.awardYearCode == year.Code)
                        .OrderByDescending(r => r.createdDate).ThenByDescending(r => r.addTime).ToList();
                    foreach (var record in recordsForTheYear)
                    {
                        expectedAwardLetterHistoryItems.Add(new AwardLetterHistoryItem(record.recordKey, record.createdDate));
                    }

                    for (int i = 0; i < expectedAwardLetterHistoryItems.Count; i++)
                    {
                        Assert.AreEqual(expectedAwardLetterHistoryItems[i], year.AwardLetterHistoryItemsForYear[i]);
                    }
                }

            }

            [TestMethod]
            public async Task CsYearSetsTotalEstimatedExpensesTest()
            {
                var fooYear = "FOO";
                var expectedExpenses = 55555;
                expectedRepository.FaStudentData.FaCsYears.Add(fooYear);
                expectedRepository.CsStudentData.Add(new TestStudentAwardYearRepository.CsStudent() { AwardYear = fooYear, TotalEstimatedExpenses = expectedExpenses });
                officeRepository.officeParameterRecordData.Add(new TestFinancialAidOfficeRepository.OfficeParameterRecord() { AwardYear = "FOO", IsSelfServiceActive = "Y", OfficeCode = "MAIN" });
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                var actualStudentAwardYear = actualStudentAwardYears.First(y => y.Code == fooYear);
                Assert.AreEqual(expectedExpenses, actualStudentAwardYear.TotalEstimatedExpenses);
            }

            [TestMethod]
            public async Task CsYearSetsEstimatedExpensesAdjustmentTest()
            {
                var fooYear = "FOO";
                var expectedAdjustment = 1234;
                expectedRepository.FaStudentData.FaCsYears.Add(fooYear);
                expectedRepository.CsStudentData.Add(new TestStudentAwardYearRepository.CsStudent() { AwardYear = fooYear, ExpensesAdjustment = expectedAdjustment });
                officeRepository.officeParameterRecordData.Add(new TestFinancialAidOfficeRepository.OfficeParameterRecord() { AwardYear = "FOO", IsSelfServiceActive = "Y", OfficeCode = "MAIN" });
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                var actualStudentAwardYear = actualStudentAwardYears.First(y => y.Code == fooYear);
                Assert.AreEqual(expectedAdjustment, actualStudentAwardYear.EstimatedExpensesAdjustment);
            }

            [TestMethod]
            public async Task NoCsRecordTests()
            {
                var fooYear = "FOO";
                expectedRepository.FaStudentData.FaCsYears.Add(fooYear);
                officeRepository.officeParameterRecordData.Add(new TestFinancialAidOfficeRepository.OfficeParameterRecord() { AwardYear = "FOO", IsSelfServiceActive = "Y", OfficeCode = "MAIN" });
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                var nonCsYearStudentAwardYear = actualStudentAwardYears.First(y => y.Code == fooYear);

                var defaultOffice = currentOfficeService.GetDefaultOffice();
                Assert.AreEqual(defaultOffice, nonCsYearStudentAwardYear.CurrentOffice);
                Assert.IsNull(nonCsYearStudentAwardYear.TotalEstimatedExpenses);
                Assert.IsNull(nonCsYearStudentAwardYear.EstimatedExpensesAdjustment);
            }

            [TestMethod]
            public async Task PendingLoanRequestIdsTest()
            {
                var expectedId = "BAR";
                var fooYear = "FOO";
                expectedRepository.FaStudentData.PendingLoanRequestIds.Add(expectedId);
                expectedRepository.FaStudentData.FaYsYears.Add(fooYear);
                loanRequestRepository.NewLoanRequestList = new List<TestLoanRequestRepository.NewLoanRequestData>()
                    {
                        new TestLoanRequestRepository.NewLoanRequestData()
                        {
                            id = expectedId,
                            awardYear = fooYear
                        }
                    };
                officeRepository.officeParameterRecordData.Add(new TestFinancialAidOfficeRepository.OfficeParameterRecord() { AwardYear = "FOO", IsSelfServiceActive = "Y", OfficeCode = "MAIN" });
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                var actualYear = actualStudentAwardYears.First(y => y.Code == fooYear);
                Assert.AreEqual(expectedId, actualYear.PendingLoanRequestId);
            }

            [TestMethod]
            public async Task NoPendingLoanRequestForAwardYear_EmptyPendingLoanRequestIdTest()
            {
                var expectedId = "BAR";
                var fooYear = "FOO";
                expectedRepository.FaStudentData.PendingLoanRequestIds.Add(expectedId);
                expectedRepository.FaStudentData.FaYsYears.Add(fooYear);
                loanRequestRepository.NewLoanRequestList = new List<TestLoanRequestRepository.NewLoanRequestData>()
                    {
                        new TestLoanRequestRepository.NewLoanRequestData()
                        {
                            id = expectedId,
                            awardYear = fooYear
                        }
                    };
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                var noLoanRequestYears = actualStudentAwardYears.Where(y => y.Code != fooYear);
                Assert.IsTrue(noLoanRequestYears.All(y => string.IsNullOrEmpty(y.PendingLoanRequestId)));
            }

            [TestMethod]
            public async Task NoSaYearSetsTotalAwardedAmountToZero()
            {
                expectedRepository.FaStudentData.FaSaYears = new List<string>();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => y.TotalAwardedAmount == 0));
            }

            [TestMethod]
            public async Task NullSaAcyrRecordSetsTotalAwardedAmountToZero()
            {
                expectedRepository.SaStudentData = new List<TestStudentAwardYearRepository.SaStudent>();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => y.TotalAwardedAmount == 0));
            }

            [TestMethod]
            public async Task TotalAwardedAmountEqualsSaAwardedTest()
            {
                var expectedAmount = 5555.55m;
                expectedRepository.SaStudentData.ForEach(sa => sa.TotalAwardedAmount = expectedAmount);
                expectedRepository.FaStudentData.FaSaYears.AddRange(expectedRepository.SaStudentData.Select(sa => sa.AwardYear));
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();

                Assert.IsTrue(actualStudentAwardYears.All(y => y.TotalAwardedAmount == expectedAmount));
            }

            [TestMethod]
            public async Task NullSaAwardedAmountSetsTotalAwardedAmountToZeroTest()
            {
                var expectedAmount = 0m;
                expectedRepository.SaStudentData.ForEach(sa => sa.TotalAwardedAmount = null);
                expectedRepository.FaStudentData.FaSaYears.AddRange(expectedRepository.SaStudentData.Select(sa => sa.AwardYear));
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => y.TotalAwardedAmount == expectedAmount));
            }

            [TestMethod]
            public async Task FalseApplicationReviewedWhenNoYsYearTest()
            {
                expectedRepository.FaStudentData.FaYsYears = new List<string>();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => !y.IsApplicationReviewed));
            }

            [TestMethod]
            public async Task TrueApplicationReviewedTest()
            {
                expectedRepository.YsStudentData.ForEach(y => y.ApplicationReviewedDate = DateTime.Today);
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                actualStudentAwardYears.ForEach(y =>
                {
                    if (expectedRepository.FaStudentData.FaYsYears.Contains(y.Code))
                    {
                        Assert.IsTrue(y.IsApplicationReviewed);
                    }
                    else
                    {
                        Assert.IsFalse(y.IsApplicationReviewed);
                    }
                });
            }

            [TestMethod]
            public async Task FalseApplicationReviewedWhenNullYsRecordTest()
            {
                expectedRepository.YsStudentData = new List<TestStudentAwardYearRepository.StudentYs>();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => !y.IsApplicationReviewed));
            }

            [TestMethod]
            public async Task FalseApplicationReviewedIfNoApplicationCompleteDateTest()
            {
                expectedRepository.YsStudentData.ForEach(y => y.ApplicationReviewedDate = null);
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => !y.IsApplicationReviewed));
            }

            /// <summary>
            /// Tests if the FederallyFlaggedIsirId property is set to an empty string 
            /// when the year is included in the finaid response csYears list but
            /// csAcyr record for the year is null
            /// </summary>
            [TestMethod]
            public async Task FederallyFlaggedIsirIdEmptyString_NullCsRecordTest()
            {
                expectedRepository.CsStudentData = new List<TestStudentAwardYearRepository.CsStudent>();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => string.IsNullOrEmpty(y.FederallyFlaggedIsirId)));
            }

            /// <summary>
            /// Tests if the FederallyFlaggedIsirId property is set to an empty string 
            /// when the year is not included in the finaid response csYears list 
            /// </summary>
            [TestMethod]
            public async Task FederallyFlaggedIsirIdEmptyString_YearNotInCsYearListTest()
            {
                expectedRepository.FaStudentData.FaCsYears = new List<string>();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => string.IsNullOrEmpty(y.FederallyFlaggedIsirId)));

            }

            /// <summary>
            /// Tests if FederallyFlaggedIsirId is set to the expected value whenever
            /// the year is in the finaid csYear list and csAcyr record for that year is not null
            /// </summary>
            [TestMethod]
            public async Task FederallyFlaggedIsirId_EqualsCsRecordFedIsirIdTest()
            {
                var fooYear = "FOO";
                var isirId = "BAR";
                expectedRepository.FaStudentData.FaCsYears.Add(fooYear);
                expectedRepository.CsStudentData.Add(new TestStudentAwardYearRepository.CsStudent()
                {
                    AwardYear = fooYear,
                    FedIsirId = isirId
                });
                officeRepository.officeParameterRecordData.Add(new TestFinancialAidOfficeRepository.OfficeParameterRecord() { AwardYear = "FOO", IsSelfServiceActive = "Y", OfficeCode = "MAIN" });
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                var actualYear = actualStudentAwardYears.First(y => y.Code == fooYear);
                Assert.AreEqual(isirId, actualYear.FederallyFlaggedIsirId);
            }

            /// <summary>
            /// Tests if the IsPaperCopyOptionSelected is false when the faPaperCopyOptInFlag is null
            /// </summary>
            [TestMethod]
            public async Task NotIsPaperCopyOptionSelected_WhenOptInFlagIsNullTest()
            {
                expectedRepository.FaStudentData.FaPaperCopyOptInFlag = null;
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => !y.IsPaperCopyOptionSelected));
            }

            /// <summary>
            /// Tests if the IsPaperCopyOptionSelected is false when the faPaperCopyOptInFlag is an
            /// empty string
            /// </summary>
            [TestMethod]
            public async Task NotIsPaperCopyOptionSelected_WhenOptInFlagIsEmptyStringTest()
            {
                expectedRepository.FaStudentData.FaPaperCopyOptInFlag = string.Empty;
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => !y.IsPaperCopyOptionSelected));
            }

            /// <summary>
            /// Tests if the IsPaperCopyOptionSelected is false when the faPaperCopyOptInFlag is "N"
            /// </summary>
            [TestMethod]
            public async Task NotIsPaperCopyOptionSelected_WhenOptInFlagIsNTest()
            {
                expectedRepository.FaStudentData.FaPaperCopyOptInFlag = "N";
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => !y.IsPaperCopyOptionSelected));
            }

            /// <summary>
            /// Tests if the IsPaperCopyOptionSelected is true when the faPaperCopyOptInFlag is "Y"
            /// </summary>
            [TestMethod]
            public async Task IsPaperCopyOptionSelected_WhenOptInFlagIsYTest()
            {
                expectedRepository.FaStudentData.FaPaperCopyOptInFlag = "Y";
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService)).ToList();
                Assert.IsTrue(actualStudentAwardYears.All(y => y.IsPaperCopyOptionSelected));

            }

            [TestMethod]
            public async Task CatchAndLogErrorCreatingStudentAwardYearTest()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<CsAcyr>(It.IsAny<string>(), It.IsAny<string>(), true)).Throws(new Exception("ex"));

                await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService);

                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            }

            [TestMethod]
            public void GetActiveYearsOnlyFalse_AllYearsActive_ReturnsAllYearsTest()
            {
                var expectedYearsCount = (expectedRepository.FaStudentData.FaCsYears.Concat(expectedRepository.FaStudentData.FaSaYears).Concat(expectedRepository.FaStudentData.FaYsYears)).Distinct().Count();
                Assert.AreEqual(expectedYearsCount, actualStudentAwardYears.Count);
            }

            [TestMethod]
            public async Task GetActiveYearsOnlyFalse_SomeInactiveYears_ReturnsAllYearsTest()
            {
                officeRepository.officeParameterRecordData.First().IsSelfServiceActive = "N";
                officeRepository.officeParameterRecordData.Last().IsSelfServiceActive = "N";
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                var expectedYearsCount = (expectedRepository.FaStudentData.FaCsYears.Concat(expectedRepository.FaStudentData.FaSaYears).Concat(expectedRepository.FaStudentData.FaYsYears))
                    .Distinct().Count();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService, false)).ToList();
                Assert.AreEqual(expectedYearsCount, actualStudentAwardYears.Count);
            }

            [TestMethod]
            public async Task GetActiveYearsOnlyFalse_AllInactiveYears_ReturnsAllYearsTest()
            {
                officeRepository.officeParameterRecordData.ForEach(o => o.IsSelfServiceActive = "N");
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                var expectedYearsCount = (expectedRepository.FaStudentData.FaCsYears.Concat(expectedRepository.FaStudentData.FaSaYears).Concat(expectedRepository.FaStudentData.FaYsYears))
                    .Distinct().Count();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService, false)).ToList();
                Assert.AreEqual(expectedYearsCount, actualStudentAwardYears.Count);
            }

            [TestMethod]
            public async Task GetActiveYearsOnlyTrue_AllYearsActive_ReturnsAllYearsTest()
            {
                var expectedYearsCount = (expectedRepository.FaStudentData.FaCsYears.Concat(expectedRepository.FaStudentData.FaSaYears).Concat(expectedRepository.FaStudentData.FaYsYears)).Distinct().Count();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService, true)).ToList();
                Assert.AreEqual(expectedYearsCount, actualStudentAwardYears.Count);
            }

            [TestMethod]
            public async Task GetActiveYearsOnlyTrue_SomeInactiveYears_ReturnsActiveYearsOnlyTest()
            {
                officeRepository.officeParameterRecordData.First().IsSelfServiceActive = "N";
                officeRepository.officeParameterRecordData.Last().IsSelfServiceActive = "N";
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                var expectedYearsCount = (expectedRepository.FaStudentData.FaCsYears.Concat(expectedRepository.FaStudentData.FaSaYears).Concat(expectedRepository.FaStudentData.FaYsYears))
                    .Distinct().Except(new List<string>() { officeRepository.officeParameterRecordData.First().AwardYear, officeRepository.officeParameterRecordData.Last().AwardYear }).Count();
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService, true)).ToList();
                Assert.AreEqual(expectedYearsCount, actualStudentAwardYears.Count);
            }

            [TestMethod]
            public async Task GetActiveYearsOnlyTrue_AllInactiveYears_ReturnsNoYearsTest()
            {
                officeRepository.officeParameterRecordData.ForEach(o => o.IsSelfServiceActive = "N");
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                
                actualStudentAwardYears = (await actualRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService, true)).ToList();
                Assert.IsTrue(!actualStudentAwardYears.Any());
            }
        }

        [TestClass]
        public class UpdatePaperCopyOptionFlag : StudentAwardYearRepositoryTests
        {

            public StudentAwardYear actualStudentAwardYear
            { get { return actualRepository.UpdateStudentAwardYear(inputStudentAwardYear); } }

            public StudentAwardYear inputStudentAwardYear;

            public string awardYear;

            [TestInitialize]
            public void Initialize()
            {
                StudentAwardYearRepositoryTestsInitialize();   

                awardYear = "1986";
                inputStudentAwardYear = new StudentAwardYear(studentId, awardYear, currentOfficeService.GetDefaultOffice())
                {
                    IsPaperCopyOptionSelected = false
                };
            }

            /// <summary>
            /// Tests if the actual student award year matches the expected 
            /// studentAwardYear 
            /// </summary>
            [TestMethod]
            public void UpdatedPaperCopyOptionFlagToFalseTest_EqualsExpectedTest()
            {
                Assert.AreEqual(inputStudentAwardYear, actualStudentAwardYear);
                Assert.AreEqual(inputStudentAwardYear.IsPaperCopyOptionSelected, actualStudentAwardYear.IsPaperCopyOptionSelected);
            }

            /// <summary>
            /// Tests if the actual student award year matches the expected 
            /// studentAwardYear 
            /// </summary>
            [TestMethod]
            public void UpdatedPaperCopyOptionFlagToTrueTest_EqualsExpectedTest()
            {
                inputStudentAwardYear.IsPaperCopyOptionSelected = true;
                Assert.AreEqual(inputStudentAwardYear, actualStudentAwardYear);
                Assert.AreEqual(inputStudentAwardYear.IsPaperCopyOptionSelected, actualStudentAwardYear.IsPaperCopyOptionSelected);
            }

            /// <summary>
            /// Tests if ArgumentNullException is thrown when
            /// the argument is null
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAwardYearArgIsNull_ExceptionThrownTest()
            {
                inputStudentAwardYear = null;
                var test = actualStudentAwardYear;
            }

            /// <summary>
            /// Tests if the update transaction was invoked
            /// </summary>
            [TestMethod]
            public void VerifyTransactionWasInvokedTest()
            {
                var test = actualStudentAwardYear;
                transManagerMock.Verify(t => t.Execute<UpdateCorrOptionFlagRequest, UpdateCorrOptionFlagResponse>(It.IsAny<UpdateCorrOptionFlagRequest>()));
            }

            /// <summary>
            /// Test if the studentId and corrOptionFlag od the inputStudentAwardYear
            /// match the corresponding ones of the update requets
            /// </summary>
            [TestMethod]
            public void UpdateRequestMatchesInputDataTest()
            {
                var test = actualStudentAwardYear;
                Assert.AreEqual(inputStudentAwardYear.StudentId, actualUpdateRequest.StudentId);
                Assert.AreEqual(inputStudentAwardYear.IsPaperCopyOptionSelected, actualUpdateRequest.PaperCopyOptionFlag);
            }

            /// <summary>
            /// Tests if OperationCanceledException is thrown when the FIN.AID record is locked and
            /// the appropriate message is logged
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(OperationCanceledException))]
            public void FinAidRecordLocked_ExceptionThrownTest()
            {
                expectedRepository.transactionResponseData.ErrorMessage = string.Format("Conflict: FIN.AID record for student {0} is locked by a process", studentId);

                try
                {
                    var test = actualStudentAwardYear;
                }
                catch (OperationCanceledException)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Paper copy option flag update canceled because record id {0} in FIN.AID table is locked.", inputStudentAwardYear.StudentId)));
                    throw;
                }
            }

            /// <summary>
            /// Tests if no exception is thrown if the error message from
            /// the transaction response is different than a record lock
            /// </summary>           
            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public void DifferentErrorMessage_NoExceptionThrownTest()
            {
                expectedRepository.transactionResponseData.ErrorMessage = "Different error message";

                try
                {
                    var test = actualStudentAwardYear;
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains(expectedRepository.transactionResponseData.ErrorMessage))));
                    throw;
                }
            }
        }

        [TestClass]
        public class UpdatePaperCopyOptionFlagAsyncTests : StudentAwardYearRepositoryTests
        {

            public StudentAwardYear actualStudentAwardYear;
            
            public StudentAwardYear inputStudentAwardYear;

            public string awardYear;

            [TestInitialize]
            public async void Initialize()
            {
                StudentAwardYearRepositoryTestsInitializeAsync();

                awardYear = "1986";
                inputStudentAwardYear = new StudentAwardYear(studentId, awardYear, currentOfficeService.GetDefaultOffice())
                {
                    IsPaperCopyOptionSelected = false
                };
                actualStudentAwardYear = await actualRepository.UpdateStudentAwardYearAsync(inputStudentAwardYear);
            }

            /// <summary>
            /// Tests if the actual student award year matches the expected 
            /// studentAwardYear 
            /// </summary>
            [TestMethod]
            public void UpdatedPaperCopyOptionFlagToFalseTest_EqualsExpectedTest()
            {
                Assert.AreEqual(inputStudentAwardYear, actualStudentAwardYear);
                Assert.AreEqual(inputStudentAwardYear.IsPaperCopyOptionSelected, actualStudentAwardYear.IsPaperCopyOptionSelected);
            }

            /// <summary>
            /// Tests if the actual student award year matches the expected 
            /// studentAwardYear 
            /// </summary>
            [TestMethod]
            public async Task UpdatedPaperCopyOptionFlagToTrueTest_EqualsExpectedTest()
            {
                inputStudentAwardYear.IsPaperCopyOptionSelected = true;
                actualStudentAwardYear = await actualRepository.UpdateStudentAwardYearAsync(inputStudentAwardYear);
                Assert.AreEqual(inputStudentAwardYear, actualStudentAwardYear);
                Assert.AreEqual(inputStudentAwardYear.IsPaperCopyOptionSelected, actualStudentAwardYear.IsPaperCopyOptionSelected);
            }

            /// <summary>
            /// Tests if ArgumentNullException is thrown when
            /// the argument is null
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardYearArgIsNull_ExceptionThrownTest()
            {
                inputStudentAwardYear = null;
                await actualRepository.UpdateStudentAwardYearAsync(inputStudentAwardYear);
            }

            /// <summary>
            /// Tests if the update transaction was invoked
            /// </summary>
            [TestMethod]
            public async Task VerifyTransactionWasInvokedTest()
            {
                await actualRepository.UpdateStudentAwardYearAsync(inputStudentAwardYear);
                transManagerMock.Verify(t => t.ExecuteAsync<UpdateCorrOptionFlagRequest, UpdateCorrOptionFlagResponse>(It.IsAny<UpdateCorrOptionFlagRequest>()));
            }

            /// <summary>
            /// Test if the studentId and corrOptionFlag od the inputStudentAwardYear
            /// match the corresponding ones of the update requets
            /// </summary>
            [TestMethod]
            public async Task UpdateRequestMatchesInputDataTest()
            {
                var test = await actualRepository.UpdateStudentAwardYearAsync(inputStudentAwardYear);
                Assert.AreEqual(inputStudentAwardYear.StudentId, actualUpdateRequest.StudentId);
                Assert.AreEqual(inputStudentAwardYear.IsPaperCopyOptionSelected, actualUpdateRequest.PaperCopyOptionFlag);
            }

            /// <summary>
            /// Tests if OperationCanceledException is thrown when the FIN.AID record is locked and
            /// the appropriate message is logged
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(OperationCanceledException))]
            public async Task FinAidRecordLocked_ExceptionThrownTest()
            {
                expectedRepository.transactionResponseData.ErrorMessage = string.Format("Conflict: FIN.AID record for student {0} is locked by a process", studentId);

                try
                {
                    await actualRepository.UpdateStudentAwardYearAsync(inputStudentAwardYear);
                }
                catch (OperationCanceledException)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Paper copy option flag update canceled because record id {0} in FIN.AID table is locked.", inputStudentAwardYear.StudentId)));
                    throw;
                }
            }

            /// <summary>
            /// Tests if no exception is thrown if the error message from
            /// the transaction response is different than a record lock
            /// </summary>           
            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task DifferentErrorMessage_NoExceptionThrownTest()
            {
                expectedRepository.transactionResponseData.ErrorMessage = "Different error message";

                try
                {
                    await actualRepository.UpdateStudentAwardYearAsync(inputStudentAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.Is<string>(s => s.Contains(expectedRepository.transactionResponseData.ErrorMessage))));
                    throw;
                }
            }
        }

        [TestClass]
        public class GetStudentAwardYearAsyncTests : StudentAwardYearRepositoryTests
        {
            private string awardYearCode;
            private StudentAwardYear expectedStudentAwardYear;
            private StudentAwardYear actualStudentAwardYear;

            [TestInitialize]
            public void Initialize()
            {
                StudentAwardYearRepositoryTestsInitializeAsync();
                awardYearCode = expectedRepository.CsStudentData.First().AwardYear;
                expectedStudentAwardYear = expectedRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService).Result;                
            }

            [TestMethod]
            public async Task ActualAwardYear_EqualsExpectedTest()
            {
                actualStudentAwardYear = await actualRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService);
                Assert.AreEqual(expectedStudentAwardYear, actualStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentId_ArgumentNullExceptionThrownTest()
            {
                await actualRepository.GetStudentAwardYearAsync(null, awardYearCode, currentOfficeService);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmptyAwardYearCode_ArgumentNullExceptionThrownTest()
            {
                await actualRepository.GetStudentAwardYearAsync(studentId, string.Empty, currentOfficeService);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullCurrentOfficeService_ArgumentNullExceptionThrownTest()
            {
                await actualRepository.GetStudentAwardYearAsync(studentId, awardYearCode, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullFinAidRecord_KeyNotFoundExceptionThrownTest()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<FinAid>(It.IsAny<string>(), true))
                .ReturnsAsync(() => null);
                await actualRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService);
            }

            [TestMethod]
            public async Task NoAssociatedYear_StudentAwardYearIsNullTest()
            {
                expectedRepository.FaStudentData.FaCsYears = new List<string>();
                expectedRepository.FaStudentData.FaSaYears = new List<string>();
                expectedRepository.FaStudentData.FaYsYears = new List<string>();

                Assert.IsNull(await actualRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService));
            }

            [TestMethod]
            public async Task NoCsAcyrRecord_StudentAwardYearNotNullTest()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<CsAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(() => null);
                Assert.IsNotNull(await actualRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService));
            }

            [TestMethod]
            public async Task NotCsYear_StudentAwardYearNotNullTest()
            {
                expectedRepository.FaStudentData.FaCsYears = new List<string>();
                expectedRepository.FaStudentData.FaSaYears.Add(awardYearCode);
                Assert.IsNotNull(await actualRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService));
            }

            [TestMethod]
            public async Task NotCsYear_DefaultOfficeAssignedTest()
            {
                expectedRepository.FaStudentData.FaCsYears = new List<string>();
                expectedRepository.FaStudentData.FaYsYears.Add(awardYearCode);
                actualStudentAwardYear = await actualRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService);
                Assert.IsTrue(actualStudentAwardYear.CurrentOffice.IsDefault);
            }

            [TestMethod]
            public async Task NoFinAidRecordYears_NoExceptionThrownTest()
            {
                bool exceptionThrown = false;
                expectedRepository.FaStudentData.FaCsYears = null;
                expectedRepository.FaStudentData.FaYsYears = null;
                expectedRepository.FaStudentData.FaSaYears = null;
                try
                {
                    await actualRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService);
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }
        }
    }
}
