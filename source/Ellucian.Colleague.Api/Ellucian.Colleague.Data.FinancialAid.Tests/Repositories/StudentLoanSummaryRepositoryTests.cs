//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class StudentLoanSummaryRepositoryTests : BaseRepositorySetup
    {
        //Test respositories
        public TestStudentLoanSummaryRepository expectedRepository;
        public StudentLoanSummaryRepository actualRepository;

        //Test data
        public StudentLoanSummary expectedLoanSummary
        {
            get { return expectedRepository.GetStudentLoanSummaryAsync(studentId).Result; }
        }

        private StudentLoanSummary actualLoanSummary;
        private async Task<StudentLoanSummary> GetLoanSummaryAsync()
        {
            return await actualRepository.GetStudentLoanSummaryAsync(studentId);
        }

        //studentId used throughout
        public string studentId;

        public void StudentLoanSummaryRepositoryTestsInitialize()
        {
            MockInitialize();
            studentId = "0003914";
            expectedRepository = new TestStudentLoanSummaryRepository();
            actualRepository = BuildStudentLoanSummaryRepository();
        }

        public StudentLoanSummaryRepository BuildStudentLoanSummaryRepository()
        {

            dataReaderMock.Setup(fsp => fsp.ReadRecordAsync<FaSysParams>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns<string,string,  bool>((id, x, b) =>
                {
                    if (expectedRepository.FaSysParams == null) return null;
                    return Task.FromResult(new FaSysParams()
                        {
                            FspUseFaInterviewsOnly = expectedRepository.FaSysParams.FspUseFaInterviewsOnly
                        });
                });

            dataReaderMock.Setup(fa => fa.ReadRecordAsync<FinAid>(studentId, true))
                .Returns<string, bool>((id, b) => 
                    {
                        if (expectedRepository.FaStudent == null) return null;
                        return Task.FromResult(new FinAid()
                        {
                            Recordkey = id,
                            FaCodPersonId = expectedRepository.FaStudent.CodPersonId,
                            FaInterviews = expectedRepository.FaStudent.FaInterviewIds,
                            FaIsirNsldsIds = expectedRepository.FaStudent.FaIsirNsldsIds,
                            FaSaYears = expectedRepository.FaStudent.FaSaYears
                        });
                    });

            dataReaderMock.Setup(i => i.BulkReadRecordAsync<FaInterview>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) =>
                    {
                        return Task.FromResult(new Collection<FaInterview>(expectedRepository.StudentInterviews
                       .Where(i => ids.Contains(i.Id))
                       .Select(record =>
                           new FaInterview()
                           {
                               Recordkey = record.Id,
                               FainStudentId = studentId,
                               FainLoanCode = record.InterviewCode,
                               FainEntranceCmplDate = record.CompleteDate
                           }).ToList()));
                    });        

            dataReaderMock.Setup(p => p.ReadRecordAsync<CodPerson>(It.IsAny<string>(), true))
                .Returns<string, bool>((id, b) =>
                    {
                        return Task.FromResult(new CodPerson()
                        {
                            Recordkey = id,
                            CodpCodMpnIds = expectedRepository.StudentMpns.Select(m => m.Id).ToList(),
                        });
                    }
                );

            dataReaderMock.Setup(m => m.BulkReadRecordAsync<CodMpn>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => 
                    {
                        return Task.FromResult(new Collection<CodMpn>(expectedRepository.StudentMpns
                        .Where(m => ids.Contains(m.Id))
                        .Select(record =>
                            new CodMpn()
                            {
                                Recordkey = record.Id,
                                CodmColleagueId = studentId,
                                CodmMpnExpDate = record.ExpirationDate,
                                CodmMpnStatus = record.Status,
                                CodmPnDtTime = record.CreateDate,
                            }).ToList()));
                    });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<IsirNslds>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => 
                    {
                        return Task.FromResult(new Collection<IsirNslds>(expectedRepository.isirNsldsData
                        .Where(Ir => ids.Contains(Ir.id))
                        .Select(record =>
                            new IsirNslds()
                            {
                                Recordkey = record.id,
                                InsdLoanIds = record.IsirNsldsLoanIds,
                                InsdAgCombTotal = record.combinedTotal,
                                InsdAgCombPrBal = record.combinedPrinBal

                            }).ToList()));
                    });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<IsirNsldsLoan>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) => 
                    {
                        return Task.FromResult(new Collection<IsirNsldsLoan>(expectedRepository.isirNsldsLoanData
                        .Where(record => ids.Contains(record.id))
                        .Select(record =>
                            new IsirNsldsLoan()
                            {
                                Recordkey = record.id,
                                InsdlSchoolCode = record.schoolCode,
                                InsdlAggrPrinBal = record.aggregatePrincipalBalance,
                                InsdlProgramCode = record.programCode,
                                InsdlCurrentStatusDate = record.currentStatusDate
                            }).ToList()));
                    });

            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FaInterviewCodes>(It.IsAny<string>(), true))
                .Returns<string, bool>((c, b) =>
                    {
                        return Task.FromResult(new Collection<FaInterviewCodes>(expectedRepository.interviewCodeData
                        .Select(record =>
                            new FaInterviewCodes()
                            {
                                Recordkey = record.id,
                                FicCategory = record.categoryCode
                            }).ToList()));
                    });

            return new StudentLoanSummaryRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetStudentLoanSummaryTests : StudentLoanSummaryRepositoryTests
        {

            [TestInitialize]
            public void Initialize()
            {
                StudentLoanSummaryRepositoryTestsInitialize();
            }

            [TestCleanup]
            public void Cleanup()
            {
                cacheProviderMock = null;
                dataReaderMock = null;
                localCacheMock = null;
                loggerMock = null;
                transFactoryMock = null;
                transManagerMock = null;

                expectedRepository = null;
                actualRepository = null;
            }

            [TestMethod]
            public async Task EqualObjectsTest()
            {
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.AreEqual(expectedLoanSummary, actualLoanSummary);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = null;
                await GetLoanSummaryAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullFinAidRecordTest()
            {
                await actualRepository.GetStudentLoanSummaryAsync("foo");
            }

            [TestMethod]
            public async Task NullInterviewIdsInFinAidTest()
            {
                //For these next series of tests both an Interview and an NSLDS.Loan record need to be
                //accounted for to make sure no Interview Date is returned. If one or the other are
                //properly present an interview date will be returned.
                expectedRepository.FaStudent.FaInterviewIds = null;
                expectedRepository.FaStudent.FaIsirNsldsIds = null;
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNull(actualLoanSummary.DirectLoanEntranceInterviewDate);
                Assert.IsNull(actualLoanSummary.GraduatePlusLoanEntranceInterviewDate);                
            }

            [TestMethod]
            public async Task NoInterviewIdsInFinAidTest()
            {
                expectedRepository.FaStudent.FaInterviewIds = new List<string>();
                expectedRepository.FaStudent.FaIsirNsldsIds = new List<string>();
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNull(actualLoanSummary.DirectLoanEntranceInterviewDate);
                Assert.IsNull(actualLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoInterviewRecordsTest()
            {
                expectedRepository.StudentInterviews = new List<TestStudentLoanSummaryRepository.InterviewRecord>();
                expectedRepository.isirNsldsData = new List<TestStudentLoanSummaryRepository.IsirNsldsRecord>();
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNull(actualLoanSummary.DirectLoanEntranceInterviewDate);
                Assert.IsNull(actualLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoSubCodeInterviewRecordsTest()
            {
                expectedRepository.StudentInterviews.ForEach(i => i.InterviewCode = "FOOBAR");
                expectedRepository.isirNsldsLoanData.ForEach(i => i.programCode = "FOOBAR");
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNull(actualLoanSummary.DirectLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoPlusCodeInterviewRecordsTest()
            {
                expectedRepository.StudentInterviews.ForEach(i => i.InterviewCode = "FOOBAR");
                expectedRepository.isirNsldsLoanData.ForEach(i => i.programCode = "FOOBAR");
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNull(actualLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoSubCategoryTest()
            {
                expectedRepository.interviewCodeData.Add(new TestStudentLoanSummaryRepository.InterviewCodeRecord()
                    {
                        id = "FOOBAR",
                        categoryCode = "F"
                    });

                expectedRepository.StudentInterviews.ForEach(i => i.InterviewCode = "FOOBAR");
                expectedRepository.FaStudent.FaIsirNsldsIds = null;
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNull(actualLoanSummary.DirectLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoPlusCategoryTest()
            {
                expectedRepository.interviewCodeData.Add(new TestStudentLoanSummaryRepository.InterviewCodeRecord()
                {
                    id = "FOOBAR",
                    categoryCode = "F"
                });

                expectedRepository.StudentInterviews.ForEach(i => i.InterviewCode = "FOOBAR");
                expectedRepository.FaStudent.FaIsirNsldsIds = null;
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNull(actualLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoInterviewCodesTest()
            {
                expectedRepository.interviewCodeData = new List<TestStudentLoanSummaryRepository.InterviewCodeRecord>();
                expectedRepository.FaStudent.FaIsirNsldsIds = null;
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNull(actualLoanSummary.DirectLoanEntranceInterviewDate);
                Assert.IsNull(actualLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoInterviewRecordsWithEntranceCompleteDateTest()
            {
                expectedRepository.StudentInterviews.ForEach(i => i.CompleteDate = null);
                expectedRepository.FaStudent.FaIsirNsldsIds = null;
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNull(actualLoanSummary.DirectLoanEntranceInterviewDate);
                Assert.IsNull(actualLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task UseInterviewWithMostRecentCompleteDateTest()
            {
                expectedRepository.FaStudent.FaInterviewIds.Add("FOO");
                expectedRepository.FaStudent.FaInterviewIds.Add("OOF");
                expectedRepository.FaStudent.FaInterviewIds.Add("BAR");
                expectedRepository.FaStudent.FaInterviewIds.Add("RAB");
                var futureDate = new DateTime(9999, 12, 31);
                var notSoFutureDate = new DateTime(9999, 12, 30);
                expectedRepository.StudentInterviews.Add(new TestStudentLoanSummaryRepository.InterviewRecord()
                    {
                        Id = "FOO",
                        CompleteDate = futureDate,
                        InterviewCode = "SUB"
                    });
                expectedRepository.StudentInterviews.Add(new TestStudentLoanSummaryRepository.InterviewRecord()
                {
                    Id = "OOF",
                    CompleteDate = notSoFutureDate,
                    InterviewCode = "SUB"
                });

                expectedRepository.StudentInterviews.Add(new TestStudentLoanSummaryRepository.InterviewRecord()
                {
                    Id = "BAR",
                    CompleteDate = futureDate,
                    InterviewCode = "PLUS"
                });
                expectedRepository.StudentInterviews.Add(new TestStudentLoanSummaryRepository.InterviewRecord()
                {
                    Id = "RAB",
                    CompleteDate = notSoFutureDate,
                    InterviewCode = "PLUS"
                });
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.AreEqual(futureDate, actualLoanSummary.DirectLoanEntranceInterviewDate);
                Assert.AreEqual(futureDate, actualLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoStudentInterviews_NsldsDirectLoanInterviewAssignedTest()
            {
                expectedRepository.StudentInterviews = new List<TestStudentLoanSummaryRepository.InterviewRecord>();
                expectedRepository.FaSysParams.FspUseFaInterviewsOnly = "N";
                actualLoanSummary = await actualRepository.GetStudentLoanSummaryAsync(studentId);
                var expectedDate = expectedRepository.isirNsldsLoanData.First().currentStatusDate;
                Assert.AreEqual(expectedDate, actualLoanSummary.DirectLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoMatchingNsldsRecord_NoDirectLoanInterviewAssignedTest()
            {
                expectedRepository.StudentInterviews = new List<TestStudentLoanSummaryRepository.InterviewRecord>();
                expectedRepository.isirNsldsLoanData.ForEach(d => d.programCode = "FOO");
                actualLoanSummary = await actualRepository.GetStudentLoanSummaryAsync(studentId);
                
                Assert.IsNull(actualLoanSummary.DirectLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoStudentInterviews_NsldsPlusLoanInterviewAssignedTest()
            {
                expectedRepository.StudentInterviews = new List<TestStudentLoanSummaryRepository.InterviewRecord>();
                expectedRepository.FaSysParams.FspUseFaInterviewsOnly = "N";
                actualLoanSummary = await actualRepository.GetStudentLoanSummaryAsync(studentId);
                var expectedDate = expectedRepository.isirNsldsLoanData.First(ld => ld.programCode == "D3").currentStatusDate;
                Assert.AreEqual(expectedDate, actualLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoMatchingNsldsRecord_NoPlusLoanInterviewAssignedTest()
            {
                expectedRepository.StudentInterviews = new List<TestStudentLoanSummaryRepository.InterviewRecord>();
                expectedRepository.isirNsldsLoanData.ForEach(d => d.programCode = "FOO");
                actualLoanSummary = await actualRepository.GetStudentLoanSummaryAsync(studentId);

                Assert.IsNull(actualLoanSummary.GraduatePlusLoanEntranceInterviewDate);
            }

            [TestMethod]
            public async Task NoCodPersonId_NoMpnDatesAssignedTest()
            {
                expectedRepository.FaStudent.CodPersonId = null;
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task NoCodMpnPointersInCodPerson_NoMpnDatesAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(m => m.Id = string.Empty);
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task NoCodMpnRecords_NoMpnDatesAssignedTest()
            {
                expectedRepository.StudentMpns = new List<TestStudentLoanSummaryRepository.MpnRecord>();
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task NoSubMpnTypeCodes_NoDirectMpnDateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(m => m.Id = "123456789N13G99999001");
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNull(actualLoanSummary.DirectLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task NoPlusMpnTypeCodes_NoPlusMpnDateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(m => m.Id = "123456789M13G99999001");
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task CorruptMpnRecordIds_NoMpnDatesAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(m => m.Id = "FOOBAR");
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task InactiveMpns_NoMpnDatesAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(m => m.Status = "FOOBAR");
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task NoMpnRecordsWithExpirationDate_NoMpnDatesAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(m => m.ExpirationDate = null);
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task UseMostRecentlyUpdatedMpn_ExpectedMpnDatesAssignedTest()
            {
                var orderedSubMpnRecords = expectedRepository.StudentMpns.Where(
                    m => m.Id.Substring(9, 1) == "M" &&
                    m.Status == "A" &&
                    m.ExpirationDate.HasValue).OrderByDescending(m => m.ExpirationDate);
                var expectedMostRecentSubMpnDate = orderedSubMpnRecords.First().ExpirationDate;

                var orderedPlusMpnRecords = expectedRepository.StudentMpns.Where(
                    m => m.Id.Substring(9, 1) == "N" &&
                    m.Status == "A" &&
                    m.ExpirationDate.HasValue).OrderByDescending(m => m.ExpirationDate);
                var expectedMostRecentPlusMpnDate = orderedPlusMpnRecords.First().ExpirationDate;
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.AreEqual(expectedMostRecentSubMpnDate, actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.AreEqual(expectedMostRecentPlusMpnDate, actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task NoActiveTypeSpecificCodeMpn_ExpectedPendingMpnDateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(mpn => mpn.Status = "R");
                expectedRepository.StudentMpns.AddRange(new List<TestStudentLoanSummaryRepository.MpnRecord>(){
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "P", ExpirationDate = new DateTime(2020, 12, 23), Id = "111111111M12G99999001"}, 
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "P", ExpirationDate = new DateTime(2021, 12, 23), Id = "222222222M12G99999001"},
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "P", ExpirationDate = new DateTime(2020, 12, 23), Id = "111111111N12G99999001"}
                });
                var orderedSubMpnRecords = expectedRepository.StudentMpns.Where(
                    m => m.Id.Substring(9, 1) == "M" &&
                    m.Status == "P" &&
                    m.ExpirationDate.HasValue).OrderByDescending(m => m.ExpirationDate);
                var expectedSubMpnDate = orderedSubMpnRecords.First().ExpirationDate;

                var orderedPlusMpnRecords = expectedRepository.StudentMpns.Where(
                    m => m.Id.Substring(9, 1) == "N" &&
                    m.Status == "P" &&
                    m.ExpirationDate.HasValue).OrderByDescending(m => m.ExpirationDate);
                var expectedPlusMpnDate = orderedPlusMpnRecords.First().ExpirationDate;
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.AreEqual(expectedSubMpnDate, actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.AreEqual(expectedPlusMpnDate, actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task ActiveTypeSpecificCodeMpnWithExpiredDate_NoMpnDateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(mpn => mpn.Status = "R");
                expectedRepository.StudentMpns.AddRange(new List<TestStudentLoanSummaryRepository.MpnRecord>(){
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "A", ExpirationDate = new DateTime(2010, 12, 23), Id = "111111111M12G99999001"},                    
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "A", ExpirationDate = new DateTime(2010, 12, 23), Id = "111111111N12G99999001"}
                });
                
                
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task ActiveTypeSpecificCodeMpnWithTodaysDate_DateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(mpn => mpn.Status = "R");
                expectedRepository.StudentMpns.AddRange(new List<TestStudentLoanSummaryRepository.MpnRecord>(){
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "A", ExpirationDate = DateTime.Today, Id = "111111111M12G99999001"},                    
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "A", ExpirationDate = DateTime.Today, Id = "111111111N12G99999001"}
                });                
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNotNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNotNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            [Ignore]
            public async Task NoPendingTypeSpecificCodeMpn_ExpectedOldMpnDateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(mpn => mpn.Status = "R");
                expectedRepository.StudentMpns.AddRange(new List<TestStudentLoanSummaryRepository.MpnRecord>(){
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "X", ExpirationDate = new DateTime(2010, 12, 23), Id = "111111111M12G99999001"}, 
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "X", ExpirationDate = new DateTime(DateTime.Today.Year, 12, 23), Id = "222222222M12G99999001"},
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "X", ExpirationDate = new DateTime(2030, 12, 23), Id = "111111111N12G99999001"},
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "X", ExpirationDate = new DateTime(2040, 12, 23), Id = "111111111N12G99999001"},
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "X", ExpirationDate = new DateTime(2050, 12, 23), Id = "111111111N12G99999001"}
                });
                var orderedSubMpnRecords = expectedRepository.StudentMpns.Where(
                    m => m.Id.Substring(9, 1) == "M" &&
                    m.Status == "X" &&
                    m.ExpirationDate.HasValue).OrderByDescending(m => m.ExpirationDate);
                var expectedSubMpnDate = orderedSubMpnRecords.First().ExpirationDate;

                var orderedPlusMpnRecords = expectedRepository.StudentMpns.Where(
                    m => m.Id.Substring(9, 1) == "N" &&
                    m.Status == "X" &&
                    m.ExpirationDate.HasValue).OrderByDescending(m => m.ExpirationDate);
                var expectedPlusMpnDate = orderedPlusMpnRecords.First().ExpirationDate;
                actualLoanSummary = await GetLoanSummaryAsync();

                //Assert.AreEqual(expectedSubMpnDate, actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.AreEqual(expectedPlusMpnDate, actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task PendingTypeSpecificCodeMpnWithExpiredDate_NoMpnDateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(mpn => mpn.Status = "R");
                expectedRepository.StudentMpns.AddRange(new List<TestStudentLoanSummaryRepository.MpnRecord>(){
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "P", ExpirationDate = new DateTime(2010, 12, 23), Id = "111111111M12G99999001"},                    
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "P", ExpirationDate = new DateTime(2010, 12, 23), Id = "111111111N12G99999001"}
                });

                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task PendingTypeSpecificCodeMpnWithTodaysDate_DateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(mpn => mpn.Status = "R");
                expectedRepository.StudentMpns.AddRange(new List<TestStudentLoanSummaryRepository.MpnRecord>(){
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "P", ExpirationDate = DateTime.Today, Id = "111111111M12G99999001"},                    
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "P", ExpirationDate = DateTime.Today, Id = "111111111N12G99999001"}
                });
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNotNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNotNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task OldTypeSpecificCodeMpnWithExpiredDate_NoMpnDateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(mpn => mpn.Status = "R");
                expectedRepository.StudentMpns.AddRange(new List<TestStudentLoanSummaryRepository.MpnRecord>(){
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "X", ExpirationDate = new DateTime(2010, 12, 23), Id = "111111111M12G99999001"},                    
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "X", ExpirationDate = new DateTime(2010, 12, 23), Id = "111111111N12G99999001"}
                });

                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task OldTypeSpecificCodeMpnWithTodaysDate_DateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(mpn => mpn.Status = "R");
                expectedRepository.StudentMpns.AddRange(new List<TestStudentLoanSummaryRepository.MpnRecord>(){
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "X", ExpirationDate = DateTime.Today, Id = "111111111M12G99999001"},                    
                    new TestStudentLoanSummaryRepository.MpnRecord(){Status = "X", ExpirationDate = DateTime.Today, Id = "111111111N12G99999001"}
                });
                actualLoanSummary = await GetLoanSummaryAsync();

                Assert.IsNotNull(actualLoanSummary.DirectLoanMpnExpirationDate);
                Assert.IsNotNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task PlusMpnTypeCodes_EndorserPlusMpnDateAssignedTest()
            {
                expectedRepository.StudentMpns.ForEach(m => m.Id = "123456789N14G99999001");
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsNotNull(actualLoanSummary.PlusLoanMpnExpirationDate);
            }

            [TestMethod]
            public async Task LoanHistoryObjectsAreEqualTest()
            {
                actualLoanSummary = await GetLoanSummaryAsync();
                CollectionAssert.AreEqual(expectedLoanSummary.StudentLoanHistory, actualLoanSummary.StudentLoanHistory);
            }

            [TestMethod]
            public async Task NoFaIsirNsldsIds_NoStudentLoanHistoryObjectsCreatedTest()
            {
                expectedRepository.FaStudent.FaIsirNsldsIds = new List<string>();
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.AreEqual(0, actualLoanSummary.StudentLoanHistory.Count());
            }

            [TestMethod]
            public async Task NullIsirNsldsRecord_NoStudentLoanHistoryObjectsCreatedTest()
            {
                expectedRepository.isirNsldsData = new List<TestStudentLoanSummaryRepository.IsirNsldsRecord>();
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.AreEqual(0, actualLoanSummary.StudentLoanHistory.Count());
            }

            [TestMethod]
            public async Task FirstIsirNsldsRecord_ExpectedStudentLoanHistoryRecordCreatedTest()
            {
                var schoolCode = "foo";
                var loanAmount = 59876;
                expectedRepository.FaStudent.FaIsirNsldsIds = new List<string>() { "foobar", "2" };
                expectedRepository.isirNsldsData = new List<TestStudentLoanSummaryRepository.IsirNsldsRecord>()
                {
                    new TestStudentLoanSummaryRepository.IsirNsldsRecord() {id = "foobar", IsirNsldsLoanIds = new List<string>() {"foobar"}}
                };
                expectedRepository.isirNsldsLoanData = new List<TestStudentLoanSummaryRepository.IsirNsldsLoanRecord>()
                {
                    new TestStudentLoanSummaryRepository.IsirNsldsLoanRecord() {id = "foobar", schoolCode = schoolCode, aggregatePrincipalBalance = loanAmount}
                };
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.AreEqual(1, actualLoanSummary.StudentLoanHistory.Count());
                Assert.AreEqual(schoolCode, actualLoanSummary.StudentLoanHistory[0].OpeId);
                Assert.AreEqual(loanAmount, actualLoanSummary.StudentLoanHistory[0].TotalLoanAmount);
            }

            [TestMethod]
            public async Task NoLoanIds_NoStudentLoanHistoryTest()
            {
                expectedRepository.isirNsldsData.ForEach(d => d.IsirNsldsLoanIds = new List<string>());
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.AreEqual(0, actualLoanSummary.StudentLoanHistory.Count());
            }

            [TestMethod]
            public async Task NoLoanRecords_NoStudentLoanHistoryTest()
            {
                expectedRepository.isirNsldsLoanData = new List<TestStudentLoanSummaryRepository.IsirNsldsLoanRecord>();
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.AreEqual(0, actualLoanSummary.StudentLoanHistory.Count());
            }

            [TestMethod]
            public async Task EmptySchoolCodeDoesNotCreateLoanHistoryTest()
            {
                expectedRepository.isirNsldsLoanData.ForEach(d => d.schoolCode = string.Empty);
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.AreEqual(0, actualLoanSummary.StudentLoanHistory.Count());
            }

            [TestMethod]
            public async Task NullAggregatePrincipalBalanceDoesNotCreateLoanHistoryTest()
            {
                expectedRepository.isirNsldsLoanData.ForEach(d => d.aggregatePrincipalBalance = null);
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.AreEqual(0, actualLoanSummary.StudentLoanHistory.Count());
            }

            [TestMethod]
            public async Task NullIsirNsldsRecord_StudentLoanCombinedTotalAmountEqualsZeroTest()
            {
                expectedRepository.isirNsldsData = new List<TestStudentLoanSummaryRepository.IsirNsldsRecord>();
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsTrue(actualLoanSummary.StudentLoanCombinedTotalAmount == 0);
            }

            [TestMethod]
            public async Task StudentLoanCombinedTotalAmount_EqualsExpectedAmountTest()
            {
                var isirId = expectedRepository.FaStudent.FaIsirNsldsIds.First();
                var expectedAmount = expectedRepository.isirNsldsData.First(d => d.id == isirId).combinedPrinBal;
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.AreEqual(expectedAmount, actualLoanSummary.StudentLoanCombinedTotalAmount);
            }

            [TestMethod]
            public async Task StudentLoanCombinedTotalAmount_EqualsZeroIfDBFieldIsNull()
            {
                var isirId = expectedRepository.FaStudent.FaIsirNsldsIds.First();
                expectedRepository.isirNsldsData.First(d => d.id == isirId).combinedPrinBal = null;
                actualLoanSummary = await GetLoanSummaryAsync();
                Assert.IsTrue(actualLoanSummary.StudentLoanCombinedTotalAmount == 0);
            }

            [TestMethod]
            public async Task NullReturnedFromReadingFaInterviews_NoExceptionThrownTest()
            {
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<FaInterview>(It.IsAny<string[]>(), true))
                .ReturnsAsync(null);
                bool exceptionThrown = false;
                try
                {
                    await actualRepository.GetStudentLoanSummaryAsync(studentId);
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            [TestMethod]
            public async Task NullReturnedFromReadingIsirNsldsRecords_NoExceptionThrownTest()
            {
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirNslds>(It.IsAny<string[]>(), true))
                .ReturnsAsync(null);
                bool exceptionThrown = false;
                try
                {
                    await actualRepository.GetStudentLoanSummaryAsync(studentId);
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            [TestMethod]
            public async Task NullReturnedFromReadingIsirNsldsLoanRecords_NoExceptionThrownTest()
            {
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirNsldsLoan>(It.IsAny<string[]>(), true))
                .ReturnsAsync(null);
                bool exceptionThrown = false;
                try
                {
                    await actualRepository.GetStudentLoanSummaryAsync(studentId);
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            [TestMethod]
            public async Task NullReturnedFromReadingCodMpnRecords_NoExceptionThrownTest()
            {
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CodMpn>(It.IsAny<string[]>(), true))
                .ReturnsAsync(null);
                bool exceptionThrown = false;
                try
                {
                    await actualRepository.GetStudentLoanSummaryAsync(studentId);
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

        }
    }
}
