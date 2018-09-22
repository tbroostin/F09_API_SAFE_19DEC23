using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PayPeriodRepositoryTests_V12
    {
        [TestClass]
        public class PayPeriodTests : BaseRepositorySetup
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transInvokerMock;
            Mock<IColleagueDataReader> dataReaderMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            Collection<DataContracts.Paycycle> _payCyclesCollection;
            List<Domain.HumanResources.Entities.PayPeriod> payPeriodsEntities;
            Collection<DataContracts.Paycntrl> _payCntrlsCollection;
            //List<Domain.HumanResources.Entities.PayPeriod> payPeriodsEntities;
            string codeItemName;
            ApiSettings apiSettings;

            PayPeriodsRepository payPeriodsRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transInvokerMock = new Mock<IColleagueTransactionInvoker>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

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
                _payCyclesCollection = new Collection<DataContracts.Paycycle>()
                {
                    new DataContracts.Paycycle() { RecordGuid = guid, Recordkey = id, PcyPaycheckDate = new List<DateTime?>() { DateTime.Now },
                        PcyDesc = "1", PcyStartDate = new List<DateTime?>() { DateTime.Now }, PcyEndDate = new List<DateTime?>() { DateTime.Now }},
                    new DataContracts.Paycycle() { RecordGuid = guid2, Recordkey = id2, PcyPaycheckDate = new List<DateTime?>() { DateTime.Now },
                        PcyDesc = "2", PcyStartDate = new List<DateTime?>() { DateTime.Now }, PcyEndDate = new List<DateTime?>() { DateTime.Now }},
                    new DataContracts.Paycycle() { RecordGuid = guid3, Recordkey = id3, PcyPaycheckDate = new List<DateTime?>() { DateTime.Now },
                        PcyDesc = "3", PcyStartDate = new List<DateTime?>() { DateTime.Now }, PcyEndDate = new List<DateTime?>() { DateTime.Now }},
                };

                // Build responses used for mocking
                _payCntrlsCollection = new Collection<DataContracts.Paycntrl>()
                {
                    new DataContracts.Paycntrl() { Recordkey = DmiString.DateTimeToPickDate(DateTime.Now).ToString() + "*" + id, PclEmployeeCutoffDate = DateTime.Now,
                        PclEmployeeCutoffTime = DateTime.Now },
                    new DataContracts.Paycntrl() { Recordkey = DmiString.DateTimeToPickDate(DateTime.Now).ToString() + "*" + id2, PclEmployeeCutoffDate = DateTime.Now,
                       PclEmployeeCutoffTime = DateTime.Now },
                    new DataContracts.Paycntrl() { Recordkey = DmiString.DateTimeToPickDate(DateTime.Now).ToString() + "*" + id3, PclEmployeeCutoffDate = DateTime.Now,
                        PclEmployeeCutoffTime = DateTime.Now },
                };

                guidLookup = new GuidLookup(guid);
                guidLookupResult = new GuidLookupResult() { Entity = "PAYCYCLE", PrimaryKey = id, SecondaryKey = DmiString.DateTimeToPickDate(DateTime.Now).ToString() };
                guidLookupDict = new Dictionary<string, GuidLookupResult>();
                recordLookup = new RecordKeyLookup("PAYCYCLE", id, "PCY.START.DATE", DmiString.DateTimeToPickDate(DateTime.Now).ToString(), false);
                recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
               
                dataReaderMock.SetupSequence(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "PAYCYCLE", PrimaryKey = id, SecondaryKey = DmiString.DateTimeToPickDate(DateTime.Now).ToString() } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid2, new GuidLookupResult() { Entity = "PAYCYCLE", PrimaryKey = id2, SecondaryKey = DmiString.DateTimeToPickDate(DateTime.Now).ToString() } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid3, new GuidLookupResult() { Entity = "PAYCYCLE", PrimaryKey = id3, SecondaryKey = DmiString.DateTimeToPickDate(DateTime.Now).ToString() } } }));
                dataReaderMock.SetupSequence(ep => ep.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "PAYCYCLE+" + id + "+" + DmiString.DateTimeToPickDate(DateTime.Now).ToString(), new RecordKeyLookupResult() { Guid = guid } } }))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "PAYCYCLE+" + id2 + "+" + DmiString.DateTimeToPickDate(DateTime.Now).ToString(), new RecordKeyLookupResult() { Guid = guid2 } } }))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "PAYCYCLE+" + id3 + "+" + DmiString.DateTimeToPickDate(DateTime.Now).ToString(), new RecordKeyLookupResult() { Guid = guid3 } } }));
                
                payPeriodsEntities = new List<Domain.HumanResources.Entities.PayPeriod>() 
                {
                    new Domain.HumanResources.Entities.PayPeriod("ce4d68f6-257d-4052-92c8-17eed0f088fa", "DESC", DateTime.Now, DateTime.Now, DateTime.Now, "Admissions")
                    {
                        TimeEntryEndOn = DateTime.Now,
                    },
                    new Domain.HumanResources.Entities.PayPeriod("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "DESC", DateTime.Now, DateTime.Now, DateTime.Now, "Agriculture Business")
                    {
                        
                    },
                    new Domain.HumanResources.Entities.PayPeriod("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "DESC", DateTime.Now, DateTime.Now, DateTime.Now, "Agriculture Mechanics")
                    {
                        TimeEntryEndOn = DateTime.Now,
                    },
                    new Domain.HumanResources.Entities.PayPeriod("d2253ac7-9931-4560-b42f-1fccd43c952e", "DESC", DateTime.Now, DateTime.Now, DateTime.Now, "Animal Science")
                    {
                        
                    }
                };

                List<string> payPeriodGuids = new List<string>();
                foreach (var mp in _payCyclesCollection)
                {
                    payPeriodGuids.Add(mp.RecordGuid);
                };
                //dataReaderMock.Setup(repo => repo.SelectAsync("PERPOS", It.IsAny<string>())).ReturnsAsync(employmentPerformanceReviewGuids.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("PAYCYCLE", "WITH PCY.START.DATE NE '' BY.EXP PCY.START.DATE")).ReturnsAsync(new List<string>() { id, id2, id3 }.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("PAYCYCLE", It.IsAny<string>())).ReturnsAsync(new List<string>() { id, id2, id3 }.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("PAYCYCLE", "WITH PCY.START.DATE NE '' BY.EXP PCY.START.DATE SAVING PCY.START.DATE")).ReturnsAsync(new List<string>() { DmiString.DateTimeToPickDate(DateTime.Now).ToString(), DmiString.DateTimeToPickDate(DateTime.Now).ToString(), DmiString.DateTimeToPickDate(DateTime.Now).ToString() }.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("PAYCYCLE", "WITH PCY.START.DATE NE '' AND WITH PCY.START.DATE EQ 'convertedStartOn' AND WITH PCY.END.DATE EQ 'convertedEndOn' AND WITH PAYCYCLE.ID = 'payCycleCode' BY.EXP PCY.START.DATE SAVING PCY.START.DATE")).ReturnsAsync(new List<string>() { DmiString.DateTimeToPickDate(DateTime.Now).ToString(), DmiString.DateTimeToPickDate(DateTime.Now).ToString(), DmiString.DateTimeToPickDate(DateTime.Now).ToString() }.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("LDM.GUID", string.Format("WITH LDM.GUID.ENTITY = 'PAYCYCLE' AND WITH LDM.GUID.PRIMARY.KEY = '{0}' AND WITH LDM.GUID.SECONDARY.FLD = 'PCY.START.DATE' AND WITH LDM.GUID.SECONDARY.KEY = '{1}'", id, DmiString.DateTimeToPickDate(DateTime.Now).ToString()))).ReturnsAsync(new List<string>() { guid }.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("LDM.GUID", string.Format("WITH LDM.GUID.ENTITY = 'PAYCYCLE' AND WITH LDM.GUID.PRIMARY.KEY = '{0}' AND WITH LDM.GUID.SECONDARY.FLD = 'PCY.START.DATE' AND WITH LDM.GUID.SECONDARY.KEY = '{1}'", id2, DmiString.DateTimeToPickDate(DateTime.Now).ToString()))).ReturnsAsync(new List<string>() { guid2 }.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("LDM.GUID", string.Format("WITH LDM.GUID.ENTITY = 'PAYCYCLE' AND WITH LDM.GUID.PRIMARY.KEY = '{0}' AND WITH LDM.GUID.SECONDARY.FLD = 'PCY.START.DATE' AND WITH LDM.GUID.SECONDARY.KEY = '{1}'", id3, DmiString.DateTimeToPickDate(DateTime.Now).ToString()))).ReturnsAsync(new List<string>() { guid3 }.ToArray());
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Paycycle>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_payCyclesCollection);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Paycycle>("PAYCYCLE", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_payCyclesCollection);
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Paycycle>("PAYCYCLE", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_payCyclesCollection.FirstOrDefault());
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Paycntrl>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_payCntrlsCollection);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Paycntrl>("PAYCNTRL", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_payCntrlsCollection);

                payPeriodsRepo = BuildValidReferenceDataRepository();

            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _payCyclesCollection = null;
                payPeriodsRepo = null;
            }

            [TestMethod]
            public async Task PayPeriodsDataRepo_GetGetPayPeriodsAsync()
            {
                var result = await payPeriodsRepo.GetPayPeriodsAsync(0, 3, bypassCache: false);

                for (int i = 0; i < _payCyclesCollection.Count(); i++)
                {
                    Assert.AreEqual(_payCyclesCollection.ElementAt(i).RecordGuid, result.Item1.ElementAt(i).Id);
                    Assert.AreEqual(_payCyclesCollection.ElementAt(i).Recordkey, result.Item1.ElementAt(i).PayCycle);
                    //Assert.AreEqual(_perposCollection.ElementAt(i).JbapPosId, result.Item1.ElementAt(i).PersonId);
                }
            }

            [TestMethod]
            public async Task PayPeriodsDataRepo_GetGetPayPeriodsAsync_WithFilters()
            {
                var result = await payPeriodsRepo.GetPayPeriodsAsync(0, 3, "payCycleCode", "convertedStartOn", "convertedEndOn", bypassCache: false);

                for (int i = 0; i < _payCyclesCollection.Count(); i++)
                {
                    Assert.AreEqual(_payCyclesCollection.ElementAt(i).RecordGuid, result.Item1.ElementAt(i).Id);
                    Assert.AreEqual(_payCyclesCollection.ElementAt(i).Recordkey, result.Item1.ElementAt(i).PayCycle);
                    //Assert.AreEqual(_perposCollection.ElementAt(i).JbapPosId, result.Item1.ElementAt(i).PersonId);
                }
            }

            [TestMethod]
            public async Task PayPeriodsDataRepo_GetGetPayPeriodByIdAsync()
            {
                var result = await payPeriodsRepo.GetPayPeriodByIdAsync("F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant());

                Assert.AreEqual(_payCyclesCollection.ElementAt(0).RecordGuid, result.Id);
                Assert.AreEqual(_payCyclesCollection.ElementAt(0).Recordkey, result.PayCycle);

            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PayPeriodsRepo_GetsGetPayPeriods_Exception()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);
                dataReaderMock.Setup(dr => dr.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(null);
                dataAccessorMock.Setup(da => da.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);

                await payPeriodsRepo.GetPayPeriodsAsync(0, 2, bypassCache: true);
            }

            private PayPeriodsRepository BuildValidReferenceDataRepository()
            {
                // Initialize the Mock framework
                MockInitialize();

                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transInvokerMock.Object);

                // Cache mocking
                var cacheProviderMock = new Mock<ICacheProvider>();
                /*cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
                )));*/

             //   cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             //x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             //.ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, null));

                payPeriodsRepo = new PayPeriodsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return payPeriodsRepo;
            }
        }

    }
}
