using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
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
using Ellucian.Colleague.Domain.Base.Transactions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class MealPlanReqsIntgRepositoryTests : BaseRepositorySetup
    {
        Mock<IColleagueDataReader> dataAccessorMock;
        private ICollection<Domain.Student.Entities.MealPlanReqsIntg> _mealPlanRequestsCollection;
        private string[] ids = new string[] { "1", "2", "3", "4" };
        int offset = 0;
        int limit = 4;

        private Collection<DataContracts.MealPlanReqsIntg> records;

        MealPlanReqsIntgRepository _mealPlanRequestsRepository;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            BuildData();
            _mealPlanRequestsRepository = BuildMealPlanRequestsRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mealPlanRequestsCollection = null;
            records = null;
            _mealPlanRequestsRepository = null;
            dataAccessorMock = null;
        }

        [TestMethod]
        public async Task MealPlanRequestsRepository_GET()
        {
            dataAccessorMock.Setup(repo => repo.SelectAsync("MEAL.PLAN.REQS.INTG", It.IsAny<string>())).ReturnsAsync(ids);
            var results = await _mealPlanRequestsRepository.GetAsync(offset, limit, It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task MealPlanRequestsRepository_GET_ById()
        {
            dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.MealPlanReqsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(records.First());
            var result = await _mealPlanRequestsRepository.GetByIdAsync("2cb5e697-8168-4203-b48b-c667556cfb8a");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task MealPlanRequestsRepository_GET_ExpectedEmptySet()
        {
            dataAccessorMock.Setup(repo => repo.SelectAsync("MEAL.PLAN.REQS.INTG", It.IsAny<string>())).ReturnsAsync(ids);
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.MealPlanReqsIntg>("MEAL.PLAN.REQS.INTG", It.IsAny<string[]>(), true)).ReturnsAsync(() => null);

            var results = await _mealPlanRequestsRepository.GetAsync(offset, limit, It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(results.Item2, 0);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MealPlanRequestsRepository_GET_ById_ArgumentNullException()
        {
            dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.MealPlanReqsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(records.First());
            var result = await _mealPlanRequestsRepository.GetByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MealPlanRequestsRepository_GET_ById_KeyNotFoundException()
        {
            dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.MealPlanReqsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(records.First());
            var result = await _mealPlanRequestsRepository.GetByIdAsync("1cb5e697-8168-4203-b48b-c667556cfb8b");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task MealPlanRequestsRepository_GET_ById_MealPlanReq_Null()
        {
            dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.MealPlanReqsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
            var result = await _mealPlanRequestsRepository.GetByIdAsync("2cb5e697-8168-4203-b48b-c667556cfb8a");
        }

        private void BuildData()
        {
            _mealPlanRequestsCollection = new List<MealPlanReqsIntg>()
                {
                    new MealPlanReqsIntg("2cb5e697-8168-4203-b48b-c667556cfb8a", "1", "11", "AT"){ Term = "2016/Spr", Status = "A", EndDate = DateTime.Today.AddDays(45),
                        StartDate = DateTime.Today, StatusDate = DateTime.Today, SubmittedDate = DateTime.Today.AddDays(1)},
                    new MealPlanReqsIntg("fe472dd0-d4e5-4b0d-a870-de8452d1fe22", "2", "12", "AC"){ Term = "2017/Spr", Status = "R"},
                    new MealPlanReqsIntg("64d2c15e-e771-4639-81ab-2c01ad00e294", "3", "13", "CU"){ Status = "S"},
                    new MealPlanReqsIntg("e4d2c15e-e771-4639-81ab-2c01ad00e29t", "4", "14", "CU"){ Status = "W"}
                };

            string[] requestedIds1 = { "1", "2", "3" };

            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 100,
                CacheName = "AllMealPlanRequestsRecordKeys",
                Entity = "MEAL.PLAN.REQS.INTG",
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
        }

        private MealPlanReqsIntgRepository BuildMealPlanRequestsRepository()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();

            // Set up data accessor for mocking 
            dataAccessorMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            records = new Collection<DataContracts.MealPlanReqsIntg>();
            foreach (var item in _mealPlanRequestsCollection)
            {
                DataContracts.MealPlanReqsIntg record = new DataContracts.MealPlanReqsIntg();
                record.RecordGuid = item.Guid;
                record.Recordkey = item.Id;
                record.MpriEndDate = item.EndDate;
                record.MpriMealPlan = item.MealPlan;
                record.MpriPerson = item.PersonId;
                record.MpriStartDate = item.StartDate;
                record.MpriStatusesEntityAssociation = new List<DataContracts.MealPlanReqsIntgMpriStatuses>()
                {
                    new DataContracts.MealPlanReqsIntgMpriStatuses(){ MpriStatusAssocMember = "A", MpriStatusDateAssocMember = DateTime.Today }
                };
                records.Add(record);
            }
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.MealPlanReqsIntg>("MEAL.PLAN.REQS.INTG", It.IsAny<string[]>(), true)).ReturnsAsync(records);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var stuprog = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, stuprog == null ? null : new GuidLookupResult() { Entity = "MEAL.PLAN.REQS.INTG", PrimaryKey = stuprog.Recordkey });
                }
                return Task.FromResult(result);
            });

            // Set up transaction manager for mocking 
            var transManagerMock = new Mock<IColleagueTransactionInvoker>();
            var transManager = transManagerMock.Object;
            // Set up transManagerMock as the object for the transaction manager
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);


            // Construct repository
            _mealPlanRequestsRepository = new MealPlanReqsIntgRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return _mealPlanRequestsRepository;
        }

        [TestClass]
        public class MealPlanReqsIntgRepositoryTests_POST
        {
            #region DECLARATIONS

            MealPlanReqsIntgRepository mealPlanRequestsRepository;
            Mock<BaseColleagueRepository> baseColleagueRepositoryMock;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            IColleagueTransactionInvoker transManager;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;

            Domain.Student.Entities.MealPlanReqsIntg mealPlanRequest;
            DataContracts.MealPlanReqsIntg record;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                baseColleagueRepositoryMock = new Mock<BaseColleagueRepository>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();


                mealPlanRequestsRepository = BuildMealPlanReqsIntgRepository();

                BuildData();


            }

            private void BuildData()
            {
                mealPlanRequest = new MealPlanReqsIntg("2cb5e697-8168-4203-b48b-c667556cfb8a", "1", "11", "AT")
                {
                    Term = "2016/Spr",
                    Status = "A",
                    EndDate = DateTime.Today.AddDays(45),
                    StartDate = DateTime.Today,
                    StatusDate = DateTime.Today,
                    SubmittedDate = DateTime.Today.AddDays(1)
                };
                record = new DataContracts.MealPlanReqsIntg()
                {
                    RecordGuid = mealPlanRequest.Guid,
                    Recordkey = mealPlanRequest.Id,
                    MpriEndDate = mealPlanRequest.EndDate,
                    MpriMealPlan = mealPlanRequest.MealPlan,
                    MpriPerson = mealPlanRequest.PersonId,
                    MpriStartDate = mealPlanRequest.StartDate,
                    MpriStatusesEntityAssociation = new List<DataContracts.MealPlanReqsIntgMpriStatuses>()
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                mealPlanRequestsRepository = null;
            }

            private MealPlanReqsIntgRepository BuildMealPlanReqsIntgRepository()
            {
                transManager = transManagerMock.Object;
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);

                return new MealPlanReqsIntgRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task MealPlanRequestsRepository_CreateMealPlanReqsIntgAsync_ArgumentNullException()
            {
                var result = await mealPlanRequestsRepository.CreateMealPlanReqsIntgAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task MealPlanRequestsRepository_CreateMealPlanReqsIntgAsync_RepositoryException()
            {
                var response = new CreateUpdateMealPlanReqResponse()
                {
                    CreateUpdateMealPlanReqRequestErrors = new List<CreateUpdateMealPlanReqRequestErrors>()
                    {
                        new CreateUpdateMealPlanReqRequestErrors() { ErrorMessages = "ERROR.MESSAGE", ErrorCodes = "ERROR.CODE" }
                    }
                };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);
                var result = await mealPlanRequestsRepository.CreateMealPlanReqsIntgAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task MealPlanRequestsRepository_CreateMealPlanReqsIntgAsync_ArgumentNullException_When_Id_Is_NullOrEmpty_On_Get()
            {
                var response = new CreateUpdateMealPlanReqResponse() { Guid = "" };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);
                var result = await mealPlanRequestsRepository.CreateMealPlanReqsIntgAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task MealPlanRequestsRepository_CreateMealPlanReqsIntgAsync_KeyNotFoundException()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                   .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", null } }));

                var response = new CreateUpdateMealPlanReqResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);
                var result = await mealPlanRequestsRepository.CreateMealPlanReqsIntgAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task MealPlanRequestsRepository_CreateMealPlanReqsIntgAsync_KeyNotFoundException_When_MealPlan_NotFound()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                   .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "MEAL.PLAN.REQS.INTG", PrimaryKey = "KEY" } } }));

                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.MealPlanReqsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);

                var response = new CreateUpdateMealPlanReqResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);
                var result = await mealPlanRequestsRepository.CreateMealPlanReqsIntgAsync(mealPlanRequest);
            }

            [TestMethod]
            public async Task MealPlanRequestsRepository_CreateMealPlanReqsIntgAsync()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                   .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "MEAL.PLAN.REQS.INTG", PrimaryKey = "KEY" } } }));

                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.MealPlanReqsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(record);

                var response = new CreateUpdateMealPlanReqResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);
                var result = await mealPlanRequestsRepository.CreateMealPlanReqsIntgAsync(mealPlanRequest);

                Assert.IsNotNull(result);
            }
        }

        [TestClass]
        public class MealPlanReqsIntgRepositoryTests_PUT
        {
            #region DECLARATIONS

            MealPlanReqsIntgRepository mealPlanRequestsRepository;
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            IColleagueTransactionInvoker transManager;
            Mock<ILogger> loggerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;

            Domain.Student.Entities.MealPlanReqsIntg mealPlanRequest;
            DataContracts.MealPlanReqsIntg record;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();

                mealPlanRequestsRepository = BuildMealPlanReqsIntgRepository();

                BuildData();
            }

            private void BuildData()
            {
                mealPlanRequest = new MealPlanReqsIntg("2cb5e697-8168-4203-b48b-c667556cfb8a", "1", "11", "AT"){ Term = "2016/Spr", Status = "A", EndDate = DateTime.Today.AddDays(45),
                                                                                                   StartDate = DateTime.Today, StatusDate = DateTime.Today, SubmittedDate = DateTime.Today.AddDays(1)};
                record = new DataContracts.MealPlanReqsIntg()
                {
                    RecordGuid = mealPlanRequest.Guid,
                    Recordkey = mealPlanRequest.Id,
                    MpriEndDate = mealPlanRequest.EndDate,
                    MpriMealPlan = mealPlanRequest.MealPlan,
                    MpriPerson = mealPlanRequest.PersonId,
                    MpriStartDate = mealPlanRequest.StartDate,
                    MpriStatusesEntityAssociation = new List<DataContracts.MealPlanReqsIntgMpriStatuses>()
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                mealPlanRequestsRepository = null;
            }

            private MealPlanReqsIntgRepository BuildMealPlanReqsIntgRepository()
            {
                transManager = transManagerMock.Object;
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);

                return new MealPlanReqsIntgRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task MealPlanRequestsRepository_UpdateMealPlanReqsIntgAsync_ArgumentNullException()
            {
                var result = await mealPlanRequestsRepository.UpdateMealPlanReqsIntgAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task MealPlanRequestsRepository_UpdateMealPlanReqsIntgAsync_RepositoryException()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                   .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "MEAL.PLAN.REQS.INTG", PrimaryKey = "KEY" } } }));

                var response = new CreateUpdateMealPlanReqResponse()
                {
                    CreateUpdateMealPlanReqRequestErrors = new List<CreateUpdateMealPlanReqRequestErrors>()
                    {
                        new CreateUpdateMealPlanReqRequestErrors() { ErrorMessages = "ERROR.MESSAGE", ErrorCodes = "ERROR.CODE" }
                    }
                };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);

                var result = await mealPlanRequestsRepository.UpdateMealPlanReqsIntgAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task MealPlanRequestsRepository_UpdateMealPlanReqsIntgAsync_ArgumentNullException_When_Id_Is_NullOrEmpty_On_Get()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                   .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "MEAL.PLAN.REQS.INTG", PrimaryKey = "KEY" } } }));

                var response = new CreateUpdateMealPlanReqResponse() { Guid = "" };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);
                var result = await mealPlanRequestsRepository.UpdateMealPlanReqsIntgAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task MealPlanRequestsRepository_UpdateMealPlanReqsIntgAsync_KeyNotFoundException()
            {
                dataAccessorMock.SetupSequence(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "MEAL.PLAN.REQS.INTG", PrimaryKey = "KEY" } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", null } }));

                var response = new CreateUpdateMealPlanReqResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);
                var result = await mealPlanRequestsRepository.UpdateMealPlanReqsIntgAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task MealPlanRequestsRepository_UpdateMealPlanReqsIntgAsync_KeyNotFoundException_When_MealPlan_NotFound()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "MEAL.PLAN.REQS.INTG", PrimaryKey = "KEY" } } }));

                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.MealPlanReqsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null); 

                var response = new CreateUpdateMealPlanReqResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);
                var result = await mealPlanRequestsRepository.UpdateMealPlanReqsIntgAsync(mealPlanRequest);
            }


            [TestMethod]
            public async Task MealPlanRequestsRepository_UpdateMealPlanReqsIntgAsync_CreateMealPlanRequest()
            {
                dataAccessorMock.SetupSequence(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", null } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "MEAL.PLAN.REQS.INTG", PrimaryKey = "KEY" } } }));

                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.MealPlanReqsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(record);

                var response = new CreateUpdateMealPlanReqResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);
                var result = await mealPlanRequestsRepository.UpdateMealPlanReqsIntgAsync(mealPlanRequest);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task MealPlanRequestsRepository_UpdateMealPlanReqsIntgAsync()
            {
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "MEAL.PLAN.REQS.INTG", PrimaryKey = "KEY" } } }));

                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<DataContracts.MealPlanReqsIntg>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(record);

                var response = new CreateUpdateMealPlanReqResponse() { Guid = Guid.NewGuid().ToString() };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<CreateUpdateMealPlanReqRequest, CreateUpdateMealPlanReqResponse>(It.IsAny<CreateUpdateMealPlanReqRequest>())).ReturnsAsync(response);
                var result = await mealPlanRequestsRepository.UpdateMealPlanReqsIntgAsync(mealPlanRequest);

                Assert.IsNotNull(result);
            }
        }
    }
}
