/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Dmi.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    /// <summary>
    /// This class tests the StudentAwardRepository
    /// </summary>
    [TestClass]
    public class StudentAwardRepositoryTests : BaseRepositorySetup
    {
        public List<Award> allAwards;
        public List<AwardStatus> allAwardStatuses;
        public List<StudentAwardYear> studentAwardYears;
        public CurrentOfficeService currentOfficeService;

        public TestStudentAwardYearRepository testStudentAwardYearRepository;
        public TestFinancialAidReferenceDataRepository testFinancialAidReferenceDataRepository;
        public TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;

        public TestStudentAwardRepository expectedStudentAwardRepository;
        public StudentAwardRepository actualStudentAwardRepository;

        public string studentId;

        public UpdateStudentAwardRequest actualStudentAwardRequestTransaction;
        public UpdateStudentAwardResponse updateStudentAwardResponseTransaction;

        public void BaseInitialize()
        {
            MockInitialize();

            studentId = "0003914";

            testFinancialAidReferenceDataRepository = new TestFinancialAidReferenceDataRepository();
            allAwards = testFinancialAidReferenceDataRepository.Awards.ToList();
            allAwardStatuses = testFinancialAidReferenceDataRepository.AwardStatuses.ToList();

            testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            currentOfficeService = new CurrentOfficeService(testFinancialAidOfficeRepository.GetFinancialAidOffices());

            testStudentAwardYearRepository = new TestStudentAwardYearRepository();
            studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService).ToList();

            expectedStudentAwardRepository = new TestStudentAwardRepository();

            updateStudentAwardResponseTransaction = new UpdateStudentAwardResponse();

            actualStudentAwardRepository = BuildStudentAwardRepository();
        }

        /// <summary>
        /// Build a StudentAwardRepository by setting up db record records to return test response data.
        /// </summary>
        /// <returns>A StudentAwardRepository object</returns>
        private StudentAwardRepository BuildStudentAwardRepository()
        {

            //Mock the record reads
            dataReaderMock
                .Setup<Task<Collection<TaAcyr>>>(reader => reader.BulkReadRecordAsync<TaAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))                    
                .Returns<string, string, bool>(
                    (acyrFileName, criteria, convertText) =>
                    {
                        return Task.FromResult(new Collection<TaAcyr>(
                            expectedStudentAwardRepository.awardPeriodData
                            .Where(ta =>
                                ta.year == acyrFileName.Split('.')[1]
                            ).Select(ta =>
                                new TaAcyr()
                                {
                                    Recordkey = string.Format("{0}*{1}*{2}", studentId, ta.award, ta.awardPeriod),
                                    TaTermAmount = ta.awardAmount,
                                    TaTermAction = ta.awardStatus,
                                    TaTermXmitAmt = ta.xmitAmount
                                }
                            ).ToList())
                        );
                    }
                );

            dataReaderMock
                .Setup<Task<Collection<TaAcyr>>>(reader => reader.BulkReadRecordAsync<TaAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>(
                    (acyrFileName, criteria, convertText) =>
                    {
                        return Task.FromResult(new Collection<TaAcyr>(
                            expectedStudentAwardRepository.awardPeriodData
                            .Where(ta =>
                                ta.year == acyrFileName.Split('.')[1]
                            ).Select(ta =>
                                new TaAcyr()
                                {
                                    Recordkey = string.Format("{0}*{1}*{2}", studentId, ta.award, ta.awardPeriod),
                                    TaTermAmount = ta.awardAmount,
                                    TaTermAction = ta.awardStatus,
                                    TaTermXmitAmt = ta.xmitAmount
                                }
                            ).ToList()));
                    }
                );

            dataReaderMock
                .Setup<Task<SaAcyr>>(reader => reader.ReadRecordAsync<SaAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>(
                    (acyrFileName, key, convertText) =>
                    {
                        return Task.FromResult(expectedStudentAwardRepository.awardData
                            .Where(sa =>
                                sa.awardYear == acyrFileName.Split('.')[1] //e.g. SA.2013
                            ).Select(sa =>
                                new SaAcyr()
                                {
                                    Recordkey = studentId,
                                    SaAward = sa.awardCodes,
                                    StatusForTermEntityAssociation = sa.periodAssociation
                                        .Select(term =>
                                            new SaAcyrStatusForTerm()
                                            {
                                                SaTermsAssocMember = term.awardPeriodId,
                                                SaAtpFreezeAssocMember = term.isFrozenOnAttendancePattern ? "Y" : string.Empty
                                            }
                                        ).ToList()
                                }
                            ).FirstOrDefault());
                    }
                );

            dataReaderMock
                .Setup<Task<SaAcyr>>(reader => reader.ReadRecordAsync<SaAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>(
                    (acyrFileName, key, convertText) =>
                    {
                        return Task.FromResult(expectedStudentAwardRepository.awardData
                            .Where(sa =>
                                sa.awardYear == acyrFileName.Split('.')[1] //e.g. SA.2013
                            ).Select(sa =>
                                new SaAcyr()
                                {
                                    Recordkey = studentId,
                                    SaAward = sa.awardCodes,
                                    StatusForTermEntityAssociation = sa.periodAssociation
                                        .Select(term =>
                                            new SaAcyrStatusForTerm()
                                            {
                                                SaTermsAssocMember = term.awardPeriodId,
                                                SaAtpFreezeAssocMember = term.isFrozenOnAttendancePattern ? "Y" : string.Empty
                                            }
                                        ).ToList()
                                }
                            ).FirstOrDefault());
                    }
                );

            dataReaderMock
                .Setup<Task<Collection<SlAcyr>>>(reader => reader.BulkReadRecordAsync<SlAcyr>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string, string[], bool>(
                    (acyrFileName, recordIds, convertText) =>
                    {
                        return Task.FromResult(new Collection<SlAcyr>(
                            expectedStudentAwardRepository.loanData
                            .Where(sl =>
                                sl.awardYear == acyrFileName.Split('.')[1] && recordIds.Contains(string.Format("{0}*{1}", studentId, sl.awardId))
                            ).Select(sl =>
                                new SlAcyr()
                                {
                                    Recordkey = string.Format("{0}*{1}", studentId, sl.awardId),
                                    SlAntDisbTerm = sl.anticipatedDisbursementAwardPeriodIds
                                }
                            ).ToList())
                        );
                    }
                );

            dataReaderMock
                .Setup<Task<SlAcyr>>(reader => reader.ReadRecordAsync<SlAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>(
                    (acyrFileName, recordId, convertText) =>
                    {
                        return Task.FromResult(expectedStudentAwardRepository.loanData
                            .Where(sl =>
                                sl.awardYear == acyrFileName.Split('.')[1] && recordId.Equals(string.Format("{0}*{1}", studentId, sl.awardId))
                            ).Select(sl =>
                                new SlAcyr()
                                {
                                    Recordkey = string.Format("{0}*{1}", studentId, sl.awardId),
                                    SlAntDisbTerm = sl.anticipatedDisbursementAwardPeriodIds
                                }
                            ).FirstOrDefault());
                    }
                );

            transManagerMock.Setup<Task<Transactions.UpdateStudentAwardResponse>>(manager =>
                manager.ExecuteAsync<UpdateStudentAwardRequest, UpdateStudentAwardResponse>(It.IsAny<UpdateStudentAwardRequest>()))
                    .Callback<UpdateStudentAwardRequest>((req) => actualStudentAwardRequestTransaction = req)
                    .Returns<UpdateStudentAwardRequest>((req) => Task.FromResult(updateStudentAwardResponseTransaction));


            return new StudentAwardRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        /// <summary>
        /// This class tests the GetStudentAwards method of the StudentAwardRepository
        /// </summary>
        [TestClass]
        public class GetAllStudentAwardsTests : StudentAwardRepositoryTests
        {
            public IEnumerable<StudentAward> expectedStudentAwards;
            public IEnumerable<StudentAward> actualStudentAwards;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();
                expectedStudentAwards = expectedStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses).Result;
                actualStudentAwards = await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);
            }

            /// <summary>
            /// Test that for each StudentAward built by the repository there is an equal StudentAward in the test
            /// data
            /// </summary>
            [TestMethod]
            public void StudentAwardListsAreEqualTest()
            {
                Assert.IsTrue(actualStudentAwards.Count() > 0);
                Assert.AreEqual(expectedStudentAwards.Count(), actualStudentAwards.Count());

                foreach (var repoStudentAward in actualStudentAwards)
                {
                    var studentAward = expectedStudentAwards.FirstOrDefault(sa => sa.Equals(repoStudentAward));
                    Assert.IsNotNull(studentAward);
                }
            }

            /// <summary>
            /// Test that for each StudentAwardPeriod built by the repository there is an equal StudentAwardPeriod in the 
            /// test data 
            /// </summary>
            [TestMethod]
            public void StudentAwardPeriodListsAreEqualTest()
            {
                Assert.IsTrue(actualStudentAwards.Count() > 0);

                foreach (var repoStudentAward in actualStudentAwards)
                {
                    var testStudentAward = expectedStudentAwards.FirstOrDefault(sa => sa.Equals(repoStudentAward));

                    Assert.IsTrue(repoStudentAward.StudentAwardPeriods.Count() > 0);

                    foreach (var repoStudentAwardPeriod in repoStudentAward.StudentAwardPeriods)
                    {
                        var studentAwardPeriod = testStudentAward.StudentAwardPeriods.FirstOrDefault(sap => sap.Equals(repoStudentAwardPeriod));

                        Assert.IsNotNull(studentAwardPeriod);
                        Assert.AreEqual(studentAwardPeriod.AwardAmount, repoStudentAwardPeriod.AwardAmount);
                        Assert.AreEqual(studentAwardPeriod.AwardStatus, repoStudentAwardPeriod.AwardStatus);
                        Assert.AreEqual(studentAwardPeriod.IsAmountModifiable, repoStudentAwardPeriod.IsAmountModifiable);
                    }
                }
            }

            /// <summary>
            /// Test that a NullArgumentException is thrown when passing a null or empty string to
            /// the get method.
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentIdException()
            {
                await actualStudentAwardRepository.GetAllStudentAwardsAsync("", studentAwardYears, allAwards, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentAwardYearsException()
            {
                await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, null, allAwards, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAllAwardsException()
            {
                await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, null, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAllAwardStatusesException()
            {
                await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task StudentIdDifferentThanStudentAwardYears_ThrowsException()
            {
                await actualStudentAwardRepository.GetAllStudentAwardsAsync("foobar", studentAwardYears, allAwards, allAwardStatuses);
            }

            [TestMethod]
            public async Task EmptyStudentAwardYearsReturnsEmptyListTest()
            {
                studentAwardYears = new List<StudentAwardYear>();
                var studentAwardEmptyList = await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);
                Assert.AreEqual(0, studentAwardEmptyList.Count());
            }

            /// <summary>
            /// Tests that an empty list of StudentAwards is returned when accessing Award data for a student
            /// who is in the FinancialAid system, has SA.ACYR records, but has no awards (TA.ACYR records).
            /// </summary>
            [TestMethod]
            public async Task NoAwardDataReturnsEmptyListTest()
            {
                expectedStudentAwardRepository.awardPeriodData = new List<TestStudentAwardRepository.TestAwardPeriodRecord>();
                // actualStudentAwardRepository = BuildStudentAwardRepository();
                var studentAwardEmptyList = await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);

                Assert.IsTrue(studentAwardEmptyList.Count() == 0);
            }

            /// <summary>
            /// When an exception is thrown for an award year that award year is skipped.
            /// Test that the StudentAward list returned from the repository does not contain the year's award data in which an exception was thrown
            /// </summary>
            [TestMethod]
            public async Task CatchExceptionAndSkipYearTest()
            {
                //setup the first year in the response data with an error message
                //var firstYearResponseData = studentAwardYear.First();
                var yearToBeRemoved = studentAwardYears.First().Code;
                //firstYearResponseData.ErrorMessage = "Error Message";

                //Rebuild the Mock repository with the updated data
                actualStudentAwardRepository = BuildStudentAwardRepository();
                dataReaderMock
                    .Setup<Task<SaAcyr>>(reader => reader.ReadRecordAsync<SaAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new Exception("test exception"));

                //Get the StudentAwards
                var studentAwardsMinusFirstYear = await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);

                var noFirstYearStudentAwards = studentAwardsMinusFirstYear.Where(s => s.StudentAwardYear.Code == yearToBeRemoved);
                Assert.AreEqual(0, noFirstYearStudentAwards.Count());
            }
        }


        [TestClass]
        public class GetStudentAwardsForYearTests : StudentAwardRepositoryTests
        {
            public StudentAwardYear studentAwardYear;
            public IEnumerable<StudentAward> expectedStudentAwardsForYear;
            public IEnumerable<StudentAward> actualStudentAwardsForYear;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                studentAwardYear = studentAwardYears.First();

                expectedStudentAwardsForYear = expectedStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses).Result;
                actualStudentAwardsForYear = await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses);
            }

            //counts are equal
            [TestMethod]
            public void NumberOfStudentAwardsAreEqualTest()
            {
                Assert.AreEqual(expectedStudentAwardsForYear.Count(), actualStudentAwardsForYear.Count());
            }

            //StudentAwrads are equal
            [TestMethod]
            public void StudentAwardsAreEqualTest()
            {
                foreach (var expectedStudentAward in expectedStudentAwardsForYear)
                {
                    var actualStudentAward = actualStudentAwardsForYear.First(s => s.Equals(expectedStudentAward));
                    Assert.AreEqual(expectedStudentAward.IsEligible, actualStudentAward.IsEligible);
                    Assert.AreEqual(expectedStudentAward.IsAmountModifiable, actualStudentAward.IsAmountModifiable);
                    Assert.AreEqual(expectedStudentAward.StudentAwardPeriods.Count(), actualStudentAward.StudentAwardPeriods.Count());
                }
            }

            //StudentawardPeriods are equal
            [TestMethod]
            public void StudentAwardPeriodsAreEqualTest()
            {
                var expectedStudentAwardPeriods = expectedStudentAwardsForYear.SelectMany(s => s.StudentAwardPeriods);
                var actualStudentAwardPeriods = actualStudentAwardsForYear.SelectMany(s => s.StudentAwardPeriods);
                foreach (var expectedStudentAwardPeriod in expectedStudentAwardPeriods)
                {
                    var actualStudentAwardPeriod = actualStudentAwardPeriods.First(p => p.Equals(expectedStudentAwardPeriod));
                    Assert.AreEqual(expectedStudentAwardPeriod.AwardAmount, actualStudentAwardPeriod.AwardAmount);
                    Assert.AreEqual(expectedStudentAwardPeriod.AwardStatus, actualStudentAwardPeriod.AwardStatus);
                    Assert.AreEqual(expectedStudentAwardPeriod.IsFrozen, actualStudentAwardPeriod.IsFrozen);
                    Assert.AreEqual(expectedStudentAwardPeriod.IsAmountModifiable, actualStudentAwardPeriod.IsAmountModifiable);
                    Assert.AreEqual(expectedStudentAwardPeriod.IsTransmitted, actualStudentAwardPeriod.IsTransmitted);
                }
            }

            //nulls
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudenIdRequiredTest()
            {
                await actualStudentAwardRepository.GetStudentAwardsForYearAsync(null, studentAwardYear, allAwards, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardYearRequiredTest()
            {
                await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, null, allAwards, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AllAwardsRequiredTest()
            {
                await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, null, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AllAwardStatusesRequiredTest()
            {
                await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, null);
            }

            //saAcyr data reader returns null
            //log message, throws knfe
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullSaAcyrRecord_ThrowsNotFoundExceptionTest()
            {
                //no sa acyr data setup for award year foobar
                var badYear = "foobar";
                studentAwardYear = new StudentAwardYear(studentId, badYear, currentOfficeService.GetDefaultOffice());

                try
                {
                    await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses);
                }
                catch (Exception e)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no award data in {1}", studentId, "SA." + badYear)));
                    throw e;
                }
            }

            //saAcyr.SaAward is empty
            //log message throws knfe
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullAwardsInSaAcyrRecord_ThrowsNotFoundExceptionTest()
            {
                expectedStudentAwardRepository.awardData = new List<TestStudentAwardRepository.TestAwardRecord>();

                try
                {
                    await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses);
                }
                catch (Exception e)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no award data in {1}", studentId, "SA." + studentAwardYear.Code)));
                    throw e;
                }
            }

            //saAcyr.SaAward is empty
            //log message throws knfe
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoAwardsInSaAcyrRecord_ThrowsNotFoundExceptionTest()
            {
                var awardSummaryRecord = expectedStudentAwardRepository.awardData.First(a => a.awardYear == studentAwardYear.Code);
                awardSummaryRecord.awardCodes = new List<string>();

                try
                {
                    await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses);
                }
                catch (Exception e)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no award data in {1}", studentId, "SA." + studentAwardYear.Code)));
                    throw e;
                }
            }

            //slAcyr data reader returns null
            //no exception thrown - all award periods for year .hasloandisbursement eq false
            [TestMethod]
            public async Task NullSlAcyrRecord_NoAwardPeriodsHaveDisbursementsTest()
            {
                expectedStudentAwardRepository.loanData = new List<TestStudentAwardRepository.TestLoanRecord>();
                actualStudentAwardsForYear = await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses);

                Assert.IsTrue(actualStudentAwardsForYear.SelectMany(a => a.StudentAwardPeriods).All(p => !p.HasLoanDisbursement));

                loggerMock.Verify(l => l.Info(string.Format("No {0} record ids for student {1} award year {2}", "SL." + studentAwardYear.Code, studentId, studentAwardYear.Code)));
            }

            //studentawardperioddata is null thorws  knfe
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullAwardPeriodData_ThrowsExceptionTest()
            {
                var acyrfile = "TA." + studentAwardYear.Code;
                Collection<TaAcyr> taAcyrCollection = null;
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<TaAcyr>(acyrfile, It.IsAny<string>(), true)).ReturnsAsync(taAcyrCollection);

                await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses);
            }

            //studentawardperiodata is empty throws knfe
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EmptyAwardPeriodData_ThrowsExceptionTest()
            {
                var acyrfile = "TA." + studentAwardYear.Code;
                Collection<TaAcyr> taAcyrCollection = new Collection<TaAcyr>();
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<TaAcyr>(acyrfile, It.IsAny<string>(), true)).ReturnsAsync(taAcyrCollection);

                await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses);
            }

            [TestMethod]
            public async Task MissingAwardCode_SkipAwardAndLogErrorMessageTest()
            {
                var badAwardCode = "FOOBAR";
                var summaryAwardForYear = expectedStudentAwardRepository.awardData.First(t => t.awardYear == studentAwardYear.Code);
                summaryAwardForYear.awardCodes.Add(badAwardCode);

                Assert.IsFalse(allAwards.Select(a => a.Code).Contains(badAwardCode));

                //actualStudentAwardRepository = BuildStudentAwardRepository();
                actualStudentAwardsForYear = await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses);

                var nonExistantStudentAward = actualStudentAwardsForYear.FirstOrDefault(a => a.Award.Code == badAwardCode);
                Assert.IsNull(nonExistantStudentAward);

                loggerMock.Verify(l => l.Error(string.Format("Award {0} is missing from Colleague", badAwardCode)));
            }

            //awardcode from loanchangerestrictionsresponse exists in all awards, but not in taAcyr - periodData count is zero - 
            //skip award and logs error message
            [TestMethod]
            public async Task NoAwardPeriodDataForAward_SkipAwardAndLogErrorMessageTest()
            {
                var badAwardCode = "FOOBAR";
                var summaryAwardForYear = expectedStudentAwardRepository.awardData.First(t => t.awardYear == studentAwardYear.Code);
                summaryAwardForYear.awardCodes.Add(badAwardCode);
                allAwards.Add(new Award(badAwardCode, "desc", new AwardCategory("foo", "bar", null)));

                Assert.IsFalse(expectedStudentAwardRepository.awardPeriodData.Select(t => t.award).Contains(badAwardCode));

                //actualStudentAwardRepository = BuildStudentAwardRepository();
                actualStudentAwardsForYear = await actualStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses);

                var actualStudentAward = actualStudentAwardsForYear.FirstOrDefault(a => a.Award.Code == badAwardCode);
                Assert.IsNull(actualStudentAward);

                loggerMock.Verify(l =>
                    l.Error(It.IsAny<ApplicationException>(),
                    string.Format("Award Period data missing from Colleague for Year {0}, Student {1} and Award {2}", studentAwardYear.Code, studentId, badAwardCode)));

            }
        }

        [TestClass]
        public class BuildStudentAwardPeriodsTests : StudentAwardRepositoryTests
        {

            public IEnumerable<StudentAward> actualStudentAwards;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            //award period has disbursement, has disb = true
            [TestMethod]
            public async Task AwardPeriodHasDisbursementTest()
            {
                var disb = expectedStudentAwardRepository.loanData.First();
                var testAwardPeriod = expectedStudentAwardRepository.awardPeriodData.First(p => p.year == disb.awardYear && p.award == disb.awardId);
                disb.anticipatedDisbursementAwardPeriodIds.Add(testAwardPeriod.awardPeriod);

                actualStudentAwards = await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);
                var actualAwardPeriod = actualStudentAwards.SelectMany(a => a.StudentAwardPeriods).First(p =>
                    p.StudentAwardYear.Code == testAwardPeriod.year &&
                    p.Award.Code == testAwardPeriod.award &&
                    p.AwardPeriodId == testAwardPeriod.awardPeriod);

                Assert.IsTrue(actualAwardPeriod.HasLoanDisbursement);

            }

            //award period has no disbursement, has disb = false,
            [TestMethod]
            public async Task AwardPeriodHasNoDisbursementTest()
            {
                var disb = expectedStudentAwardRepository.loanData.First();
                var testAwardPeriod = expectedStudentAwardRepository.awardPeriodData.First(p => p.year == disb.awardYear && p.award == disb.awardId);
                disb.anticipatedDisbursementAwardPeriodIds = new List<string>();

                actualStudentAwards = await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);
                var actualAwardPeriod = actualStudentAwards.SelectMany(a => a.StudentAwardPeriods).First(p =>
                    p.StudentAwardYear.Code == testAwardPeriod.year &&
                    p.Award.Code == testAwardPeriod.award &&
                    p.AwardPeriodId == testAwardPeriod.awardPeriod);

                Assert.IsFalse(actualAwardPeriod.HasLoanDisbursement);

            }

            //no award period data for award. award is not returned
            [TestMethod]
            public async Task NoAwardPeriodDataForAward_StudentAwardNotCreatedTest()
            {
                var awardCode = "FOOBAR";
                var summaryAward = expectedStudentAwardRepository.awardData.First();
                summaryAward.awardCodes.Add(awardCode);

                allAwards.Add(new Award(awardCode, "foobar", new AwardCategory("f", "d", null)));

                actualStudentAwards = await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);

                Assert.IsFalse(actualStudentAwards.Any(a => a.Award.Code == awardCode));
            }

            //awardPeriod Code doesn't exist . catch exception, log message, award period is not returned
            [TestMethod]
            public async Task BadTaAcyrRecordId_StudentAwardPeriodNotCreatedTest()
            {
                var awardPeriod = expectedStudentAwardRepository.awardPeriodData.First();
                awardPeriod.awardPeriod = "foo*bar";

                actualStudentAwards = await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);
                var actualAwardPeriod = actualStudentAwards.SelectMany(a => a.StudentAwardPeriods).FirstOrDefault(p => p.AwardPeriodId == awardPeriod.awardPeriod);

                Assert.IsNull(actualAwardPeriod); ;
            }
           
            //studentawardperiod.isfrozen = true if loanchangerestricitionstransaction has award period id in frozen award periods
            [TestMethod]
            public async Task AwardPeriodInFrozenList_IsFrozenSetToTrueTest()
            {
                var frozenTaAcyrRecord = expectedStudentAwardRepository.awardPeriodData.First();
                var frozenAwardPeriodId = frozenTaAcyrRecord.awardPeriod;
                expectedStudentAwardRepository.awardData.First(a => a.awardYear == frozenTaAcyrRecord.year).periodAssociation.First(p => p.awardPeriodId == frozenAwardPeriodId).isFrozenOnAttendancePattern = true;

                //actualStudentAwardRepository = BuildStudentAwardRepository();
                actualStudentAwards = await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);

                var frozenStudentAwardPeriods = actualStudentAwards.SelectMany(a => a.StudentAwardPeriods).Where(p => p.AwardPeriodId == frozenAwardPeriodId);

                Assert.IsTrue(frozenStudentAwardPeriods.All(p => p.IsFrozen));
            }

            //studentawradperiod.isfrozne = false if loanchangerestirinstranstion does not have awrad period id in frozne awawrad periods
            [TestMethod]
            public async Task AwardPeriodsNotInFrozenList_IsFrozenSetToFalseTest()
            {
                var frozenTaAcyrRecord = expectedStudentAwardRepository.awardPeriodData.First();
                var frozenAwardPeriodId = frozenTaAcyrRecord.awardPeriod;
                expectedStudentAwardRepository.awardData.First(a => a.awardYear == frozenTaAcyrRecord.year).periodAssociation.First(p => p.awardPeriodId == frozenAwardPeriodId).isFrozenOnAttendancePattern = true;

                actualStudentAwardRepository = BuildStudentAwardRepository();
                actualStudentAwards = await actualStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);

                var frozenStudentAwardPeriods = actualStudentAwards.SelectMany(a => a.StudentAwardPeriods).Where(p => p.AwardPeriodId != frozenAwardPeriodId);

                Assert.IsTrue(frozenStudentAwardPeriods.All(p => !p.IsFrozen));
            }
        }

        [TestClass]
        public class UpdateStudentAwardsTests : StudentAwardRepositoryTests
        {
            public StudentAwardYear inputStudentAwardYear
            {
                get { return studentAwardYears.First(); }
            }
            public IEnumerable<StudentAward> inputStudentAwards
            {
                get { return expectedStudentAwardRepository.GetStudentAwardsForYearAsync(studentId, inputStudentAwardYear, testFinancialAidReferenceDataRepository.Awards, testFinancialAidReferenceDataRepository.AwardStatuses).Result; }
            }

            public List<StudentAward> expectedStudentAwards
            {
                get { return expectedStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, inputStudentAwards, allAwards, allAwardStatuses).Result.ToList(); }
            }
            public IEnumerable<StudentAward> actualStudentAwards;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualStudentAwards = await actualStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, inputStudentAwards, allAwards, allAwardStatuses);
                CollectionAssert.AreEqual(expectedStudentAwards, actualStudentAwards.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardYearRequiredTest()
            {
                await actualStudentAwardRepository.UpdateStudentAwardsAsync(null, inputStudentAwards, allAwards, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAwardsRequiredTest()
            {
                await actualStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, null, allAwards, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task StudentAwardsMustBeSameAwardYearTest()
            {
                var allStudentAwards = expectedStudentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, testFinancialAidReferenceDataRepository.Awards, testFinancialAidReferenceDataRepository.AwardStatuses).Result;
                await actualStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, allStudentAwards, allAwards, allAwardStatuses);
            }

            [TestMethod]
            public async Task ActualRequestTransactionYearTest()
            {
                var updatedAwards = await actualStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, inputStudentAwards, allAwards, allAwardStatuses);
                Assert.IsNotNull(actualStudentAwardRequestTransaction);
                Assert.AreEqual(inputStudentAwardYear.Code, actualStudentAwardRequestTransaction.Year);
            }

            [TestMethod]
            public async Task ActualRequestTransactionStudentIdTest()
            {
                actualStudentAwards = await actualStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, inputStudentAwards, allAwards, allAwardStatuses); 
                Assert.AreEqual(inputStudentAwardYear.StudentId, actualStudentAwardRequestTransaction.StudentId);
            }

            [TestMethod]
            public async Task ActualRequestTransactionAwardIdsTest()
            {
                var updatedAwards = await actualStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, inputStudentAwards, allAwards, allAwardStatuses);
                Assert.AreEqual(inputStudentAwards.Count(), actualStudentAwardRequestTransaction.UpdateAward.Count());

                var awardIds = inputStudentAwards.Select(sa => sa.Award.Code).ToList();
                var actualIds = actualStudentAwardRequestTransaction.UpdateAward.Select(u => u.AwardId).ToList();
                CollectionAssert.AreEqual(awardIds, actualIds);
            }

            [TestMethod]
            public async Task ActualRequestTransactionAwardPeriodIdsTest()
            {
                var updatedAwards = await actualStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, inputStudentAwards, allAwards, allAwardStatuses);
                foreach (var updateAward in actualStudentAwardRequestTransaction.UpdateAward)
                {
                    var equivInputAward = inputStudentAwards.FirstOrDefault(sa => sa.Award.Code == updateAward.AwardId);
                    Assert.IsNotNull(equivInputAward);

                    var periodIds = equivInputAward.StudentAwardPeriods.Select(p => p.AwardPeriodId).ToList();
                    var actualIds = updateAward.AwardPeriodIds.Split(DmiString._SM).ToList();
                    CollectionAssert.AreEqual(periodIds, actualIds);
                }
            }

            [TestMethod]
            public async Task ActualRequestTransactionAwardPeriodAmountsTest()
            {
                var updatedAwards = await actualStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, inputStudentAwards, allAwards, allAwardStatuses);
                foreach (var updateAward in actualStudentAwardRequestTransaction.UpdateAward)
                {
                    var equivInputAward = inputStudentAwards.FirstOrDefault(sa => sa.Award.Code == updateAward.AwardId);
                    Assert.IsNotNull(equivInputAward);

                    var periodIds = equivInputAward.StudentAwardPeriods.Select(p => p.AwardAmount.ToString()).ToList();
                    var actualIds = updateAward.AwardPeriodAmounts.Split(DmiString._SM).ToList();
                    CollectionAssert.AreEqual(periodIds, actualIds);
                }
            }

            [TestMethod]
            public async Task ActualRequestTransactionAwardPeriodStatusesTest()
            {
                var updatedAwards = await actualStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, inputStudentAwards, allAwards, allAwardStatuses);
                foreach (var updateAward in actualStudentAwardRequestTransaction.UpdateAward)
                {
                    var equivInputAward = inputStudentAwards.FirstOrDefault(sa => sa.Award.Code == updateAward.AwardId);
                    Assert.IsNotNull(equivInputAward);

                    var periodIds = equivInputAward.StudentAwardPeriods.Select(p => p.AwardStatus.Code).ToList();
                    var actualIds = updateAward.AwardPeriodStatuses.Split(DmiString._SM).ToList();
                    CollectionAssert.AreEqual(periodIds, actualIds);
                }
            }
                        
            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ErrorMessageInResponseLogsMessageThrowsException()
            {
                updateStudentAwardResponseTransaction.ErrorMessage = "Error Message";
                try
                {
                    await actualStudentAwardRepository.UpdateStudentAwardsAsync(inputStudentAwardYear, inputStudentAwards, allAwards, allAwardStatuses);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(updateStudentAwardResponseTransaction.ErrorMessage));
                    throw;
                }
            }
        }
        /*
        [TestClass]
        public class UpdateStudentAwardsTests : StudentAwardRepositoryTests
        {

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
            }

            [TestMethod]
            public void UpdateSuccessTest()
            {
                var testStudentAward = expectedStudentAwards.First();
                var updatedStudentAward = actualStudentAwardRepository.UpdateStudentAward(testStudentAward, allAwards, allAwardStatuses);

                Assert.AreEqual(expectedStudentAwards.First(), updatedStudentAward);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void UpdateTransactionReturnsGenericErrorMessage()
            {
                updateResponseTestData.ErrorMessage = "foobar";
                actualStudentAwardRepository = BuildStudentAwardRepository();

                var testStudentAward = expectedStudentAwards.First();

                var updatedStudentAward = actualStudentAwardRepository.UpdateStudentAward(testStudentAward, allAwards, allAwardStatuses);
            }
        }

        
        [TestClass]
        public class UpdateStudentAwardTests : BaseRepositorySetup
        {
            /// <summary>
            /// A sub-class of TaAcyr containing one extra field, AwardYear.
            /// The award year is needed to setup Mock to return particular
            /// data for a year.
            /// </summary>
            public class TaAcyrTest : TaAcyr
            {
                public string AwardYear { get; set; }
            }

            /// <summary>
            /// A sub-class of GetLoanChangeRestrictionResponse, containing one extra field, AwardYear.
            /// The award year is needed to setup Mock to return data for a particular year.
            /// </summary>
            public class GetLoanChangeRestrictionsResponseTest : Transactions.GetLoanChangeRestrictionsResponse
            {
                public string AwardYear { get; set; }
            }

            /// <summary>
            /// TaAcyrTest data contract objects, which are actually TaAcyr DataContract objects with an AwardYear,
            /// are setup to be returned during a call to the db in the StudentAwardRepository Get method
            /// These objects are built from test data in the TestStudentAwardRepository.
            /// </summary>
            private Collection<TaAcyrTest> taAcyrTestResponseData;

            /// <summary>
            /// GetLoanChangeRestrctionsResponseTest data contract objects, which are actually GetLoanChangeRestrictionsResponse DataContract objects
            /// with an AwardYear.
            /// These objects are built from test data in the TestStudentAwardRepository
            /// </summary>
            private Collection<GetLoanChangeRestrictionsResponseTest> getLoanChangeResponseTestData;

            private FinAid finAidResponseData;

            private Collection<Awards> awardResponseData;

            private Collection<AwardActions> awardStatusResponseData;

            private IEnumerable<StudentAward> testStudentAwards;

            private StudentAwardRepository studentAwardRepository;

            private IEnumerable<StudentAward> repoStudentAwards;

            private Transactions.UpdateStudentAwardResponse updateResponseTestData;

            private string studentId;

            private Mock<StudentAwardRepository> repositoryMock;
            

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                var testFinancialAidReferenceDataRepository = new TestFinancialAidReferenceDataRepository();
                //awardResponseData = BuildAwardResponseData(testFinancialAidReferenceDataRepository.Awards);
                //awardStatusResponseData = BuildAwardStatusResponseData(testFinancialAidReferenceDataRepository.AwardActionData);

                var testStudentAwardRepository = new TestStudentAwardRepository();
                studentId = testStudentAwardRepository.studentId;

                testStudentAwards = testStudentAwardRepository.GetStudentAwards(studentId);

                updateResponseTestData = new Transactions.UpdateStudentAwardResponse();

                repositoryMock = new Mock<StudentAwardRepository>(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                //repositoryMock.As<IStudentAwardRepository>();

                studentAwardRepository = (StudentAwardRepository) BuildStudentAwardRepository();

            }

            [TestMethod]
            public void UpdateTest()
            {

                var updatedStudentAward = studentAwardRepository.Update(testStudentAwards.First());
                Assert.AreEqual(updatedStudentAward, testStudentAwards.First());
            }

            private StudentAwardRepository BuildStudentAwardRepository()
            {
                transManagerMock.Setup<Transactions.UpdateStudentAwardResponse>(manager => 
                    manager.Execute<Transactions.UpdateStudentAwardRequest, Transactions.UpdateStudentAwardResponse>(
                    It.IsAny<Transactions.UpdateStudentAwardRequest>())).Returns(updateResponseTestData);

                repositoryMock.As<IStudentAwardRepository>().Setup<IEnumerable<StudentAward>>(repo => repo.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(testStudentAwards);

                return repositoryMock.Object;

                //return new StudentAwardRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }
        }*/

        [TestClass]
        public class GetStudentAwardAsyncTests : StudentAwardRepositoryTests
        {
            private StudentAward expectedStudentAward;
            private StudentAward actualStudentAward;
            private StudentAwardYear studentAwardYear;

            private string awardYearCode, awardCode, loanCode;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                awardYearCode = testStudentAwardYearRepository.CsStudentData.First().AwardYear;
                awardCode = expectedStudentAwardRepository.awardData.First(a => a.awardYear == awardYearCode).awardCodes.First();
                loanCode = expectedStudentAwardRepository.loanData.First(a => a.awardYear == awardYearCode).awardId;
                studentAwardYear = testStudentAwardYearRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService).Result;
                expectedStudentAward = expectedStudentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, awardCode, allAwards, allAwardStatuses).Result;                
            }

            [TestMethod]
            public async Task ActualStudentAward_EqualsExpectedTest()
            {
                actualStudentAward = await actualStudentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, awardCode, allAwards, allAwardStatuses);
                Assert.AreEqual(expectedStudentAward, actualStudentAward);
            }

            [TestMethod]
            public async Task ActualStudentLoan_EqualsExpectedTest()
            {
                expectedStudentAward = expectedStudentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, loanCode, allAwards, allAwardStatuses).Result;
                actualStudentAward = await actualStudentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, loanCode, allAwards, allAwardStatuses);
                Assert.AreEqual(expectedStudentAward, actualStudentAward);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentId_ArgumentNullExceptionThrownTest()
            {
                await actualStudentAwardRepository.GetStudentAwardAsync(null, studentAwardYear, awardCode, allAwards, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentAwardYear_ArgumentNullExceptionThrownTest()
            {
                await actualStudentAwardRepository.GetStudentAwardAsync(studentId, null, awardCode, allAwards, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardCode_ArgumentNullExceptionThrownTest()
            {
                await actualStudentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, null, allAwards, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAllAwards_ArgumentNullExceptionThrownTest()
            {
                await actualStudentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, awardCode, null, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAllAwardStatuses_ArgumentNullExceptionThrownTest()
            {
                await actualStudentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, awardCode, allAwards, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UnknownAwardCode_KeyNotFoundExceptionThrownTest()
            {
                await actualStudentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, "foo", allAwards, allAwardStatuses);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoStudentAwardPeriodDataReturned_KeyNotFoundExceptionThrownTest()
            {
                dataReaderMock
                .Setup<Task<Collection<TaAcyr>>>(reader => reader.BulkReadRecordAsync<TaAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(() => null);
                await actualStudentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, awardCode, allAwards, allAwardStatuses);
            }

            [TestMethod]
            public async Task NoDisbursementData_LogsInfoMessageTest()
            {
                dataReaderMock
                .Setup<Task<SlAcyr>>(reader => reader.ReadRecordAsync<SlAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(() => null);
                await actualStudentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, loanCode, allAwards, allAwardStatuses);
                loggerMock.Verify(l => l.Info(string.Format("No loan disbursement data for {0} for student {1} {2} award year", loanCode, studentId, studentAwardYear.Code)));
            }

        }
    }
}
