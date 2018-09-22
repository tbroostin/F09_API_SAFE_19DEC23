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
    public class PersonBenefitDependentRepositoryTests_V10
    {
        [TestClass]
        public class PersonBenefitDependentTests : BaseRepositorySetup
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transInvokerMock;
            Mock<IColleagueDataReader> dataReaderMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            Collection<DataContracts.Perben> _perbensCollection;
            List<Domain.HumanResources.Entities.PersonBenefitDependent> personBenefitDependentsEntities;
            //CreatePerposReviewRequest employmentPerformanceReviewRequest;
            //CreatePerposReviewResponse updateResponse;
            //DeletePerposReviewRequest deleteEmploymentPerformanceReviewRequest;
            //DeletePerposReviewResponse deleteEmploymentPerformanceReviewResponse;
            string codeItemName;
            ApiSettings apiSettings;

            PersonBenefitDependentsRepository perbensRepo;

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
                _perbensCollection = new Collection<DataContracts.Perben>()
                {
                    new DataContracts.Perben() { RecordGuid = guid, Recordkey = id, PerbenDependId = new List<string>() { sid },
                        PerbenDepProviderId = new List<string>() { sid }, PerbenDepStartDate = new List<DateTime?>() { DateTime.Now }, PerbenDepEndDate = new List<DateTime?>() { DateTime.Now },
                        PerbenDepFullTimeStudent = new List<string>(){ "Y" }, PerbenDepProviderName = new List<string>(){ "Me" }},
                    new DataContracts.Perben() { RecordGuid = guid2, Recordkey = id2, PerbenDependId = new List<string>() { sid2 },
                        PerbenDepProviderId = new List<string>() { sid2 }, PerbenDepStartDate = new List<DateTime?>() { DateTime.Now }, PerbenDepEndDate = new List<DateTime?>() { DateTime.Now },
                        PerbenDepFullTimeStudent = new List<string>(){ "Y" }, PerbenDepProviderName = new List<string>(){ "You" }},
                    new DataContracts.Perben() { RecordGuid = guid3, Recordkey = id3, PerbenDependId = new List<string>() { sid3 },
                        PerbenDepProviderId = new List<string>() { sid3 }, PerbenDepStartDate = new List<DateTime?>() { DateTime.Now }, PerbenDepEndDate = new List<DateTime?>() { DateTime.Now },
                        PerbenDepFullTimeStudent = new List<string>(){ "Y" }, PerbenDepProviderName = new List<string>(){ "We" }},
                };

                guidLookup = new GuidLookup(guid);
                guidLookupResult = new GuidLookupResult() { Entity = "PERBEN", PrimaryKey = id, SecondaryKey = sid };
                guidLookupDict = new Dictionary<string, GuidLookupResult>();
                recordLookup = new RecordKeyLookup("PERBEN", id, "PERBEN.DEPEND.ID", sid, false);
                recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
               
                dataReaderMock.SetupSequence(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "PERBEN", PrimaryKey = id, SecondaryKey = sid } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid2, new GuidLookupResult() { Entity = "PERBEN", PrimaryKey = id2, SecondaryKey = sid2 } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid3, new GuidLookupResult() { Entity = "PERBEN", PrimaryKey = id3, SecondaryKey = sid3 } } }));
                dataReaderMock.SetupSequence(ep => ep.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "PERBEN+" + id + "+" + sid, new RecordKeyLookupResult() { Guid = guid } } }))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "PERBEN+" + id2 + "+" + sid2, new RecordKeyLookupResult() { Guid = guid2 } } }))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "PERBEN+" + id3 + "+" + sid3, new RecordKeyLookupResult() { Guid = guid3 } } }));
                
                personBenefitDependentsEntities = new List<Domain.HumanResources.Entities.PersonBenefitDependent>() 
                {
                    new Domain.HumanResources.Entities.PersonBenefitDependent("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e"),
                    new Domain.HumanResources.Entities.PersonBenefitDependent("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d"),
                    new Domain.HumanResources.Entities.PersonBenefitDependent("d2253ac7-9931-4560-b42f-1fccd43c952e", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                };

                List<string> personBenefitDependentGuids = new List<string>();
                foreach (var mp in _perbensCollection)
                {
                    personBenefitDependentGuids.Add(mp.RecordGuid);
                };
                //dataReaderMock.Setup(repo => repo.SelectAsync("PERPOS", It.IsAny<string>())).ReturnsAsync(employmentPerformanceReviewGuids.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("PERBEN", "WITH PERBEN.DEPEND.ID NE '' BY.EXP PERBEN.DEPEND.ID")).ReturnsAsync(new List<string>() { id, id2, id3 }.ToArray());
                dataReaderMock.Setup(repo => repo.SelectAsync("PERBEN", "WITH PERBEN.DEPEND.ID NE '' BY.EXP PERBEN.DEPEND.ID SAVING PERBEN.DEPEND.ID")).ReturnsAsync(new List<string>() { sid, sid2, sid3 }.ToArray());
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Perben>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_perbensCollection);
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Perben>("PERBEN", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_perbensCollection);
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Perben>("PERBEN", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_perbensCollection.FirstOrDefault());

                perbensRepo = BuildValidReferenceDataRepository();

            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _perbensCollection = null;
                perbensRepo = null;
            }

            [TestMethod]
            public async Task PersonBenefitDependentsDataRepo_GetGetPersonBenefitDependentsAsync()
            {
                var result = await perbensRepo.GetPersonBenefitDependentsAsync(0, 3, false);

                for (int i = 0; i < _perbensCollection.Count(); i++)
                {
                    Assert.AreEqual(_perbensCollection.ElementAt(i).RecordGuid, result.Item1.ElementAt(i).Guid);
                    Assert.AreEqual(_perbensCollection.ElementAt(i).Recordkey, result.Item1.ElementAt(i).DeductionArrangement);
                    //Assert.AreEqual(_perposCollection.ElementAt(i).JbapPosId, result.Item1.ElementAt(i).PersonId);
                }
            }

            [TestMethod]
            public async Task PersonBenefitDependentsDataRepo_GetGetPersonBenefitDependentByIdAsync()
            {
                var result = await perbensRepo.GetPersonBenefitDependentByIdAsync("F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant());

                Assert.AreEqual(_perbensCollection.ElementAt(0).RecordGuid, result.Guid);
                Assert.AreEqual(_perbensCollection.ElementAt(0).Recordkey, result.DeductionArrangement);
                    
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PersonBenefitDependentsRepo_GetsGetPersonBenefitDependents_Exception()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);
                dataAccessorMock.Setup(da => da.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);

                await perbensRepo.GetPersonBenefitDependentsAsync(0, 2, true);
            }

            private PersonBenefitDependentsRepository BuildValidReferenceDataRepository()
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

                perbensRepo = new PersonBenefitDependentsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return perbensRepo;
            }
        }

    }
}
