//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
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
    public class StudentFinancialAidNeedSummaryRepositoryTests //: BaseRepositorySetup
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

        StudentFinancialAidNeedSummaryRepository fundRepo;
        IEnumerable<StudentNeedSummary> allStudentFinancialAidNeedSummaries;
        
        StudentFinancialAidNeedSummaryRepository repositoryUnderTest;
        public TestStudentFinancialAidNeedSummaryRepository testDataRepository;

        [TestInitialize]
        public void MockInitialize()
        {
            loggerMock = new Mock<ILogger>();
            apiSettings = new ApiSettings("TEST");

            allStudentFinancialAidNeedSummaries = new TestStudentFinancialAidNeedSummaryRepository().GetStudentFinancialAidNeedSummaries();
          
            testDataRepository = new TestStudentFinancialAidNeedSummaryRepository();

            fundRepo = BuildValidFundRepository();
            valcodeName = fundRepo.BuildFullCacheKey("AllStudentFinancialAidNeedSummaries");
        }

        [TestCleanup]
        public void Cleanup()
        {
            allStudentFinancialAidNeedSummaries = null;
            valcodeName = string.Empty;
            apiSettings = null;
        }
        #endregion

        #region MOCK EVENTS
        private StudentFinancialAidNeedSummaryRepository BuildValidFundRepository()
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


            //transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);



            var csRecords = new Collection<DataContracts.CsAcyr>();
            foreach (var item in allStudentFinancialAidNeedSummaries)
            {
                DataContracts.CsAcyr csRecord = new DataContracts.CsAcyr();
                csRecord.RecordGuid = item.Guid;
                csRecord.Recordkey = item.StudentId;
                csRecord.CsFedIsirId = "FED1";
                csRecord.CsInstIsirId = "INST1";
                csRecord.CsFc = "3";
                csRecord.CsNeed = 1;
                csRecord.CsInstNeed = 2;
              
                csRecords.Add(csRecord);
            }

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.CsAcyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(csRecords);
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.CsAcyr>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(csRecords.FirstOrDefault());

            var isirRecords = new Collection<DataContracts.IsirFafsa>();
            DataContracts.IsirFafsa isirRecord = new DataContracts.IsirFafsa();
            isirRecord.RecordGuid = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4";
            isirRecord.Recordkey = "ISIR";
            isirRecords.Add(isirRecord);

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.IsirFafsa>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(isirRecords);
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.IsirFafsa>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(isirRecord);

            var saRecords = new Collection<DataContracts.SaAcyr>();
            DataContracts.SaAcyr saRecord = new DataContracts.SaAcyr();
            saRecord.SaAward = new List<string>() { "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4" };
            saRecord.Recordkey = "SA*ACYR";
            saRecord.SaTerms = new List<string>() { "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4" };
            saRecords.Add(saRecord);

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.SaAcyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(saRecords);
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.SaAcyr>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(saRecord);

            var taRecords = new Collection<DataContracts.TaAcyr>();
            DataContracts.TaAcyr taRecord = new DataContracts.TaAcyr();
            //taRecord.RecordGuid = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4";
            taRecord.Recordkey = "CS*AWARD";
            taRecord.TaTermAction = "ACT";
            taRecord.TaTermAmount = (decimal)2000;
            taRecords.Add(taRecord);

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.TaAcyr>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(taRecords);
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.TaAcyr>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(taRecord);

            var awardRecords = new Collection<DataContracts.Awards>();
            DataContracts.Awards awardRecord = new DataContracts.Awards();
            awardRecord.RecordGuid = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4";
            awardRecord.AwDescription = "DESC";
            awardRecord.Recordkey = "AWARD";
            awardRecord.AwExplanationText = "DESC2";
            awardRecord.AwType = "TYPE";
            awardRecord.AwCategory = "CATEGORY";
            awardRecord.AwReportingFundingType = "FUNDTYPE";
            awardRecord.AwNeedCost = "F";
            awardRecords.Add(awardRecord);

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Awards>("AWARDS", It.IsAny<string[]>(), true)).ReturnsAsync(awardRecords);
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Awards>("AWARDS", It.IsAny<string>(), true)).ReturnsAsync(awardRecord);

            var faSysRecords = new Collection<DataContracts.FaSysParams>();
            DataContracts.FaSysParams faSysRecord = new DataContracts.FaSysParams();
            faSysRecord.Recordkey = "FASYS*PARAMS";
            faSysRecord.FspNotAwardedCat = new List<string>() { "NAC1" };
            faSysRecords.Add(faSysRecord);

            dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.FaSysParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync((DataContracts.FaSysParams)faSysRecord);


            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "CSACYRID1" });

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var rel = csRecords.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, rel == null ? null : new GuidLookupResult() { Entity = "CS.2006", PrimaryKey = rel.Recordkey });
                }
                return Task.FromResult(result);
            });

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            // Construct repository
            fundRepo = new StudentFinancialAidNeedSummaryRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

           
            var csRecordList = csRecords.Select(x => string.Concat("CS.2006." , x.Recordkey)).ToList();
            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 100,
                CacheName = "AllStudentFinancialAidNeedSummary:",
                Entity = "",
                Sublist = csRecordList, 
                TotalCount = csRecordList.Count(),
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


            return fundRepo;
        }


        #endregion

        #region GetStudentFinancialAidNeedSummariesTests
        [TestClass]
        public class GetStudentFinancialAidNeedSummariesTests : StudentFinancialAidNeedSummaryRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
            }

            #region TESTS FOR FUNCTIONALITY
       
            [TestMethod]
            public async Task StudentFinancialAidNeedSummaryRepository_GetStudentFinancialAidNeedSummariesAsync()
            {
                var results = await fundRepo.GetAsync(0, 100, false, new List<string>() { "2006", "2007", "2008", "2009", "2010", "2011", "2012", "2013", "2014", "2015", "2016" });
                Assert.AreEqual(allStudentFinancialAidNeedSummaries.Count(), results.Item2);

                foreach (var financialAidNeed in allStudentFinancialAidNeedSummaries)
                {
                    var result = results.Item1.FirstOrDefault(i => i.Guid == financialAidNeed.Guid);

                    //Assert.AreEqual(financialAidNeed.Code, result.Code);
                    //Assert.AreEqual(financialAidNeed.Description, result.Description);
                    Assert.AreEqual(financialAidNeed.Guid, result.Guid);
                    Assert.AreEqual(financialAidNeed.StudentId, result.StudentId);
                }

            }

            [TestMethod]
            public async Task StudentFinancialAidNeedSummaryRepository_GetStudentFinancialAidNeedSummaryByIdAsync()
            {
                var result = await fundRepo.GetByIdAsync("31d8aa32-dbe6-4a49-a1c4-2cad39e232e4");
                var financialAidNeed = allStudentFinancialAidNeedSummaries.Where(a => a.Guid == "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4").FirstOrDefault();

                Assert.AreEqual(financialAidNeed.Guid, result.Guid);
                Assert.AreEqual(financialAidNeed.StudentId, result.StudentId);

            }

            #endregion

        }
        #endregion

    }
}