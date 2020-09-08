// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class PersonVisaRepositoryTests
    {
        [TestClass]
        public class PersonVisasGetMethods_PUT_POST_DELETE
        {
            Mock<ICacheProvider> iCacheProviderMock;
            Mock<IColleagueTransactionFactory> iColleagueTransactionFactoryMock;
            Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;
            Mock<ILogger> iLoggerMock;
            Mock<IColleagueDataReader> dataReaderMock;

            PersonVisaRepository personVisaRepository;
            PersonVisa personVisa;
            Dictionary<string, GuidLookupResult> guidLookupResults;
            ForeignPerson foreignPersonContract;
            Ellucian.Colleague.Data.Base.DataContracts.Person personContract;
            Domain.Base.Entities.PersonVisaRequest personVisaRequest;
            UpdatePersonVisaResponse updateResponse;
            DeletePersonVisaRequest deletePersonVisaRequest;
            DeletePersonVisaResponse deletePersonVisaResponse;
            string id = string.Empty;
            string recKey = string.Empty;

            [TestInitialize]
            public void Initialize()
            {
                iCacheProviderMock = new Mock<ICacheProvider>();
                iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                iLoggerMock = new Mock<ILogger>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(iColleagueTransactionInvokerMock.Object);

                BuildObjects();
                personVisaRepository = new PersonVisaRepository(iCacheProviderMock.Object, iColleagueTransactionFactoryMock.Object, iLoggerMock.Object);
            }

            private void BuildObjects()
            {
                id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";
                recKey = "0012297";
                guidLookupResults = new Dictionary<string,GuidLookupResult>();// 
                guidLookupResults.Add("FOREIGN.PERSON", new GuidLookupResult() { Entity = "FOREIGN.PERSON", PrimaryKey = "0012297", SecondaryKey = "" });
                foreignPersonContract = new ForeignPerson()
                {
                    Recordkey = recKey,
                    RecordGuid = id,
                    FperVisaNo = "A123456",
                    FperVisaRequestDate = new DateTime(2015, 09, 17)
                };
                personContract = new DataContracts.Person()
                {
                    Recordkey = recKey,
                    RecordGuid = id,
                    VisaType = "F1",
                    VisaIssuedDate = new DateTime(2015, 10, 17),
                    VisaExpDate = new DateTime(2017, 12, 17),
                    PersonCountryEntryDate = new DateTime(2016, 02, 05)
                };
                personVisa = BuildPersonVisa(foreignPersonContract, personContract);

                personVisaRequest = new PersonVisaRequest(id, recKey) 
                {
                    EntryDate = new DateTime(2016, 01, 10),
                    ExpireDate = new DateTime(2017, 12, 17),
                    IssueDate = new DateTime(2015, 10, 17),
                    PersonId = recKey,
                    RequestDate = new DateTime(2015, 09, 17),
                    Status =  "current",
                    StrGuid = id,
                    VisaNo = "A123456",
                    VisaType = "F1"
                };
                updateResponse = new UpdatePersonVisaResponse() 
                {
                    PersonId = recKey,
                    StrGuid = id
                };
                deletePersonVisaRequest = new DeletePersonVisaRequest()
                {
                    StrGuid = id,
                    PersonId = recKey
                };
                deletePersonVisaResponse = new DeletePersonVisaResponse() 
                {
                    DeleteVisaErrors = new List<DeleteVisaErrors>()
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                personVisaRepository = null;
                personVisa = null;
                guidLookupResults = null;
                foreignPersonContract = null;
                personContract = null;
                personVisaRequest = null;
                updateResponse = null;
                deletePersonVisaRequest = null;
                deletePersonVisaResponse = null;
                id = string.Empty;
                recKey = string.Empty;
            }
            private PersonVisa BuildPersonVisa(ForeignPerson foreignPersonContract, Ellucian.Colleague.Data.Base.DataContracts.Person personContract)
            {
                PersonVisa personVisa = new PersonVisa(personContract.Recordkey, personContract.VisaType);
                personVisa.Guid = foreignPersonContract.RecordGuid;
                personVisa.PersonGuid = personContract.RecordGuid;
                personVisa.VisaNumber = foreignPersonContract.FperVisaNo;
                personVisa.RequestDate = foreignPersonContract.FperVisaRequestDate;
                personVisa.IssueDate = personContract.VisaIssuedDate;
                personVisa.ExpireDate = personContract.VisaExpDate;
                personVisa.EntryDate = personContract.PersonCountryEntryDate;
                return personVisa;
            }

            [TestMethod]
            public async Task PersonVisaRepo_UpdatePersonVisaAsync()
            {
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdatePersonVisaRequest, UpdatePersonVisaResponse>(It.IsAny<UpdatePersonVisaRequest>())).ReturnsAsync(updateResponse);

                //var foreignPersonContract = await DataReader.ReadRecordAsync<ForeignPerson>(updateResponse.PersonId, false);
                dataReaderMock.Setup(i => i.ReadRecordAsync<ForeignPerson>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(foreignPersonContract);

                //var personContract = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(updateResponse.PersonId, false);
                dataReaderMock.Setup(i => i.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(personContract);


                var result = await personVisaRepository.UpdatePersonVisaAsync(personVisaRequest);

                Assert.AreEqual(personVisaRequest.PersonId, result.PersonId);
                Assert.AreEqual(personVisaRequest.StrGuid, result.Guid);
            }
           

            [TestMethod]
            public async Task PersonVisaRepo_DeletePersonVisaAsync()
            {
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<DeletePersonVisaRequest, DeletePersonVisaResponse>(It.IsAny<DeletePersonVisaRequest>())).ReturnsAsync(deletePersonVisaResponse);
                await personVisaRepository.DeletePersonVisaAsync(id, recKey);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonVisasRepo_UpdatePersonVisaAsync_RepositoryException()
            {
                updateResponse.VisaErrorMessages = new List<VisaErrorMessages>() {new VisaErrorMessages(){ ErrorCode = "123", ErrorMsg = "Error occured"} };
                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdatePersonVisaRequest, UpdatePersonVisaResponse>(It.IsAny<UpdatePersonVisaRequest>())).ReturnsAsync(updateResponse);
                var result = await personVisaRepository.UpdatePersonVisaAsync(personVisaRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonVisasRepo_DeletePersonVisaAsync_RepositoryException()
            {
                deletePersonVisaResponse.DeleteVisaErrors = new List<DeleteVisaErrors>() { new DeleteVisaErrors() { ErrorCode = "123", ErrorMsg = "Error Occured" } };

                iColleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<DeletePersonVisaRequest, DeletePersonVisaResponse>(It.IsAny<DeletePersonVisaRequest>())).ReturnsAsync(deletePersonVisaResponse);
                await personVisaRepository.DeletePersonVisaAsync(id, recKey);
            }            
        }

        [TestClass]
        public class PersonVisasGetMethods_GET : BaseRepositorySetup
        {
            IPersonVisaRepository personVisaRepository;
            Tuple<IEnumerable<PersonVisa>, int> personVisaTuple;
            IEnumerable<PersonVisa> personVisaEntities;
            Collection<ForeignPerson> foreignPersonDataContracts;
            Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> personDataContracts;

            string[] personVisaIds = new[] { "1", "2" };



            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                BuildData();
                personVisaRepository = BuildValidPersonVisaRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                personVisaRepository = null;
                personVisaTuple = null;
                personVisaEntities = null;
                foreignPersonDataContracts = null;
                personDataContracts = null;
                personVisaIds = null;
            }

            [TestMethod]
            public async Task PersonVisaRepository_GetAll()
            {
                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(personVisaIds);
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(personDataContracts);
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<ForeignPerson>("FOREIGN.PERSON", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(foreignPersonDataContracts);

                var actuals = await personVisaRepository.GetAllPersonVisasAsync(0, 3, It.IsAny<string>(), It.IsAny<bool>());
                Assert.IsNotNull(actuals);
                Assert.AreEqual(personVisaEntities.Count(), actuals.Item1.Count());

                foreach (var actual in actuals.Item1)
                {
                    var expected = personVisaEntities.FirstOrDefault(i => i.Guid.Equals(actual.Guid, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.PersonId, actual.PersonId);
                    Assert.AreEqual(expected.VisaNumber, actual.VisaNumber);
                    Assert.AreEqual(expected.RequestDate, actual.RequestDate);
                    Assert.AreEqual(expected.IssueDate, actual.IssueDate);
                    Assert.AreEqual(expected.ExpireDate, actual.ExpireDate);
                    Assert.AreEqual(expected.EntryDate, actual.EntryDate);
                }
            }

            [TestMethod]
            public async Task PersonVisaRepository_GetById()
            {
                var foreignPerson = foreignPersonDataContracts.First();
                var person = personDataContracts.First();
                dataReaderMock.Setup(i => i.ReadRecordAsync<ForeignPerson>("1", It.IsAny<bool>())).ReturnsAsync(foreignPerson);
                dataReaderMock.Setup(i => i.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("1", It.IsAny<bool>())).ReturnsAsync(person);

                var actual = await personVisaRepository.GetPersonVisaByIdAsync("1");
                var expected = personVisaEntities.FirstOrDefault(i => i.Guid.Equals(actual.Guid, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.PersonId, actual.PersonId);
                Assert.AreEqual(expected.VisaNumber, actual.VisaNumber);
                Assert.AreEqual(expected.RequestDate, actual.RequestDate);
                Assert.AreEqual(expected.IssueDate, actual.IssueDate);
                Assert.AreEqual(expected.ExpireDate, actual.ExpireDate);
                Assert.AreEqual(expected.EntryDate, actual.EntryDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonVisaRepository_GetById_ArgumentNullException()
            {
                var actual = await personVisaRepository.GetPersonVisaByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonVisaRepository_GetById_KeyNotFoundException()
            {
                var actual = await personVisaRepository.GetPersonVisaByIdAsync("BadKey");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonVisaRepository_GetById_Null_ForeignPerson_KeyNotFoundException()
            {
                var foreignPerson = foreignPersonDataContracts.First();
                var person = personDataContracts.First();
                dataReaderMock.Setup(i => i.ReadRecordAsync<ForeignPerson>("1", It.IsAny<bool>())).ReturnsAsync(null);

                var actual = await personVisaRepository.GetPersonVisaByIdAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonVisaRepository_GetById_Null_Person_KeyNotFoundException()
            {
                var foreignPerson = foreignPersonDataContracts.First();
                var person = personDataContracts.First();
                dataReaderMock.Setup(i => i.ReadRecordAsync<ForeignPerson>("1", It.IsAny<bool>())).ReturnsAsync(foreignPerson);
                dataReaderMock.Setup(i => i.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("1", It.IsAny<bool>())).ReturnsAsync(null);

                var actual = await personVisaRepository.GetPersonVisaByIdAsync("1");
            }

            private void BuildData()
            {
                personDataContracts = new Collection<DataContracts.Person>() 
                {
                    new DataContracts.Person()
                    {
                        Recordkey = "1", RecordGuid = "6992cd94-10cc-49fc-b9e2-efff3ec13901", VisaType = "F1", VisaIssuedDate = new DateTime(2016, 01, 01), VisaExpDate = new DateTime(2017, 12, 31), 
                        PersonCountryEntryDate = new DateTime(2016, 02, 01)
                    },
                    new DataContracts.Person()
                    {
                        Recordkey = "2", RecordGuid = "a5e286ee-29ef-4e0e-85bb-4f7b80dd44dd", VisaType = "F1", VisaIssuedDate = new DateTime(2015, 01, 01), VisaExpDate = new DateTime(2018, 12, 31), 
                        PersonCountryEntryDate = new DateTime(2015, 02, 01)
                    },
                    new DataContracts.Person()
                    {
                        Recordkey = "3", RecordGuid = "332b196b-9ec0-4b07-a158-67eef656feaa", VisaType = "F1", VisaIssuedDate = new DateTime(2014, 01, 01), VisaExpDate = new DateTime(2019, 12, 31), 
                        PersonCountryEntryDate = new DateTime(2014, 02, 01)
                    },
                };

                foreignPersonDataContracts = new Collection<ForeignPerson>() 
                {
                    new ForeignPerson(){ RecordGuid = "8be66e85-85dc-447b-8a6a-e5edc7027dfa", Recordkey = "1", FperVisaNo = "1", FperVisaRequestDate  = new DateTime(2015, 12, 31) },
                    new ForeignPerson(){ RecordGuid = "c987c8bd-898d-4480-836b-64e424a3efe5", Recordkey = "2", FperVisaNo = "2", FperVisaRequestDate  = new DateTime(2014, 12, 31) },
                    new ForeignPerson(){ RecordGuid = "f90aefe5-8a33-497a-969a-641c6361e1e7", Recordkey = "3", FperVisaNo = "3", FperVisaRequestDate  = new DateTime(2013, 12, 31) }
                };

                personVisaEntities = new List<PersonVisa>() 
                {
                    new PersonVisa("1", "F1")
                    { 
                        Guid = "8be66e85-85dc-447b-8a6a-e5edc7027dfa", PersonGuid = "6992cd94-10cc-49fc-b9e2-efff3ec13901", VisaNumber = "1", RequestDate = new DateTime(2015, 12, 31), 
                        IssueDate = new DateTime(2016, 01, 01), ExpireDate = new DateTime(2017, 12, 31), EntryDate =  new DateTime(2016, 02, 01)
                    },
                    new PersonVisa("2", "F1")
                    { 
                        Guid = "c987c8bd-898d-4480-836b-64e424a3efe5", PersonGuid = "a5e286ee-29ef-4e0e-85bb-4f7b80dd44dd", VisaNumber = "2", RequestDate = new DateTime(2014, 12, 31), 
                        IssueDate = new DateTime(2015, 01, 01), ExpireDate = new DateTime(2018, 12, 31), EntryDate =  new DateTime(2015, 02, 01)
                    },
                    new PersonVisa("3", "F1")
                    { 
                        Guid = "f90aefe5-8a33-497a-969a-641c6361e1e7", PersonGuid = "332b196b-9ec0-4b07-a158-67eef656feaa", VisaNumber = "3", RequestDate = new DateTime(2013, 12, 31) , 
                        IssueDate = new DateTime(2014, 01, 01), ExpireDate = new DateTime(2019, 12, 31), EntryDate =  new DateTime(2014, 02, 01)
                    },
                };
                personVisaTuple = new Tuple<IEnumerable<PersonVisa>, int>(personVisaEntities, personVisaEntities.Count());
            }

            public IPersonVisaRepository BuildValidPersonVisaRepository()
            {
                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        var rel = personDataContracts.FirstOrDefault(x => x.Recordkey == gl.Guid);
                        result.Add(gl.Guid, rel == null ? null : new GuidLookupResult() { Entity = "FOREIGN.PERSON", PrimaryKey = rel.Recordkey });
                    }
                    return Task.FromResult(result);
                });

                // Build  repository
                personVisaRepository = new PersonVisaRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
              x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
              .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                Mock<IColleagueTransactionInvoker> mockManager = new Mock<IColleagueTransactionInvoker>();

                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                var resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 2,
                    CacheName = "AllPersonVisas",
                    Entity = "FOREIGN.PERSON",
                    Sublist = new List<string>() { "1", "2", "3" },
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
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);


                return personVisaRepository;
            }
        }
    }
}