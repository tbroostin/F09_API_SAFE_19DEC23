//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentFinancialAidAwardRepositoryTests
    {
        #region SETUP
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<IColleagueTransactionInvoker> transManagerMock;
        IColleagueTransactionInvoker transManager;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataReaderMock;
        Mock<ILogger> loggerMock;

        string valcodeName;
        ApiSettings apiSettings;

        StudentFinancialAidAwardRepository awardRepo;
        IEnumerable<StudentFinancialAidAward> allFinancialAidAwards;
        IEnumerable<StudentAwardHistoryByPeriod> allStudentAwardHistoryByPeriods;
        IEnumerable<StudentAwardHistoryStatus> allStudentAwardHistoryStatuses;        
        Collection<TcAcyr> tcAcyrDataContracts;

        public string awardId;
        public string[] studentFinancialAidAwardIds;
        StudentFinancialAidAwardRepository repositoryUnderTest;
        public TestStudentFinancialAidAwardRepository testDataRepository;

        public async Task<IEnumerable<StudentFinancialAidAward>> getExpectedStudentFinancialAidAwards()
        {
            return await testDataRepository.GetStudentFinancialAidAwardsAsync(false);
        }
        
        public async Task<Tuple<IEnumerable<StudentFinancialAidAward>, int>> getActualStudentFinancialAidAwards()
        {
            return await awardRepo.GetAsync(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" } );
        }        

        public async Task<StudentFinancialAidAward> getActualStudentFinancialAidAwardsById(string id)
        {
            return await awardRepo.GetByIdAsync(id);
        }

        [TestInitialize]
        public void MockInitialize()
        {
            loggerMock = new Mock<ILogger>();
            apiSettings = new ApiSettings("TEST");

         

            allFinancialAidAwards = new TestStudentFinancialAidAwardRepository().GetStudentFinancialAidAwardsAsync(false).Result;
            allStudentAwardHistoryByPeriods = new TestStudentFinancialAidAwardRepository().GetStudentAwardHistoryByPeriodsAsync().Result;
            allStudentAwardHistoryStatuses = new TestStudentFinancialAidAwardRepository().GetStudentAwardHistoryStatusesAsync().Result;
            

            studentFinancialAidAwardIds = new string[2];
            studentFinancialAidAwardIds[0] = "5*FUND1";
            studentFinancialAidAwardIds[1] = "6*FUND2";

            tcAcyrDataContracts = new Collection<TcAcyr>() 
                {
                    new TcAcyr()
                    {
                        Recordkey = "1", 
                        RecordGuid = "bb66b971-3ee0-4477-9bb7-539721f93434", 
                    },
                    new TcAcyr()
                    {
                        Recordkey = "2", 
                        RecordGuid = "5aeebc5c-c973-4f83-be4b-f64c95002124", 
                    },
                    new TcAcyr()
                    {
                        Recordkey = "3", 
                        RecordGuid = "27178aab-a6e8-4d1e-ae27-eca1f7b33363", 
                    },
                };

            //fundYears = new List<string>() {"2008"};
            testDataRepository = new TestStudentFinancialAidAwardRepository();

        
            awardRepo = BuildValidFundRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            allFinancialAidAwards = null;
            valcodeName = string.Empty;
            apiSettings = null;
        }
        #endregion

        #region MOCK EVENTS
        private StudentFinancialAidAwardRepository BuildValidFundRepository()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            transManagerMock = new Mock<IColleagueTransactionInvoker>();
            transManager = transManagerMock.Object;

            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();

            // Set up data accessor for mocking 
            dataReaderMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                       
            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentFinancialAidAwardIds);

            var ids = new List<string>();
            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 1,
                CacheName = "AllStudentAcademicPrograms:",
                Entity = "STUDENT.PROGRAMSs",
                Sublist = ids.ToList(),
                TotalCount = ids.ToList().Count,
                KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
            };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                .ReturnsAsync(resp);

            var tcAcyrRecords = new Collection<DataContracts.TcAcyr>();
            foreach (var item in allFinancialAidAwards)
            {
                DataContracts.TcAcyr record = new DataContracts.TcAcyr();
                record.Recordkey = item.StudentId + "*" + item.AwardFundId;
                record.TcTaTerms = new List<string>() { item.StudentId + "*" + item.AwardFundId };
                record.RecordGuid = allFinancialAidAwards.Where(f => f.StudentId == item.StudentId).FirstOrDefault().Guid;
                tcAcyrRecords.Add(record);
            }

            var taAcyrRecords = new Collection<DataContracts.TaAcyr>();
            foreach (var item in allStudentAwardHistoryByPeriods)
            {
                DataContracts.TaAcyr record = new DataContracts.TaAcyr();
                record.Recordkey = allFinancialAidAwards.FirstOrDefault().StudentId + "*" + allFinancialAidAwards.FirstOrDefault().AwardFundId + "*" + allFinancialAidAwards.FirstOrDefault().AidYearId;
                record.TaTermAction = item.Status;
                record.TaTermActionDate = item.StatusDate;
                record.TaTermAmount = item.Amount;
                record.TaTermXmitAmt = item.XmitAmount;
                taAcyrRecords.Add(record);
            }

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<TcAcyr>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string, string[], bool>((x, y, z) =>
                    Task.FromResult(new Collection<TcAcyr>(allFinancialAidAwards.Select(a => 
                        new TcAcyr() 
                        {
                            Recordkey = a.StudentId + "*" + a.AwardFundId,
                            TcTaTerms = new List<string>() { a.StudentId + "*" + a.AwardFundId + "*" + allFinancialAidAwards.FirstOrDefault().AidYearId },
                            RecordGuid = allFinancialAidAwards.Where(f => f.StudentId == a.StudentId).FirstOrDefault().Guid,
                        }).ToList())));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<TaAcyr>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string, string[], bool>((x, y, z) =>
                    Task.FromResult(new Collection<TaAcyr>(allStudentAwardHistoryByPeriods.Select(a =>
                        new TaAcyr()
                        {
                            Recordkey = allFinancialAidAwards.FirstOrDefault().StudentId + "*" + allFinancialAidAwards.FirstOrDefault().AwardFundId + "*" + allFinancialAidAwards.FirstOrDefault().AidYearId,
                            TaTermAction = a.Status,
                            TaTermActionDate = a.StatusDate,
                            TaTermAmount = a.Amount,
                            TaTermXmitAmt = a.XmitAmount
                        }).ToList())));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<FaAwardHistory>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((x, y) =>
                    Task.FromResult(new Collection<FaAwardHistory>(allStudentAwardHistoryByPeriods.Select(a =>
                        new FaAwardHistory()
                        {
                            Recordkey = allFinancialAidAwards.FirstOrDefault().StudentId + "*" + allFinancialAidAwards.FirstOrDefault().AwardFundId + "*" + allFinancialAidAwards.FirstOrDefault().AidYearId,
                            FawhCrntTermAction = a.Status,
                            FaAwardHistoryChgdate = a.StatusDate,
                            FawhChgTime = a.StatusDate,
                            FawhCrntTermAmt = a.Amount,
                        }).ToList())));

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var rel = tcAcyrDataContracts.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, rel == null ? null : new GuidLookupResult() { Entity = "TC.ACYR", PrimaryKey = rel.Recordkey });
                }
                return Task.FromResult(result);
            });

            dataReaderMock.Setup(d => d.ReadRecordAsync<TcAcyr>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((x, y, z) => Task.FromResult(tcAcyrRecords.FirstOrDefault()));

            // Construct repository
            awardRepo = new StudentFinancialAidAwardRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return awardRepo;
        }


        #endregion

       
        [TestClass]
        public class GetStudentFinancialAidAwardsTests : StudentFinancialAidAwardRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
            }

            #region TESTS FOR FUNCTIONALITY
            [TestMethod]
            public async Task ExpectedEqualsActualStudentFinancialAidAwardsTest()
            {
                var expected = await getExpectedStudentFinancialAidAwards();
                var actual = await getActualStudentFinancialAidAwards();
                Assert.AreEqual(expected.Count(), actual.Item2);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualStudentFinancialAidAwardsByIdTest()
            {
                var expected = await getExpectedStudentFinancialAidAwards();
                var actual = await getActualStudentFinancialAidAwardsById(expected.FirstOrDefault().Guid);
                Assert.AreEqual(expected.FirstOrDefault().Guid, actual.Guid);
            }

            [TestMethod]
            public async Task StudentFinancialAidAwardRepository_GetStudentFinancialAidAwardsAsync_False()
            {
                var results = await getActualStudentFinancialAidAwards();
                Assert.AreEqual(allFinancialAidAwards.Count(), results.Item2);

                foreach (var financialAidAward in allFinancialAidAwards)
                {
                    var result = results.Item1.FirstOrDefault(i => i.Guid == financialAidAward.Guid);
                    Assert.AreEqual(financialAidAward.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards_WithStudentId()
            {
                StudentFinancialAidAward award = new StudentFinancialAidAward("studentId");
                var actual = await awardRepo.GetAsync(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" }, award);
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards_WithStudentId_Result()
            {
                FinAid fa = new FinAid();
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fa);
                StudentFinancialAidAward award = new StudentFinancialAidAward("studentId");
                var actual = await awardRepo.GetAsync(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" }, 
                    award);

                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards_WithYears()
            {
                FinAid fa = new FinAid() { FaSaYears = new List<string>() { "2012", "2013", "2014" } };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fa);
                StudentFinancialAidAward award = new StudentFinancialAidAward("studentId", "", "2012");
                var actual = await awardRepo.GetAsync(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
                //Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards_WithNoYearMatch()
            {
                FinAid fa = new FinAid() { FaSaYears = new List<string>() { "2012", "2013", "2014" } };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fa);
                StudentFinancialAidAward award = new StudentFinancialAidAward("studentId", "", "2010");
                var actual = await awardRepo.GetAsync(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards_WithJustAidYear()
            {
                //FinAid fa = new FinAid() { FaSaYears = new List<string>() { "2012", "2013", "2014" } };
                dataReaderMock.Setup(repo => repo.SelectAsync("FIN.AID", It.IsAny<string>())).ReturnsAsync(new string[] { });
                StudentFinancialAidAward award = new StudentFinancialAidAward("", "", "2010");
                var actual = await awardRepo.GetAsync(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards_WithAidYearAndAwardFund()
            {
                FinAid fa = new FinAid() { FaSaYears = new List<string>() { "2012", "2013", "2014" } };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fa);
                StudentFinancialAidAward award = new StudentFinancialAidAward("studentId", "Fund1", "2012");
                var actual = await awardRepo.GetAsync(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards_WithAwardFundNoStudentId()
            {
                FinAid fa = new FinAid() { FaSaYears = new List<string>() { "2012", "2013", "2014" } };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fa);
                StudentFinancialAidAward award = new StudentFinancialAidAward("", "Fund1", "2012");
                var actual = await awardRepo.GetAsync(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
            }

            #endregion
        }


        [TestClass]
        public class GetStudentFinancialAidAwardsTests2 : StudentFinancialAidAwardRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

              

            }

            #region TESTS FOR FUNCTIONALITY
           
     
            [TestMethod]
            public async Task getActualStudentFinancialAidAwards2_WithStudentId()
            {
                StudentFinancialAidAward award = new StudentFinancialAidAward("studentId");
                var actual = await awardRepo.Get2Async(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" }, award);
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards2_WithStudentId_Result()
            {
                FinAid fa = new FinAid();
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fa);
                StudentFinancialAidAward award = new StudentFinancialAidAward("studentId");
                var actual = await awardRepo.Get2Async(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards2_WithYears()
            {
                FinAid fa = new FinAid() { FaSaYears = new List<string>() { "2012", "2013", "2014" } };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fa);
                StudentFinancialAidAward award = new StudentFinancialAidAward("studentId", "", "2012");
                var actual = await awardRepo.Get2Async(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
                //Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards2_WithNoYearMatch()
            {
                FinAid fa = new FinAid() { FaSaYears = new List<string>() { "2012", "2013", "2014" } };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fa);
                StudentFinancialAidAward award = new StudentFinancialAidAward("studentId", "", "2010");
                var actual = await awardRepo.Get2Async(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards2_WithJustAidYear()
            {
                //FinAid fa = new FinAid() { FaSaYears = new List<string>() { "2012", "2013", "2014" } };
                dataReaderMock.Setup(repo => repo.SelectAsync("FIN.AID", It.IsAny<string>())).ReturnsAsync(new string[] { });
                StudentFinancialAidAward award = new StudentFinancialAidAward("", "", "2010");
                var actual = await awardRepo.Get2Async(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards2_WithAidYearAndAwardFund()
            {
                FinAid fa = new FinAid() { FaSaYears = new List<string>() { "2012", "2013", "2014" } };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fa);
                StudentFinancialAidAward award = new StudentFinancialAidAward("studentId", "Fund1", "2012");
                var actual = await awardRepo.Get2Async(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
            }

            [TestMethod]
            public async Task getActualStudentFinancialAidAwards2_WithAwardFundNoStudentId()
            {
                FinAid fa = new FinAid() { FaSaYears = new List<string>() { "2012", "2013", "2014" } };
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<FinAid>("FIN.AID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fa);
                StudentFinancialAidAward award = new StudentFinancialAidAward("", "Fund1", "2012");
                var actual = await awardRepo.Get2Async(0, 10, false, false, new List<string>() { "FUND1", "FUND2" }, new List<string>() { "YEAR1", "YEAR2" },
                    award);

                Assert.IsNotNull(actual);
            }

           
        }

        #endregion

    }
}