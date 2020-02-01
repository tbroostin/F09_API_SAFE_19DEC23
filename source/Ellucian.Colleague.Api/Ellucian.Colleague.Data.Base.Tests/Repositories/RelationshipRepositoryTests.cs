// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
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
    public class RelationshipRepositoryTests
    {
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataReaderMock;
        Mock<ILogger> loggerMock;
        RelationshipRepository relationshipRepo;
        protected Mock<IColleagueTransactionInvoker> transManagerMock;
        protected IColleagueTransactionInvoker transManager;

        private const string _primaryId = "PrimaryId";
        private const string _parentId = "ParentId";
        private const string _childId = "ChildId";

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            cacheProviderMock = new Mock<ICacheProvider>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            dataReaderMock = new Mock<IColleagueDataReader>();

            // set up cache for async
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null)).Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1))));

            // set up the data reader mock to return the above data structure.  Requires the use of 'Task.FromResult' to avoid hanging.
            dataReaderMock.Setup<Task<Collection<Data.Base.DataContracts.Relationship>>>(dr => dr.BulkReadRecordAsync<Data.Base.DataContracts.Relationship>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult(dataFromDataReader()));
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

            // Set up transaction manager for mocking 
            transManagerMock = new Mock<IColleagueTransactionInvoker>();
            transManager = transManagerMock.Object;

            // Build  repository
            relationshipRepo = new RelationshipRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataReaderMock = null;
            cacheProviderMock = null;
            relationshipRepo = null;
        }

        #region GetPersonRelationshipsAsync Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipRepository_GetPersonRelationshipsAsync_NullId()
        {
            var repoData = await relationshipRepo.GetPersonRelationshipsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipRepository_GetPersonRelationshipsAsync_EmptyId()
        {
            var repoData = await relationshipRepo.GetPersonRelationshipsAsync(string.Empty);
        }

        [TestMethod]
        public async Task RelationshipRepository_GetPersonRelationshipsAsync_Count()
        {
            var repoData = await relationshipRepo.GetPersonRelationshipsAsync(_primaryId);
            Assert.AreEqual(dataFromDataReader().Count, repoData.Count());
        }

        [TestMethod]
        public async Task RelationshipRepository_GetPersonRelationshipsAsync_Content()
        {
            var repoData = await relationshipRepo.GetPersonRelationshipsAsync(_primaryId);
            var sourceData = dataFromDataReader();
            for (int i = 0; i < sourceData.Count(); i++)
            {
                Assert.AreEqual(sourceData.ElementAt(i).RsId1, repoData.ElementAt(i).PrimaryEntity);
                Assert.AreEqual(sourceData.ElementAt(i).RsId2, repoData.ElementAt(i).OtherEntity);
                Assert.AreEqual(sourceData.ElementAt(i).RsRelationType, repoData.ElementAt(i).RelationshipType);
                Assert.AreEqual(null, repoData.ElementAt(i).StartDate);
                Assert.AreEqual(null, repoData.ElementAt(i).EndDate);
            }
        }

        #endregion

        #region GetRelatedPersonIdsAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipRepository_GetRelatedPersonIdsAsync_NullId()
        {
            var repoData = await relationshipRepo.GetRelatedPersonIdsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipRepository_GetRelatedPersonIdsAsync_EmptyId()
        {
            var repoData = await relationshipRepo.GetRelatedPersonIdsAsync(string.Empty);
        }

        [TestMethod]
        public async Task RelationshipRepository_GetRelatedPersonIdsAsync_Count()
        {
            var repoData = await relationshipRepo.GetRelatedPersonIdsAsync(_primaryId);
            Assert.AreEqual(3, repoData.Count());
        }

        [TestMethod]
        public async Task RelationshipRepository_GetRelatedPersonIdsAsync_Content()
        {
            var repoData = await relationshipRepo.GetRelatedPersonIdsAsync(_primaryId);
            var sourceData = dataFromDataReader();
            Assert.IsTrue(repoData.Contains(_primaryId));
            Assert.IsTrue(repoData.Contains(_parentId));
            Assert.IsTrue(repoData.Contains(_childId));
        }

        #endregion

        #region PostRelationshipAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipRepository_PostRelationshipAsync_Null_P1()
        {
            var newRel = await relationshipRepo.PostRelationshipAsync(null, "RELT", "P2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipRepository_PostRelationshipAsync_Empty_P1()
        {
            var newRel = await relationshipRepo.PostRelationshipAsync("", "RELT", "P2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipRepository_PostRelationshipAsync_Null_RelType()
        {
            var newRel = await relationshipRepo.PostRelationshipAsync("P1", null, "P2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipRepository_PostRelationshipAsync_Empty_RelType()
        {
            var newRel = await relationshipRepo.PostRelationshipAsync("P1", "", "P2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipRepository_PostRelationshipAsync_Null_P2()
        {
            var newRel = await relationshipRepo.PostRelationshipAsync("P1", "RELT", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RelationshipRepository_PostRelationshipAsync_Empty_P2()
        {
            var newRel = await relationshipRepo.PostRelationshipAsync("P1", "RELT", "");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task RelationshipRepository_PostRelationshipAsync_CtxError()
        {
            var createRequest = new CreateRelationshipsRequest()
            {
                RelationsToCreate = new List<RelationsToCreate>() { new RelationsToCreate() { IsTheIds = "CtxError", RelationTypes = "FOO", OfTheIds = "P2" } }
            };
            var ctxError = new CreateRelationshipsResponse() { ErrorInd = true, Messages = new List<string>(), RelationshipIdentifiers = new List<string>() };
            transManagerMock.Setup(mgr =>
                mgr.ExecuteAsync<CreateRelationshipsRequest, CreateRelationshipsResponse>(It.Is<CreateRelationshipsRequest>(req => req.RelationsToCreate[0].IsTheIds.Equals("CtxError"))))
                .Returns(Task.FromResult<CreateRelationshipsResponse>(ctxError));
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);
            // relationshipRepo was not getting updated with mock setup, so create a new local repository
            var localRelationshipRepo = new RelationshipRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            var result = await localRelationshipRepo.PostRelationshipAsync("CtxError", "FOO", "P2");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task RelationshipRepository_PostRelationshipAsync_CtxNullResponse()
        {
            var createRequest = new CreateRelationshipsRequest()
            {
                RelationsToCreate = new List<RelationsToCreate>() { new RelationsToCreate() { IsTheIds = "CtxNullResponse", RelationTypes = "FOO", OfTheIds = "P2" } }
            };
            var ctxNullResponse = new CreateRelationshipsResponse() { ErrorInd = false, Messages = new List<string>(), RelationshipIdentifiers = null };
            transManagerMock.Setup(mgr =>
                mgr.ExecuteAsync<CreateRelationshipsRequest, CreateRelationshipsResponse>(It.Is<CreateRelationshipsRequest>(req => req.RelationsToCreate[0].IsTheIds.Equals("CtxNullResponse"))))
                .Returns(Task.FromResult<CreateRelationshipsResponse>(ctxNullResponse));
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);
            // relationshipRepo was not getting updated with mock setup, so create a new local repository
            var localRelationshipRepo = new RelationshipRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            var result = await localRelationshipRepo.PostRelationshipAsync("CtxNullResponse", "FOO", "P2");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task RelationshipRepository_PostRelationshipAsync_CtxEmptyResponse()
        {
            var createRequest = new CreateRelationshipsRequest()
            {
                RelationsToCreate = new List<RelationsToCreate>() { new RelationsToCreate() { IsTheIds = "CtxEmptyResponse", RelationTypes = "FOO", OfTheIds = "P2" } }
            };
            var ctxEmptyResponse = new CreateRelationshipsResponse() { ErrorInd = false, Messages = new List<string>(), RelationshipIdentifiers = new List<string>() };
            transManagerMock.Setup(mgr =>
                mgr.ExecuteAsync<CreateRelationshipsRequest, CreateRelationshipsResponse>(It.Is<CreateRelationshipsRequest>(req => req.RelationsToCreate[0].IsTheIds.Equals("CtxEmptyResponse"))))
                .Returns(Task.FromResult<CreateRelationshipsResponse>(ctxEmptyResponse));
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);
            // relationshipRepo was not getting updated with mock setup, so create a new local repository
            var localRelationshipRepo = new RelationshipRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            var result = await localRelationshipRepo.PostRelationshipAsync("CtxEmptyResponse", "FOO", "P2");
        }


        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task RelationshipRepository_PostRelationshipAsync_NewRelationshipNotFound()
        {
            var relationCreationRequest = new CreateRelationshipsRequest()
            {
                RelationsToCreate = new List<RelationsToCreate>() { new RelationsToCreate() { IsTheIds = "newRelNotFound", RelationTypes = "FOO", OfTheIds = "P2" } }
            };

            var ctxNewRelNotFoundResponse = new CreateRelationshipsResponse()
            { ErrorInd = false, Messages = new List<string>(), RelationshipIdentifiers = new List<string>() { "3" } };

            transManagerMock.Setup(mgr =>
                mgr.ExecuteAsync<CreateRelationshipsRequest, CreateRelationshipsResponse>(It.IsAny<CreateRelationshipsRequest>()))
                .Returns(Task.FromResult<CreateRelationshipsResponse>(ctxNewRelNotFoundResponse));

            // data reader returns corresponding data structure.  Requires the use of 'Task.FromResult' to avoid hanging.
            Mock<IColleagueDataReader> localDataReaderMock = new Mock<IColleagueDataReader>();
            localDataReaderMock.Setup<Task<Collection<Data.Base.DataContracts.Relationship>>>(dr =>
                dr.BulkReadRecordAsync<Data.Base.DataContracts.Relationship>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns(Task.FromResult((Collection<Data.Base.DataContracts.Relationship>)null)
            );
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(localDataReaderMock.Object);
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);
            // relationshipRepo was not getting updated with mock setup, so create a new local repository
            var localRelationshipRepo = new RelationshipRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            var result = await localRelationshipRepo.PostRelationshipAsync("newRelNotFound", "FOO", "P2");
        }

        [TestMethod]
        public async Task RelationshipRepository_PostRelationshipAsync_Success()
        {
            string p1 = "P1", p2 = "P2", relType = "FOO";
            var relationshipRequest = new CreateRelationshipsRequest()
            {
                RelationsToCreate = new List<RelationsToCreate>() { new RelationsToCreate() { IsTheIds = p1, RelationTypes = relType, OfTheIds = p2 } }
            };

            var ctxGoodResponse = new CreateRelationshipsResponse()
            { ErrorInd = false, Messages = new List<string>(), RelationshipIdentifiers = new List<string>() { "3" } };

            transManagerMock.Setup(mgr =>
                mgr.ExecuteAsync<CreateRelationshipsRequest, CreateRelationshipsResponse>(It.IsAny<CreateRelationshipsRequest>()))
                .Returns(Task.FromResult<CreateRelationshipsResponse>(ctxGoodResponse));

            // set up the data reader mock to return the above data structure.  Requires the use of 'Task.FromResult' to avoid hanging.
            var localDataReaderMock = new Mock<IColleagueDataReader>();
            localDataReaderMock.Setup<Task<Data.Base.DataContracts.Relationship>>(dr =>
                dr.ReadRecordAsync<Data.Base.DataContracts.Relationship>(It.IsAny<string>(), true))
                .Returns(Task.FromResult<Relationship>(new Relationship() { RsId1 = p1, RsId2 = p2, RsRelationType = relType, RsPrimaryRelationshipFlag = "N" }));

            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(localDataReaderMock.Object);
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);
            // relationshipRepo was not getting updated with mock setup, so create a new local repository
            var localRelationshipRepo = new RelationshipRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            var result = await localRelationshipRepo.PostRelationshipAsync(p1, relType, p2);
            Assert.AreEqual(p1, result.PrimaryEntity);
            Assert.AreEqual(relType, result.RelationshipType);
            Assert.AreEqual(p2, result.OtherEntity);
        }

        #endregion

        private Collection<Data.Base.DataContracts.Relationship> dataFromDataReader()
        {
            return new Collection<DataContracts.Relationship>()
                {
                    new Data.Base.DataContracts.Relationship(){Recordkey = "1", RsId1 = _parentId, RsId2 = _primaryId, RsRelationType = "C", RsEndDate = null, RsStartDate = null, RsPrimaryRelationshipFlag = "Y", RecordGuid = "58182363-9d54-4d47-812d-d02fc392be50"},
                    new Data.Base.DataContracts.Relationship(){Recordkey = "2", RsId1 = _primaryId, RsId2 = _childId, RsRelationType = "C", RsEndDate = null, RsStartDate = null, RsPrimaryRelationshipFlag = "Y", RecordGuid = "2d65dec8-f6f3-445d-ad7a-5f7574f0e626"},
                };
        }

        private Collection<DataContracts.Relation> GetRelationData()
        {
            return new Collection<DataContracts.Relation>()
            {
                new DataContracts.Relation(){Recordkey = _parentId + "*" + _primaryId, RelationComments = "Comment 1", RelationRelationships = new List<string>(){"C"}},
                new DataContracts.Relation(){Recordkey = _primaryId + "*" + _childId, RelationComments = "Comment 1", RelationRelationships = new List<string>(){"C"}}
            };
        }
    }

    [TestClass]
    public class RelationshipRepositoryEEDMTests : BaseRepositorySetup
    {
        IRelationshipRepository relationshipRepo;
        //protected Mock<IColleagueTransactionInvoker> transManagerMock;
        //protected IColleagueTransactionInvoker transManager;

        string[] personRelationshipsIds = new[] { "1", "2" };
        private List<string> guarianRelationshipTypes = new List<string>() { "WA", "GU" };
        Collection<Relationship> relationshipDataContracts;// = dataFromDataReader();
        Collection<Relation> relationContracts;// = GetRelationData();
        Collection<Person> people;
        Collection<Person> nonPersonRelpeople;

        private Collection<Ellucian.Colleague.Data.Base.DataContracts.Institutions> _institutionsDataContracts;

        private const string _primaryId = "PrimaryId";
        private const string _parentId = "ParentId";
        private const string _childId = "ChildId";
        string personRelationshipsId = "1";


        #region EEDM Personal Relationships Tests

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            relationshipDataContracts = dataFromDataReader();
            relationContracts = GetRelationData();
            people = GetPeople();
            nonPersonRelpeople = GetNonPersonRelationPeople();
            _institutionsDataContracts = GetInstitutions();
            relationshipRepo = BuildValidRelationshipRepository();
        }

        private Collection<Institutions> GetInstitutions()
        {
            return new Collection<Ellucian.Colleague.Data.Base.DataContracts.Institutions>()
            {
                new Institutions()
                {
                    Recordkey = "1"
                },
                new Institutions()
                {
                    Recordkey = "2"
                },
                new Institutions()
                {
                    Recordkey = "3"
                }
            };

        }

        private Collection<Person> GetPeople()
        {
            return new Collection<Person>()
            {
                new Person(){ RecordGuid = "1d65dec8-f6f3-445d-ad7a-5f7574f0e622", Gender = "M", Recordkey = "ParentId"},
                new Person(){ RecordGuid = "3d65dec8-f6f3-445d-ad7a-5f7574f0e624", Gender = "F", Recordkey = "PrimaryId"},
                new Person(){ RecordGuid = "5d65dec8-f6f3-445d-ad7a-5f7574f0e626", Gender = "M", Recordkey = "ChildId"}
            };
        }

        private Collection<Person> GetNonPersonRelationPeople()
        {
            return new Collection<Person>()
            {
                new Person(){ RecordGuid = "1d65dec8-f6f3-445d-ad7a-5f7574f0e622", Gender = "M", Recordkey = "ParentId", PersonCorpIndicator = "Y"},
                new Person(){ RecordGuid = "3d65dec8-f6f3-445d-ad7a-5f7574f0e624", Gender = "F", Recordkey = "PrimaryId", PersonCorpIndicator = "Y"},
                new Person(){ RecordGuid = "5d65dec8-f6f3-445d-ad7a-5f7574f0e626", Gender = "M", Recordkey = "ChildId", PersonCorpIndicator = "Y"}
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            cacheProviderMock = null;
            dataReaderMock = null;
            loggerMock = null;
            relationshipRepo = null;
            transManagerMock = null;
            transManager = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPersonalRelationshipById2Async_ArgumentNullException()
        {
            await relationshipRepo.GetPersonalRelationshipById2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetPersonalRelationshipById2Async_GetPersonalRelationshipsIdFromGuidAsync()
        {
            await relationshipRepo.GetPersonalRelationshipById2Async("abc");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetPersonalRelationshipById2Async_relationshipContract_Null()
        {
            dataReaderMock.Setup(i => i.ReadRecordAsync<Relationship>("RELATIONSHIP", It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(null);
            await relationshipRepo.GetPersonalRelationshipById2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetPersonalRelationshipById2Async_IsCor()
        {
            dataReaderMock.Setup(i => i.ReadRecordAsync<Relationship>("RELATIONSHIP", It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(relationshipDataContracts.FirstOrDefault());
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<Person>()
                {
                    new Person(){PersonCorpIndicator = "Y"}
                });
            await relationshipRepo.GetPersonalRelationshipById2Async("1");
        }

        [TestMethod]
        public async Task GetPersonalRelationshipById2Async()
        {
            dataReaderMock.Setup(i => i.ReadRecordAsync<Relationship>("RELATIONSHIP", It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(relationshipDataContracts.FirstOrDefault());
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(people);
            await relationshipRepo.GetPersonalRelationshipById2Async("1");
        }

        [TestMethod]
        public async Task PersonalRelationships_GetAllAsync()
        {
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(people);
            var repoData = await relationshipRepo.GetAllAsync(0, 2, It.IsAny<bool>(), It.IsAny<List<string>>());
            Assert.AreEqual(relationshipDataContracts.Count, repoData.Item1.Count());

            for (int i = 0; i < relationshipDataContracts.Count; i++)
            {
                var expected = relationshipDataContracts[i];
                var actual = repoData.Item1.ToList()[i];

                Assert.AreEqual(expected.RecordGuid, actual.Guid);
                Assert.AreEqual(expected.RsId1, actual.PrimaryEntity);
                Assert.AreEqual(expected.RsId2, actual.OtherEntity);
                Assert.AreEqual(expected.RsRelationType, actual.RelationshipType);
                Assert.AreEqual(expected.RsStartDate, actual.StartDate);
                Assert.AreEqual(expected.RsEndDate, actual.StartDate);
                Assert.AreEqual(expected.RsStartDate, actual.EndDate);
                Assert.AreEqual(expected.RsStatus, actual.Status);
            }
        }

        [TestMethod]
        public async Task PersonalRelationships_GetRelationshipsAsync()
        {
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(people);
            var repoData = await relationshipRepo.GetRelationshipsAsync(0, 2, It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
            Assert.AreEqual(relationshipDataContracts.Count, repoData.Item1.Count());

            for (int i = 0; i < relationshipDataContracts.Count; i++)
            {
                var expected = relationshipDataContracts[i];
                var actual = repoData.Item1.ToList()[i];

                Assert.AreEqual(expected.RecordGuid, actual.Guid);
                Assert.AreEqual(expected.RsId1, actual.PrimaryEntity);
                Assert.AreEqual(expected.RsId2, actual.OtherEntity);
                Assert.AreEqual(expected.RsRelationType, actual.RelationshipType);
                Assert.AreEqual(expected.RsStartDate, actual.StartDate);
                Assert.AreEqual(expected.RsEndDate, actual.StartDate);
                Assert.AreEqual(expected.RsStartDate, actual.EndDate);
                Assert.AreEqual(expected.RsStatus, actual.Status);
            }
        }

        [TestMethod]
        public async Task PersonalRelationships_GetRelationships2Async()
        {
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(people);
            var repoData = await relationshipRepo.GetRelationships2Async(0, 2, new string[] { "1" }, "RelType", "InvRelType");
            Assert.AreEqual(relationshipDataContracts.Count, repoData.Item1.Count());

            for (int i = 0; i < relationshipDataContracts.Count; i++)
            {
                var expected = relationshipDataContracts[i];
                var actual = repoData.Item1.ToList()[i];

                Assert.AreEqual(expected.RecordGuid, actual.Guid);
                Assert.AreEqual(expected.RsId1, actual.PrimaryEntity);
                Assert.AreEqual(expected.RsId2, actual.OtherEntity);
                Assert.AreEqual(expected.RsRelationType, actual.RelationshipType);
                Assert.AreEqual(expected.RsStartDate, actual.StartDate);
                Assert.AreEqual(expected.RsEndDate, actual.StartDate);
                Assert.AreEqual(expected.RsStartDate, actual.EndDate);
                Assert.AreEqual(expected.RsStatus, actual.Status);
            }
        }

        [TestMethod]
        public async Task PersonalRelationships_GetRelationships2Async_PeopleIds()
        {
            Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> dict1 = new Dictionary<string, string>();
            dict1.Add("1", "1");
            dict.Add("1", dict1);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(people);
            dataReaderMock.Setup(i => i.BatchReadRecordColumnsAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string[]>()))
                .ReturnsAsync(dict);
            dataReaderMock.Setup(i => i.SelectAsync("RELATION", It.IsAny<string[]>(), It.IsAny<string>()))
                .ReturnsAsync(new string[] {"1", "2" });
            var repoData = await relationshipRepo.GetRelationships2Async(0, 2, new string[] { "1", "2" }, "RelType", "InvRelType");
            Assert.AreEqual(relationshipDataContracts.Count, repoData.Item1.Count());

            for (int i = 0; i < relationshipDataContracts.Count; i++)
            {
                var expected = relationshipDataContracts[i];
                var actual = repoData.Item1.ToList()[i];

                Assert.AreEqual(expected.RecordGuid, actual.Guid);
                Assert.AreEqual(expected.RsId1, actual.PrimaryEntity);
                Assert.AreEqual(expected.RsId2, actual.OtherEntity);
                Assert.AreEqual(expected.RsRelationType, actual.RelationshipType);
                Assert.AreEqual(expected.RsStartDate, actual.StartDate);
                Assert.AreEqual(expected.RsEndDate, actual.StartDate);
                Assert.AreEqual(expected.RsStartDate, actual.EndDate);
                Assert.AreEqual(expected.RsStatus, actual.Status);
            }
        }
        [TestMethod]
        public async Task PersonalRelationships_GetPersonRelationshipByIdAsync()
        {
            var expected = relationshipDataContracts[0];
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(people);

            var actual = await relationshipRepo.GetPersonRelationshipByIdAsync(personRelationshipsId);

            Assert.AreEqual(expected.RecordGuid, actual.Guid);
            Assert.AreEqual(expected.RsId1, actual.PrimaryEntity);
            Assert.AreEqual(expected.RsId2, actual.OtherEntity);
            Assert.AreEqual(expected.RsRelationType, actual.RelationshipType);
            Assert.AreEqual(expected.RsStartDate, actual.StartDate);
            Assert.AreEqual(expected.RsEndDate, actual.StartDate);
            Assert.AreEqual(expected.RsStartDate, actual.EndDate);
            Assert.AreEqual(expected.RsStatus, actual.Status);
        }

        [TestMethod]
        public async Task PersonalRelationships_GetDefaultGuardianRelationshipTypesAsync_BypassTrue()
        {
            string fileName = "CORE.PARMS";
            string field = "LDM.DEFAULTS";
            LdmDefaults ldmDefaults = new LdmDefaults() { LdmdGuardianRelTypes = guarianRelationshipTypes };
            dataReaderMock.Setup(i => i.ReadRecordAsync<LdmDefaults>(fileName, field, It.IsAny<bool>())).ReturnsAsync(ldmDefaults);

            var actuals = await relationshipRepo.GetDefaultGuardianRelationshipTypesAsync(true);
            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = guarianRelationshipTypes.FirstOrDefault(i => i.Equals(actual, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
            }
        }

        [TestMethod]
        public async Task PersonalRelationships_GetDefaultGuardianRelationshipTypesAsync_BypassFalse()
        {
            string fileName = "CORE.PARMS";
            string field = "LDM.DEFAULTS";
            LdmDefaults ldmDefaults = new LdmDefaults() { LdmdGuardianRelTypes = guarianRelationshipTypes };
            dataReaderMock.Setup(i => i.ReadRecordAsync<LdmDefaults>(fileName, field, It.IsAny<bool>())).ReturnsAsync(ldmDefaults);

            var actuals = await relationshipRepo.GetDefaultGuardianRelationshipTypesAsync(false);
            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = guarianRelationshipTypes.FirstOrDefault(i => i.Equals(actual, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PersonalRelationships_GetPersonRelationshipByIdAsync_ArgumentNullException()
        {
            var actual = await relationshipRepo.GetPersonRelationshipByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonalRelationships_GetPersonRelationshipByIdAsync_KeyNotFoundException()
        {
            var actual = await relationshipRepo.GetPersonRelationshipByIdAsync("bogusId");
        }

        #region PersonGuardianRelationship

        [TestMethod]
        public async Task PersonGuardinRelationships_GetAllAsync()
        {
            dataReaderMock.Setup(i => i.SelectAsync("RELATIONSHIP", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(new[] { "ParentId", "PrimaryId" });
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>("RELATIONSHIP", new[] { "ParentId", "PrimaryId" }, true)).ReturnsAsync(relationshipDataContracts);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(It.IsAny<string[]>(), true)).ReturnsAsync(people);

            var repoData = await relationshipRepo.GetAllGuardiansAsync(0, 4, "ParentId", guarianRelationshipTypes);
            Assert.AreEqual(relationshipDataContracts.Count, repoData.Item1.Count());

            for (int i = 0; i < relationshipDataContracts.Count; i++)
            {
                var expected = relationshipDataContracts[i];
                var actual = repoData.Item1.ToList()[i];

                Assert.AreEqual(expected.RecordGuid, actual.Guid);
                Assert.AreEqual(expected.RsId1, actual.PrimaryEntity);
                Assert.AreEqual(expected.RsId2, actual.OtherEntity);
                Assert.AreEqual(expected.RsRelationType, actual.RelationshipType);
                Assert.AreEqual(expected.RsStartDate, actual.StartDate);
                Assert.AreEqual(expected.RsEndDate, actual.StartDate);
                Assert.AreEqual(expected.RsStartDate, actual.EndDate);
                Assert.AreEqual(expected.RsStatus, actual.Status);
            }
        }

        [TestMethod]
        public async Task PersonGuardinRelationships_GetAByIdAsync()
        {
            var expected = relationshipDataContracts[0];
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(people);

            var actual = await relationshipRepo.GetPersonGuardianRelationshipByIdAsync(personRelationshipsId);

            Assert.AreEqual(expected.RecordGuid, actual.Guid);
            Assert.AreEqual(expected.RsId1, actual.PrimaryEntity);
            Assert.AreEqual(expected.RsId2, actual.OtherEntity);
            Assert.AreEqual(expected.RsRelationType, actual.RelationshipType);
            Assert.AreEqual(expected.RsStartDate, actual.StartDate);
            Assert.AreEqual(expected.RsEndDate, actual.StartDate);
            Assert.AreEqual(expected.RsStartDate, actual.EndDate);
            Assert.AreEqual(expected.RsStatus, actual.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PersonGuardinRelationships_GetAByIdAsync_ArgumentNullException()
        {
            var actual = await relationshipRepo.GetPersonGuardianRelationshipByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonGuardinRelationships_GetAByIdAsync_KeyNotFoundException()
        {
            var actual = await relationshipRepo.GetPersonGuardianRelationshipByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonGuardinRelationships_GetAByIdAsync_DataContract_KeyNotFoundException()
        {
            dataReaderMock.Setup(i => i.ReadRecordAsync<Relationship>("RELATIONSHIP", "1", It.IsAny<bool>())).ReturnsAsync(null);
            var actual = await relationshipRepo.GetPersonGuardianRelationshipByIdAsync("1");
        }

        #endregion

        private Collection<Data.Base.DataContracts.Relationship> dataFromDataReader()
        {
            return new Collection<DataContracts.Relationship>()
                {
                    new Data.Base.DataContracts.Relationship(){Recordkey = "1", RsId1 = _parentId, RsId2 = _primaryId, RsRelationType = "C", RsEndDate = null, RsStartDate = null, RsPrimaryRelationshipFlag = "Y", RecordGuid = "58182363-9d54-4d47-812d-d02fc392be50"},
                    new Data.Base.DataContracts.Relationship(){Recordkey = "2", RsId1 = _primaryId, RsId2 = _childId, RsRelationType = "C", RsEndDate = null, RsStartDate = null, RsPrimaryRelationshipFlag = "Y", RecordGuid = "2d65dec8-f6f3-445d-ad7a-5f7574f0e626"},
                };
        }

        private Collection<DataContracts.Relation> GetRelationData()
        {
            return new Collection<DataContracts.Relation>()
            {
                new DataContracts.Relation(){Recordkey = _parentId + "*" + _primaryId, RelationComments = "Comment 1", RelationRelationships = new List<string>(){"C"}},
                new DataContracts.Relation(){Recordkey = _primaryId + "*" + _childId, RelationComments = "Comment 1", RelationRelationships = new List<string>(){"C"}}
            };
        }

        #region NonPersonRelationship
        [TestMethod]
        public async Task NonPersonalRelationships_GetNonPersonRelationshipsAsync_OrgId()
        {
            var results = await relationshipRepo.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), "1", It.IsAny<string>(), "1",
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonPersonalRelationships_GetNonPersonRelationshipsAsync_InstId_NotNull()
        {
            var results = await relationshipRepo.GetNonPersonRelationshipsAsync(It.IsAny<int>(), It.IsAny<int>(), "4", "4", "1",
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonPersonalRelationships_GetNonPersonRelationshipsAsync()
        {
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(nonPersonRelpeople);

            var results = await relationshipRepo.GetNonPersonRelationshipsAsync(0, 2, "4", "2", "1",
                "RelType", It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NonPersonalRelationships_GetNonPersonRelationshipsByIdAsync_ArgumentNullException()
        {
            var results = await relationshipRepo.GetNonPersonRelationshipByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task NonPersonalRelationships_GetNonPersonRelationshipsByIdAsync_KeyNotFoundException()
        {
            var results = await relationshipRepo.GetNonPersonRelationshipByIdAsync("ParentId");
        }

        [TestMethod]
        public async Task NonPersonalRelationships_GetNonPersonRelationshipsByIdAsync_KeyNotFoundException_Null_DataContract()
        {
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(nonPersonRelpeople);

            var results = await relationshipRepo.GetNonPersonRelationshipByIdAsync("1");
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task NonPersonalRelationships_GetNonPersonRelationshipsByIdAsync_KeyNotFoundException_RsId1_NotCorp()
        {
            nonPersonRelpeople.First().PersonCorpIndicator = "N";
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(nonPersonRelpeople);

            var results = await relationshipRepo.GetNonPersonRelationshipByIdAsync("1");
            Assert.IsNotNull(results);
        }
        #endregion

        public IRelationshipRepository BuildValidRelationshipRepository()
        {
            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

            dataReaderMock.Setup(i => i.SelectAsync("RELATIONSHIP", It.IsAny<string>())).ReturnsAsync(personRelationshipsIds);
            dataReaderMock.Setup(i => i.SelectAsync("RELATIONSHIP", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(personRelationshipsIds);
            dataReaderMock.Setup<Task<Collection<Relationship>>>(i => i.BulkReadRecordAsync<Relationship>("RELATIONSHIP", personRelationshipsIds, true)).ReturnsAsync(relationshipDataContracts);
            dataReaderMock.Setup<Task<Collection<Relation>>>(i => i.BulkReadRecordAsync<Relation>("RELATION", It.IsAny<string>(), true)).ReturnsAsync(relationContracts);
            dataReaderMock.Setup(i => i.ReadRecordAsync<Relationship>("RELATIONSHIP", personRelationshipsId, true)).ReturnsAsync(relationshipDataContracts[0]);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Institutions>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_institutionsDataContracts);

            IEnumerable<string> sublist = new List<string>() { _parentId, _primaryId, _childId };
            Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
            recordKeyLookupResults.Add("PERSON+ParentId", new RecordKeyLookupResult() { Guid = "854da721-4191-4875-bf58-7d6c00ffea8f", ModelName = "persons" });
            recordKeyLookupResults.Add("PERSON+PrimaryId", new RecordKeyLookupResult() { Guid = "71e1a806-24a8-4d93-91a2-02d86056b63c", ModelName = "persons" });
            recordKeyLookupResults.Add("PERSON+ChildId", new RecordKeyLookupResult() { Guid = "61e1a806-24a8-4d93-91a2-02d86056b63c", ModelName = "persons" });
            List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

            dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new[] { _parentId, _primaryId, _childId });
            dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var rel = dataFromDataReader().FirstOrDefault(x => x.Recordkey == gl.Guid);
                    result.Add(gl.Guid, rel == null ? null : new GuidLookupResult() { Entity = "RELATIONSHIP", PrimaryKey = rel.Recordkey });
                }
                return Task.FromResult(result);
            });

            // Build  repository
            relationshipRepo = new RelationshipRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return relationshipRepo;
        }
        #endregion
    }

    

    [TestClass]
    public class RelationshipRepositoryTests_PUT_V16 : BaseRepositorySetup
    {
        #region DECLARATIONS

        private RelationshipRepository relationshipRepository;

        private Domain.Base.Entities.Relationship relationship;
        private Relationship entityRelationship;
        private Relation relation;
        private UpdatePersonRelationshipResponse response;
        private Collection<Person> persons;

        private Dictionary<string, GuidLookupResult> dicResult;

        private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            InitializeTestData();

            InitializeTestMock();

            relationshipRepository = new RelationshipRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }

        private void InitializeTestData()
        {
            persons = new Collection<Person>()
            {
                new Person(){ RecordGuid = "1b49eed8-5fe7-4120-b1cf-f23266b9e874", Recordkey = "1", Gender = "M"},
                new Person(){ RecordGuid = "1c49eed8-5fe7-4120-b1cf-f23266b9e874", Recordkey = "2", Gender = "F"}
            };

            relationship = new Domain.Base.Entities.Relationship("1", "2", "1", true, null, null) { Guid = guid };

            relation = new Relation() { Recordkey = "1", RelationComments = "comments" };

            dicResult = new Dictionary<string, GuidLookupResult>()
            {
                {"1", new GuidLookupResult() { Entity = "RELATIONSHIP", PrimaryKey = "1" } }
            };

            entityRelationship = new Relationship()
            {
                RsId1 = "1",
                RsId2 = "2",
                RecordGuid = guid,
                Recordkey = "1",
                RsRelationType = "1",
                RsPrimaryRelationshipFlag = "Y",
            };

            response = new UpdatePersonRelationshipResponse() { RelationshipGuid = guid };
        }

        private void InitializeTestMock()
        {
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonRelationshipRequest, UpdatePersonRelationshipResponse>(It.IsAny<UpdatePersonRelationshipRequest>())).ReturnsAsync(response);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).ReturnsAsync(persons);
            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
            dataReaderMock.Setup(d => d.ReadRecordAsync<Relationship>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(entityRelationship);
            dataReaderMock.Setup(r => r.ReadRecordAsync<Relation>(It.IsAny<string>(), true)).ReturnsAsync(relation);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task RelationshipRepository_UpdatePersonRelationshipsAsync_Entity_Null()
        {
            await relationshipRepository.UpdatePersonalRelationshipsAsync(null);
        }


        [TestMethod]
        public async Task RelationshipRepository_Create_With_UpdatePersonRelationshipsAsync()
        {
            dicResult = new Dictionary<string, GuidLookupResult>()
            {
                {"1", new GuidLookupResult() { Entity = "RELATIONSHIP", PrimaryKey = null } }
            };
            var dicResult1 = new Dictionary<string, GuidLookupResult>()
            {
                {"1", new GuidLookupResult() { Entity = "RELATIONSHIP", PrimaryKey = "1" } }
            };
            dataReaderMock.SetupSequence(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).Returns(Task.FromResult<Dictionary<string, GuidLookupResult>>(dicResult1))
                .Returns(Task.FromResult<Dictionary<string, GuidLookupResult>>(dicResult1));
            var result = await relationshipRepository.UpdatePersonalRelationshipsAsync(relationship);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Guid, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task RelationshipRepository_Update_Returns_Invalid_Response_With_ErrorCodes()
        {
            response = new UpdatePersonRelationshipResponse()
            {
                ErrorOccurred = true,
                PersonRelationshipErrors = new List<PersonRelationshipErrors>()
                {
                    new PersonRelationshipErrors() { ErrorCodes = "400", ErrorMessages = "ErrorMessages"}
                }
            };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonRelationshipRequest, UpdatePersonRelationshipResponse>(It.IsAny<UpdatePersonRelationshipRequest>())).ReturnsAsync(response);
            await relationshipRepository.UpdatePersonalRelationshipsAsync(relationship);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task RelationshipRepository_Update_Returns_Invalid_Response_WithOut_ErrorCodes()
        {
            response = new UpdatePersonRelationshipResponse()
            {
                ErrorOccurred = true,
                PersonRelationshipErrors = new List<PersonRelationshipErrors>()
                {
                    new PersonRelationshipErrors() { ErrorCodes = "400", ErrorMessages = "ErrorMessages"}
                }
            };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonRelationshipRequest, UpdatePersonRelationshipResponse>(It.IsAny<UpdatePersonRelationshipRequest>())).ReturnsAsync(response);
            await relationshipRepository.UpdatePersonalRelationshipsAsync(relationship);
        }

        [TestMethod]
        public async Task RelationshipRepository_UpdatePersonRelationshipsAsync()
        {
            response = new UpdatePersonRelationshipResponse() { RelationshipGuid = guid };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdatePersonRelationshipRequest, UpdatePersonRelationshipResponse>(It.IsAny<UpdatePersonRelationshipRequest>())).ReturnsAsync(response);
            var result = await relationshipRepository.UpdatePersonalRelationshipsAsync(relationship);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Guid, guid);
        }
    }

    [TestClass]
    public class RelationshipRepositoryTests_DELETE_V13 : BaseRepositorySetup
    {
        #region DECLARATIONS

        private RelationshipRepository relationshipRepository;

        private DeleteRelationshipResponse response;

        private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            relationshipRepository = new RelationshipRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }

        #endregion

        [TestMethod]
        public async Task RelationshipRepository_Delete_Response_With_Errors_As_Null()
        {
            var response = new DeleteRelationshipResponse()
            {
                DeleteRelationshipErrors = null
            };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<DeleteRelationshipRequest, DeleteRelationshipResponse>(It.IsAny<DeleteRelationshipRequest>())).ReturnsAsync(response);
            var result = await relationshipRepository.DeletePersonRelationshipAsync(guid);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RelationshipRepository_Delete_Response_With_Empty_Errors()
        {
            var response = new DeleteRelationshipResponse()
            {
                DeleteRelationshipErrors = new List<DeleteRelationshipErrors>() { }
            };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<DeleteRelationshipRequest, DeleteRelationshipResponse>(It.IsAny<DeleteRelationshipRequest>())).ReturnsAsync(response);
            var result = await relationshipRepository.DeletePersonRelationshipAsync(guid);

            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task RelationshipRepository_Delete_Response_Without_ErrorCode()
        {
            var response = new DeleteRelationshipResponse()
            {
                DeleteRelationshipErrors = new List<DeleteRelationshipErrors>()
                {
                    new DeleteRelationshipErrors() { ErrorCode = string.Empty, ErrorMsg = "error message"}
                }
            };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<DeleteRelationshipRequest, DeleteRelationshipResponse>(It.IsAny<DeleteRelationshipRequest>())).ReturnsAsync(response);
            await relationshipRepository.DeletePersonRelationshipAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task RelationshipRepository_Delete_Response_With_ErrorCode()
        {
            var response = new DeleteRelationshipResponse()
            {
                DeleteRelationshipErrors = new List<DeleteRelationshipErrors>()
                {
                    new DeleteRelationshipErrors() { ErrorCode = "400", ErrorMsg = "error message"}
                }
            };
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<DeleteRelationshipRequest, DeleteRelationshipResponse>(It.IsAny<DeleteRelationshipRequest>())).ReturnsAsync(response);
            await relationshipRepository.DeletePersonRelationshipAsync(guid);
        }
    }
}
