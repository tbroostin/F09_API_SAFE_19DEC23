// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class PersonMatchingRequestsRepositoryTests : BaseRepositorySetup
    {
        Mock<IPersonMatchingRequestsRepository> _personMatchingRequestsRepositoryMock;
        IPersonMatchingRequestsRepository _personMatchingRequestsRepository;
        Domain.Base.Entities.PersonMatchRequest criteriaObj = null;
        readonly string personId = "0003582";

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();
            _personMatchingRequestsRepositoryMock = new Mock<IPersonMatchingRequestsRepository>();
            _personMatchingRequestsRepository = BuildValidPersonMatchingRequestsRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            cacheProviderMock = null;
            localCacheMock = null;
        }

        #region GET ALL
        [TestMethod]
        public async Task GetPersonMatchRequestsAsync_PersonFilter_EmptyResults()
        {
            var result = await _personMatchingRequestsRepository.GetPersonMatchRequestsAsync(0, 100, criteriaObj, new string[1] { "1" }, It.IsAny<bool>());

            Assert.IsNotNull(result);
            Assert.AreEqual(true, !result.Item1.Any());
            Assert.AreEqual(0, result.Item2);
        }

        [TestMethod]
        public async Task GetPersonMatchRequestsAsync_CriteriaObj_WithOutcomes()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync("PERSON.MATCH.REQUEST", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(() => null);
            criteriaObj = new Domain.Base.Entities.PersonMatchRequest();
            criteriaObj.AddPersonMatchRequestOutcomes(
                new Domain.Base.Entities.PersonMatchRequestOutcomes(
                    Domain.Base.Entities.PersonMatchRequestType.Initial,
                    Domain.Base.Entities.PersonMatchRequestStatus.ExistingPerson,
                    new DateTimeOffset(new DateTime(2019, 11, 1))));

            var result = await _personMatchingRequestsRepository.GetPersonMatchRequestsAsync(0, 100, criteriaObj, null, It.IsAny<bool>());

            Assert.IsNotNull(result);
            Assert.AreEqual(true, !result.Item1.Any());
            Assert.AreEqual(0, result.Item2);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetPersonMatchRequestsAsync_CriteriaObj_Originator_NullDataContracts_PersonMatchRequest()
        {
            DataContracts.PersonMatchRequest personMatchRequests = new DataContracts.PersonMatchRequest();
            dataReaderMock.Setup(repo => repo.SelectAsync("PERSON.MATCH.REQUEST", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1" });
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.PersonMatchRequest>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<DataContracts.PersonMatchRequest>() { personMatchRequests });
            criteriaObj = new Domain.Base.Entities.PersonMatchRequest()
            {
                Originator = "ELEVATE"
            };
            var result = await _personMatchingRequestsRepository.GetPersonMatchRequestsAsync(0, 100, criteriaObj, null, It.IsAny<bool>());
        }

        [TestMethod]
        public async Task GetPersonMatchingRequestsAsync()
        {
            DataContracts.PersonMatchRequest personMatchRequests = new DataContracts.PersonMatchRequest()
            {
                Recordkey = "1",
                RecordGuid = "1c083c69-7f58-42b0-ac38-3effec7fc7bc",
                PmrPersonId = personId,
                PmrOriginator = "ELEVATE",
                PmrInitialStatus = "D",
                PmrInitialStatusDate = new DateTime(2019, 11, 1),
                PmrInitialStatusTime = new DateTime(2019, 11, 1, 12, 15, 1),
                PmrRequestType = "PROSPECT"
            };
            dataReaderMock.Setup(repo => repo.SelectAsync("PERSON.MATCH.REQUEST", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1" });
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.PersonMatchRequest>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<DataContracts.PersonMatchRequest>() { personMatchRequests });

            var result = await _personMatchingRequestsRepository.GetPersonMatchRequestsAsync(0, 100, It.IsAny<Domain.Base.Entities.PersonMatchRequest>(), null, It.IsAny<bool>());
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Item2);
        }

        #endregion

        #region GET BY ID

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPersonMatchRequestsByIdAsync_ArgumentNullException()
        {
            await _personMatchingRequestsRepository.GetPersonMatchRequestsByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetPersonMatchRequestsByIdAsync_KeyNotFoundException()
        {
            await _personMatchingRequestsRepository.GetPersonMatchRequestsByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetPersonMatchRequestsByIdAsync_Missing_Initial_and_Final()
        {
            DataContracts.PersonMatchRequest personMatchRequests = new DataContracts.PersonMatchRequest() { PmrPersonId = "0003582", PmrOriginator = "ELEVATE" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.PersonMatchRequest>(It.IsAny<string>(), It.IsAny<GuidLookup>(), It.IsAny<bool>())).ReturnsAsync(personMatchRequests);

            await _personMatchingRequestsRepository.GetPersonMatchRequestsByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        public async Task GetPersonMatchRequestsByIdAsync()
        {
            DataContracts.PersonMatchRequest personMatchRequests = new DataContracts.PersonMatchRequest()
            {
                Recordkey = "1",
                RecordGuid = "1c083c69-7f58-42b0-ac38-3effec7fc7bc",
                PmrPersonId = personId,
                PmrOriginator = "ELEVATE",
                PmrInitialStatus = "D",
                PmrInitialStatusDate = new DateTime(2019, 11, 1),
                PmrInitialStatusTime = new DateTime(2019, 11, 1, 12, 15, 1),
                PmrRequestType = "PROSPECT"
            };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.PersonMatchRequest>(It.IsAny<string>(), It.IsAny<GuidLookup>(), It.IsAny<bool>())).ReturnsAsync(personMatchRequests);

            var result = await _personMatchingRequestsRepository.GetPersonMatchRequestsByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
            Assert.IsNotNull(result);
            Assert.AreEqual("ELEVATE", result.Originator);
            Assert.AreEqual(personId, result.PersonId);
            Assert.AreEqual(true, (result.Outcomes != null && result.Outcomes.Any()));
            Assert.AreEqual("ExistingPerson", result.Outcomes.ElementAt(0).Status.ToString());
            Assert.AreEqual("Initial", result.Outcomes.ElementAt(0).Type.ToString());
            Assert.AreEqual(new DateTime(2019, 11, 1, 12, 15, 1), result.Outcomes.ElementAt(0).Date);
        }

        #endregion

        #region person-matching-requests-initiations-prospects        
        [TestClass]
        public class PersonMatchingRequestsInitiationsProspectsRepositoryTests : BaseRepositorySetup
        {
            #region DECLARATIONS

            private PersonMatchingRequestsRepository personMatchingRequestsRepository;
            private CreatePersonMatchRequestResponse response;
            private Domain.Base.Entities.PersonMatchRequestInitiation entity;
            private DataContracts.PersonMatchRequest personMatchRequestDataContract;
            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";
            private string personId = "0002384";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                InitializeTestData();
                InitializeTestMock();
                personMatchingRequestsRepository = new PersonMatchingRequestsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
                personMatchingRequestsRepository = null;
                response = null;
                entity = null;
                personMatchRequestDataContract = null;
            }

            private void InitializeTestData()
            {
                entity = new Domain.Base.Entities.PersonMatchRequestInitiation()
                {
                    PersonId = personId,
                    Guid = guid,
                    RecordKey = "1",
                    Originator = "ELEVATE",
                    FirstName = "Joshua",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Gender = "M",
                    BirthDate = new DateTime(2001, 11, 22)
                };

                personMatchRequestDataContract = new DataContracts.PersonMatchRequest()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    PmrPersonId = personId,
                    PmrOriginator = "ELEVATE",
                    PmrInitialStatus = "D",
                    PmrInitialStatusDate = new DateTime(2019, 11, 1),
                    PmrInitialStatusTime = new DateTime(2019, 11, 1, 12, 15, 1),
                    PmrRequestType = "PROSPECT"
                };

                response = new CreatePersonMatchRequestResponse() { Guid = guid, PersonMatchRequestId = "1" };
            }

            private void InitializeTestMock()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<CreatePersonMatchRequestRequest, CreatePersonMatchRequestResponse>(It.IsAny<CreatePersonMatchRequestRequest>()))
                    .ReturnsAsync(response);
                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.PersonMatchRequest>(It.IsAny<string>(), It.IsAny<GuidLookup>(), true)).ReturnsAsync(personMatchRequestDataContract);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CreatePersonMatchingRequestsInitiationsProspectsAsync_RepositoryException()
            {
                response.CreatePersonMatchErrors = new List<CreatePersonMatchErrors>()
                {
                    new CreatePersonMatchErrors(){ ErrorCodes = "Error" }
                };
                await personMatchingRequestsRepository.CreatePersonMatchingRequestsInitiationsProspectsAsync(entity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreatePersonMatchingRequestsInitiationsProspectsAsync_ArgumentNullException()
            {
                var actual = await personMatchingRequestsRepository.CreatePersonMatchingRequestsInitiationsProspectsAsync(null);
            }

            [TestMethod]
            public async Task CreatePersonMatchingRequestsInitiationsProspectsAsync()
            {
                personMatchingRequestsRepository.EthosExtendedDataDictionary = new Dictionary<string, string>();
                personMatchingRequestsRepository.EthosExtendedDataDictionary.Add("key", "value");

                var actual = await personMatchingRequestsRepository.CreatePersonMatchingRequestsInitiationsProspectsAsync(entity);
                Assert.IsNotNull(actual);
                Assert.AreEqual(actual.Guid, guid);
                Assert.AreEqual("ELEVATE", actual.Originator);
                Assert.AreEqual(personId, actual.PersonId);
                Assert.AreEqual(true, (actual.Outcomes != null && actual.Outcomes.Any()));
                Assert.AreEqual("ExistingPerson", actual.Outcomes.ElementAt(0).Status.ToString());
                Assert.AreEqual("Initial", actual.Outcomes.ElementAt(0).Type.ToString());
                Assert.AreEqual(new DateTime(2019, 11, 1, 12, 15, 1), actual.Outcomes.ElementAt(0).Date);
            }
            #endregion
        }

        #endregion

        private PersonMatchingRequestsRepository BuildValidPersonMatchingRequestsRepository()
        {
            PersonMatchingRequestsRepository repository = new PersonMatchingRequestsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            return repository;
        }
    }
}
