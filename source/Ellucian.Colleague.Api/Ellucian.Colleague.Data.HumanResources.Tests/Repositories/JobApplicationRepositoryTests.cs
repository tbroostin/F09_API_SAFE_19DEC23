//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Transactions;
using System.Threading;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class JobApplicationRepositoryTests_V10
    {
        [TestClass]
        public class JobApplicationTests : BaseRepositorySetup
        {

            Mock<ICacheProvider> iCacheProviderMock;
            Mock<IColleagueTransactionFactory> iColleagueTransactionFactoryMock;
            Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;
            Collection<DataContracts.Jobapps> _jobAppsCollection;
            List<Domain.HumanResources.Entities.JobApplication> jobApplicationsEntities;

            JobApplicationsRepository jobAppsRepo;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                iCacheProviderMock = new Mock<ICacheProvider>();
                iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(iColleagueTransactionInvokerMock.Object);

                string id, guid, id2, guid2, id3, guid3, sid, sid2, sid3;
                GuidLookup guidLookup;
                GuidLookupResult guidLookupResult;
                Dictionary<string, GuidLookupResult> guidLookupDict;
                RecordKeyLookup recordLookup;
                RecordKeyLookupResult recordLookupResult;
                Dictionary<string, RecordKeyLookupResult> recordLookupDict;

                // Set up for GUID lookups
                id = "1";
                id2 = "2";
                id3 = "3";

                // Secondary keys for GUID lookups
                sid = "11";
                sid2 = "22";
                sid3 = "33";

                guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                var offsetDate = DmiString.DateTimeToPickDate(new DateTime(2007, 02, 05));

                // Build responses used for mocking
                _jobAppsCollection = new Collection<DataContracts.Jobapps>()
                {
                    new DataContracts.Jobapps() { RecordGuid = guid, Recordkey = id, JbapPosId = new List<string>() { sid } ,
                        JbapIntgPosIdx = new List<string>() {"1" }, JbapApplicationDate = new List<DateTime?>() { DateTime.Now }, JbapMinSalary = new List<long?>() {(long?) 30.0} },
                    new DataContracts.Jobapps() { RecordGuid = guid2, Recordkey = id2, JbapPosId = new List<string>() { sid2 }  ,
                        JbapIntgPosIdx = new List<string>() {"1" }, JbapApplicationDate = new List<DateTime?>() { DateTime.Now }, JbapMinSalary = new List<long?>() {(long?) 30.0} },
                    new DataContracts.Jobapps() { RecordGuid = guid3, Recordkey = id3, JbapPosId = new List<string>() { sid3 } ,
                        JbapIntgPosIdx = new List<string>() {"1" }, JbapApplicationDate = new List<DateTime?>() { DateTime.Now }, JbapMinSalary = new List<long?>() {(long?) 30.0} },
                };

                guidLookup = new GuidLookup(guid);
                guidLookupResult = new GuidLookupResult() { Entity = "JOBAPPS", PrimaryKey = id, SecondaryKey = "1" };
                guidLookupDict = new Dictionary<string, GuidLookupResult>();
                recordLookup = new RecordKeyLookup("JOBAPPS", id, "JBAP.INTG.POS.IDX", "1", false);
                recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

                dataReaderMock.SetupSequence(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "JOBAPPS", PrimaryKey = id, SecondaryKey = "1" } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid2, new GuidLookupResult() { Entity = "JOBAPPS", PrimaryKey = id2, SecondaryKey = "1" } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid3, new GuidLookupResult() { Entity = "JOBAPPS", PrimaryKey = id3, SecondaryKey = "1" } } }));
                dataReaderMock.SetupSequence(ep => ep.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "JOBAPPS+" + id + "+" + "1", new RecordKeyLookupResult() { Guid = guid } } }))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "JOBAPPS+" + id2 + "+" + "1", new RecordKeyLookupResult() { Guid = guid2 } } }))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "JOBAPPS+" + id3 + "+" + "1", new RecordKeyLookupResult() { Guid = guid3 } } }));

                jobApplicationsEntities = new List<Domain.HumanResources.Entities.JobApplication>() 
                {
                    new Domain.HumanResources.Entities.JobApplication("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d"),
                    new Domain.HumanResources.Entities.JobApplication("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e"),
                    new Domain.HumanResources.Entities.JobApplication("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                };

                List<string> jobApplicationGuids = new List<string>();
                foreach (var mp in _jobAppsCollection)
                {
                    jobApplicationGuids.Add(mp.RecordGuid);
                };
                dataReaderMock.Setup(repo => repo.SelectAsync("JOBAPPS", "WITH JBAP.INTG.POS.IDX NE '' BY.EXP JBAP.INTG.POS.IDX")).ReturnsAsync(new List<string>() { id, id2, id3 }.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("JOBAPPS", "WITH JBAP.INTG.POS.IDX NE '' BY.EXP JBAP.INTG.POS.IDX SAVING JBAP.INTG.POS.IDX")).ReturnsAsync(new List<string>() { "1|123", "1|456", "1|789" }.ToArray());

                dataReaderMock.Setup(repo => repo.SelectAsync("JOBAPPS", It.IsAny<string>())).ReturnsAsync(new List<string>() { "1", "1", "1" }.ToArray());

                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Jobapps>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_jobAppsCollection);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Jobapps>("JOBAPPS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_jobAppsCollection);
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Jobapps>("JOBAPPS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_jobAppsCollection.FirstOrDefault());
                
                var invalidRecords = new Dictionary<string, string>();
                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.Jobapps>()
                {
                    BulkRecordsRead = _jobAppsCollection,
                    InvalidRecords = invalidRecords,
                    InvalidKeys = new string[] { }
                };
                dataReaderMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.Jobapps>("JOBAPPS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);

                // Set up transaction manager for mocking 
                var transManagerMock = new Mock<IColleagueTransactionInvoker>();

                string[] requestedIds1 = { "1|1", "2|1", "3|1" };

                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 100,
                    CacheName = "AllJobApplicationsRecordKeys",
                    Entity = "",
                    Sublist = requestedIds1.ToList(),
                    TotalCount = 3,
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

                var transManager = transManagerMock.Object;

                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);

                jobAppsRepo = BuildValidReferenceDataRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                iCacheProviderMock = null;
                iColleagueTransactionFactoryMock = null;
                iColleagueTransactionInvokerMock = null;

                transFactoryMock = null;
                cacheProviderMock = null;
                _jobAppsCollection = null;
                jobAppsRepo = null;
            }

            [TestMethod]
            public async Task JobApplicationsDataRepo_GetGetJobApplicationsAsync()
            {
                var result = await jobAppsRepo.GetJobApplicationsAsync(0, 3, false);

                for (int i = 0; i < _jobAppsCollection.Count(); i++)
                {         
                    Assert.AreEqual(_jobAppsCollection.ElementAt(i).RecordGuid, result.Item1.ElementAt(i).Guid);
                    Assert.AreEqual(_jobAppsCollection.ElementAt(i).Recordkey, result.Item1.ElementAt(i).PersonId);
                }
            }

            [TestMethod]
            public async Task JobApplicationsDataRepo_GetGetJobApplicationByIdAsync()
            {
                var result = await jobAppsRepo.GetJobApplicationByIdAsync("F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant());

                Assert.AreEqual(_jobAppsCollection.ElementAt(0).RecordGuid, result.Guid);
                Assert.AreEqual(_jobAppsCollection.ElementAt(0).Recordkey, result.PersonId);
                    
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task JobApplicationsRepo_GetsGetJobApplications_Exception()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(() => null);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(() => null);

                await jobAppsRepo.GetJobApplicationsAsync(0, 2, true);
            }

            private JobApplicationsRepository BuildValidReferenceDataRepository()
            {
                // Cache mocking
                var cacheProviderMock = new Mock<ICacheProvider>();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                jobAppsRepo = new JobApplicationsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return jobAppsRepo;
            }
        }

    }
}
