//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using System.Collections;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class MealPlanRequestsServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewMealPlanRequest = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.MEAL.PLAN.REQUEST");
            protected Ellucian.Colleague.Domain.Entities.Role createMealPlanRequest = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.MEAL.PLAN.REQUEST");

            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Samwise",
                            PersonId = "STU1",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            // Represents a third party system like ILP
            public class ThirdPartyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "ILP",
                            PersonId = "ILP",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "ILP",
                            Roles = new List<string>() { "VIEW.MEAL.PLAN.REQUEST", "CREATE.MEAL.PLAN.REQUEST" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class MealPlanRequestsServiceTests_GET_GETALL : CurrentUserSetup
        {
            private const string mealPlanRequestsGuid = "2cb5e697-8168-4203-b48b-c667556cfb8a";
            private const string mealPlanRequestsCode = "AT";
            private List<MealPlanReqsIntg> _mealPlanRequestsCollection;
            private Tuple<IEnumerable<MealPlanReqsIntg>, int> mealPlanRequestsTuple;
            private ICurrentUserFactory currentUserFactory;

            private MealPlanRequestsService _mealPlanRequestsService;
            private Mock<ILogger> _loggerMock;
            private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
            private Mock<IMealPlanReqsIntgRepository> _mealPlanReqsIntgRepositoryMock;
            private Mock<ITermRepository> _termRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _mealPlanReqsIntgRepositoryMock = new Mock<IMealPlanReqsIntgRepository>();
                _termRepositoryMock = new Mock<ITermRepository>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _loggerMock = new Mock<ILogger>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                BuildData();

                _mealPlanRequestsService = new MealPlanRequestsService(_studentReferenceRepositoryMock.Object, _mealPlanReqsIntgRepositoryMock.Object, _termRepositoryMock.Object,
                    _personRepositoryMock.Object, baseConfigurationRepository, _adapterRegistryMock.Object, currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);
            }

            private void BuildData()
            {
                List<Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod> acadPeriods = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "2016 Spring", new DateTime(2016, 01, 01), new DateTime(2016, 05, 01), 2016, 1, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("7f3aac22-e0b5-4159-b4e2-da158362c41b", "2016/Fall", "2016 Fall", new DateTime(2016, 09, 01), new DateTime(2016, 10, 15), 2016, 2, "Fall", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("8f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Spr", "2017 Spring", new DateTime(2017, 01, 01), new DateTime(2017, 05, 01), 2017, 3, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("9f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Fall", "2017 Fall", new DateTime(2017, 09, 01), new DateTime(2017, 12, 31), 2017, 4, "Fall", "", "", null)
                };

                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);
                foreach (var record in acadPeriods)
                {
                    _termRepositoryMock.Setup(r => r.GetAcademicPeriodsGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                List<Domain.Student.Entities.MealPlan> mealPlans = new List<MealPlan>()
                {
                    new Domain.Student.Entities.MealPlan("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.MealPlan("949e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.MealPlan("e2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);
                foreach (var record in mealPlans)
                {
                    _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                _personRepositoryMock.SetupSequence(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("2f8d6c4d-86bb-44cd-b506-d955ef797270"))
                    .Returns(Task.FromResult("cbc3dfdd-0cb6-4499-a61c-738be8fcf406"))
                    .Returns(Task.FromResult("288a0b18-83a3-4f74-869c-4e0b20d24c01"))
                    .Returns(Task.FromResult("287ef0ad-b0cd-45de-980f-6d49eb0c0a30"));

                _mealPlanRequestsCollection = new List<MealPlanReqsIntg>()
                {
                    new MealPlanReqsIntg("2cb5e697-8168-4203-b48b-c667556cfb8a", "1", "11", "AT"){ Term = "2016/Spr", Status = "A", EndDate = DateTime.Today.AddDays(45), StartDate = DateTime.Today, StatusDate = DateTime.Today, SubmittedDate = DateTime.Today.AddDays(1)},
                    new MealPlanReqsIntg("fe472dd0-d4e5-4b0d-a870-de8452d1fe22", "2", "12", "AC"){ Term = "2017/Spr", Status = "R"},
                    new MealPlanReqsIntg("64d2c15e-e771-4639-81ab-2c01ad00e294", "3", "13", "CU"){ Status = "S"},
                    new MealPlanReqsIntg("e4d2c15e-e771-4639-81ab-2c01ad00e29t", "4", "14", "CU"){ Status = "W"}
                };
                mealPlanRequestsTuple = new Tuple<IEnumerable<MealPlanReqsIntg>, int>(_mealPlanRequestsCollection, _mealPlanRequestsCollection.Count());
                _mealPlanReqsIntgRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(mealPlanRequestsTuple);

                var personGuidDictionary = new Dictionary<string, string>() { };
                personGuidDictionary.Add("11", "1dd56e2d-9b99-4a5b-ab84-55131a31f2e3");
                personGuidDictionary.Add("12", "a7cbdbbe-131e-4b91-9c99-d9b65c41f1c8");
                personGuidDictionary.Add("13", "ae91ddf9-0b25-4008-97c5-76ac5fe570a3");
                personGuidDictionary.Add("14", "149195b8-fe43-4538-aa90-16fbe240a2d5");

                _personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>()))
                    .ReturnsAsync(personGuidDictionary);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _mealPlanRequestsService = null;
                _mealPlanRequestsCollection = null;
                currentUserFactory = null;
                _studentReferenceRepositoryMock = null;
                _mealPlanReqsIntgRepositoryMock = null;
                _termRepositoryMock = null;
                _personRepositoryMock = null;
                _adapterRegistryMock = null;
                _roleRepositoryMock = null;
                _loggerMock = null;
                mealPlanRequestsTuple = null;
            }

            [TestMethod]
            public async Task MealPlanRequestsService_GetMealPlanRequestsAsync()
            {
                viewMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanRequest));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlanRequest });

                var actuals = await _mealPlanRequestsService.GetMealPlanRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = _mealPlanRequestsCollection.FirstOrDefault(i => i.Guid.Equals(actual.Id));
                    Assert.IsNotNull(expected);
                }
            }

            [TestMethod]
            public async Task MealPlanRequestsService_GetMealPlanRequestsByGuidAsync_Expected()
            {
                viewMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanRequest));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlanRequest });

                var expectedResults =
                    _mealPlanRequestsCollection.First(c => c.Guid == mealPlanRequestsGuid);
                _mealPlanReqsIntgRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expectedResults);

                var actualResult =
                    await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(mealPlanRequestsGuid, false);
                Assert.IsNotNull(actualResult);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task MealPlanRequestsService_GetMealPlanRequestByGuidAsync_ArgumentNullException()
            {
                var actualResult =
                   await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(null, false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task MealPlanRequestsService_GetMealPlanRequestByGuidAsync_KeyNotFoundException()
            {
                viewMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanRequest));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlanRequest });

                _mealPlanReqsIntgRepositoryMock.Setup(repo => repo.GetByIdAsync(mealPlanRequestsGuid)).ThrowsAsync(new KeyNotFoundException());
                var actualResult =
                   await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(mealPlanRequestsGuid, false);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task MealPlanRequestsService_GetMealPlanRequestByGuidAsync_InvalidOperationException()
            {
                viewMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanRequest));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlanRequest });

                _mealPlanReqsIntgRepositoryMock.Setup(repo => repo.GetByIdAsync(mealPlanRequestsGuid)).ThrowsAsync(new InvalidOperationException());
                var actualResult =
                   await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(mealPlanRequestsGuid, false);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task MealPlanRequestsService_GetMealPlanRequestsByGuidAsync_AcademicPeriodEntities_Null_Exception()
            {
                viewMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanRequest));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlanRequest });

                var expectedResults =
                    _mealPlanRequestsCollection.First(c => c.Guid == mealPlanRequestsGuid);
                _mealPlanReqsIntgRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expectedResults);

                //List<Domain.Student.Entities.AcademicPeriod> acadPeriods = null;
                //_termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                var actualResult =
                    await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(mealPlanRequestsGuid, false);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task MealPlanRequestsService_GetMealPlanRequestsByGuidAsync_InvalidTerm_Exception()
            {
                viewMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanRequest));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlanRequest });

                var expectedResults =
                    _mealPlanRequestsCollection.First(c => c.Guid == mealPlanRequestsGuid);
                _mealPlanReqsIntgRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expectedResults);

                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                expectedResults.Term = "BadTerm";
                var actualResult =
                    await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(mealPlanRequestsGuid, false);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task MealPlanRequestsService_GetMealPlanRequestsByGuidAsync_MealPlans_Null_Exception()
            {
                viewMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanRequest));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlanRequest });

                var expectedResults =
                    _mealPlanRequestsCollection.First(c => c.Guid == mealPlanRequestsGuid);
                _mealPlanReqsIntgRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expectedResults);

                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                var actualResult =
                    await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(mealPlanRequestsGuid, false);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task MealPlanRequestsService_GetMealPlanRequestsByGuidAsync_InvalidMealPlan_Exception()
            {
                viewMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanRequest));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlanRequest });

                var expectedResults = new MealPlanReqsIntg("2cb5e697-8168-4203-b48b-c667556cfb8a", "1", "11", "BadData") { Term = "2016/Spr", Status = "A", EndDate = DateTime.Today.AddDays(45), StartDate = DateTime.Today, StatusDate = DateTime.Today, SubmittedDate = DateTime.Today.AddDays(1) };
                _mealPlanReqsIntgRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expectedResults);

                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                var actualResult =
                    await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(mealPlanRequestsGuid, false);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task MealPlanRequestsService_GetMealPlanRequestsByGuidAsync_InvalidPersonId_Exception()
            {
                viewMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanRequest));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlanRequest });

                _personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("");

                var expectedResults = new MealPlanReqsIntg("2cb5e697-8168-4203-b48b-c667556cfb8a", "1", "1111", "AT") { Term = "2016/Spr", Status = "A", EndDate = DateTime.Today.AddDays(45), StartDate = DateTime.Today, StatusDate = DateTime.Today, SubmittedDate = DateTime.Today.AddDays(1) };
                _mealPlanReqsIntgRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expectedResults);

                var actualResult =
                    await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(mealPlanRequestsGuid, false);
            }
        }

        [TestClass]
        public class MealPlanRequestsServiceTests_POST : CurrentUserSetup
        {
            #region DECLARATIONS

            private ICurrentUserFactory currentUserFactory;

            private MealPlanRequestsService mealPlanRequestsService;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceRepositoryMock;
            private Mock<IMealPlanReqsIntgRepository> mealPlanReqsIntgRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private List<MealPlanReqsIntg> mealPlanRequestsCollection;
            private List<Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod> acadPeriods;
            private List<Domain.Student.Entities.MealPlan> mealPlans;
            private MealPlanRequests mealPlanRequest;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                mealPlanReqsIntgRepositoryMock = new Mock<IMealPlanReqsIntgRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                BuildData();

                mealPlanRequestsService = new MealPlanRequestsService(studentReferenceRepositoryMock.Object, mealPlanReqsIntgRepositoryMock.Object, termRepositoryMock.Object,
                    personRepositoryMock.Object, baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            private void BuildData()
            {
                mealPlanRequest = new MealPlanRequests()
                {
                    Id = "2cb5e697-8168-4203-b48b-c667556cfb8a",
                    Person = new GuidObject2("2f8d6c4d-86bb-44cd-b506-d955ef797270"),
                    MealPlan = new GuidObject2("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    AcademicPeriod = new GuidObject2("b9691210-8516-45ca-9cd1-7e5aa1777234"),
                    Status = Dtos.EnumProperties.MealPlanRequestsStatus.Submitted
                };

                acadPeriods = new List<Domain.Student.Entities.AcademicPeriod>() {
                    new Domain.Student.Entities.AcademicPeriod("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "2016 Spring", new DateTime(2016, 01, 01), new DateTime(2016, 05, 01), 2016, 1, "Spring", "", "", null)
                };

                mealPlans = new List<MealPlan>() { new Domain.Student.Entities.MealPlan("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic") };

                personRepositoryMock.SetupSequence(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("2f8d6c4d-86bb-44cd-b506-d955ef797270"));

                mealPlanRequestsCollection = new List<MealPlanReqsIntg>()
                {
                    new MealPlanReqsIntg("e4d2c15e-e771-4639-81ab-2c01ad00e29t", "5", "14", "UC"){ Status = "W"},
                    new MealPlanReqsIntg("2cb5e697-8168-4203-b48b-c667556cfb8a", "1", "11", "AT"){ Term = "2016/Spr", Status = "S", StartDate = DateTime.Today, StatusDate = DateTime.Today, SubmittedDate = DateTime.Today.AddDays(1)}
                };

            }

            [TestCleanup]
            public void Cleanup()
            {
                mealPlanRequestsService = null;
                currentUserFactory = null;
                studentReferenceRepositoryMock = null;
                mealPlanReqsIntgRepositoryMock = null;
                termRepositoryMock = null;
                personRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                mealPlanRequestsCollection = null;
            }

            #endregion

            #region EXCEPTIONS

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequests_PostMealPlanRequestsAsync_ArgumentNullException()
            {
                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequests_PostMealPlanRequestsAsync_ArgumentNullException_When_Id_Is_Null()
            {
                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(new MealPlanRequests() { Id = null });
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_PermissionsException()
            {
                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(new MealPlanRequests() { Id = Guid.NewGuid().ToString() });

            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_Person_Null()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });

                mealPlanRequest.Person = null;

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_Person_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_MeanPlan_Null()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");

                mealPlanRequest.MealPlan = null;

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            /**
             * Mocking of function "GetMealPlansAsync" returning empty list, It should return null.
             * Modified code to check empty list " File: MealPlanRequestsService.cs, Line: 362
             **/
            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_MeanPlans_Empty()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(true)).ReturnsAsync((IEnumerable<Ellucian.Colleague.Domain.Student.Entities.MealPlan>)null);

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_MeanPlan_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                mealPlanRequest.MealPlan = new GuidObject2(Guid.NewGuid().ToString());

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            /**
            * Mocking of function "GetAcademicPeriods" returning empty list, It should return null.
            * Modified code to check empty list " File: MealPlanRequestsService.cs, Line: 402
            **/
            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_AcademicPeriods_Empty()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(null)).Returns((IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod>)null);

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_AcademicPeriod_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                mealPlanRequest.AcademicPeriod = new GuidObject2(Guid.NewGuid().ToString());

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_Status_NotSet()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                mealPlanRequest.Status = Dtos.EnumProperties.MealPlanRequestsStatus.NotSet;

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_RepositoryException()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.CreateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ThrowsAsync(new RepositoryException());

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            /**
             * This test is not working because of academic periods cache.
             **/
            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Domain_Dto_Exception_When_AcademicPeriods_Empty()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.SetupSequence(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>()))
                                  .Returns(acadPeriods)
                                  .Returns((IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod>)null);

                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.CreateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ReturnsAsync(mealPlanRequestsCollection.LastOrDefault());

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Domain_Dto_Exception_When_AcademicPeriod_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                var record = mealPlanRequestsCollection.LastOrDefault();
                record.Term = Guid.NewGuid().ToString();

                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.CreateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ReturnsAsync(record);

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Domain_Dto_Exception_When_MealPlan_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.CreateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ReturnsAsync(mealPlanRequestsCollection.FirstOrDefault());

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync_Convert_Domain_Dto_Exception_When_Person_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                var record = mealPlanRequestsCollection.LastOrDefault();

                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.CreateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ReturnsAsync(record);

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);
            }

            #endregion

            [TestMethod]
            public async Task MealPlanRequestsService_PostMealPlanRequestsAsync()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("2f8d6c4d-86bb-44cd-b506-d955ef797270");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                var record = mealPlanRequestsCollection.LastOrDefault();


                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.CreateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ReturnsAsync(record);

                var actualResult = await mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequest);

                Assert.AreEqual(mealPlanRequest.Id, actualResult.Id);
                Assert.AreEqual(mealPlanRequest.MealPlan.Id, actualResult.MealPlan.Id);
                Assert.AreEqual(mealPlanRequest.Person.Id, actualResult.Person.Id);
                Assert.AreEqual(mealPlanRequest.Status, actualResult.Status);
                Assert.AreEqual(mealPlanRequest.AcademicPeriod.Id, actualResult.AcademicPeriod.Id);
                Assert.AreEqual(mealPlanRequest.EndOn, actualResult.EndOn);
            }
        }

        [TestClass]
        public class MealPlanRequestsServiceTests_PUT : CurrentUserSetup
        {
            #region DECLARATIONS

            private ICurrentUserFactory currentUserFactory;

            private MealPlanRequestsService mealPlanRequestsService;
            private Mock<ILogger> loggerMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceRepositoryMock;
            private Mock<IMealPlanReqsIntgRepository> mealPlanReqsIntgRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private List<MealPlanReqsIntg> mealPlanRequestsCollection;
            private List<Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod> acadPeriods;
            private List<Domain.Student.Entities.MealPlan> mealPlans;
            private MealPlanRequests mealPlanRequest;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                mealPlanReqsIntgRepositoryMock = new Mock<IMealPlanReqsIntgRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                BuildData();

                mealPlanRequestsService = new MealPlanRequestsService(studentReferenceRepositoryMock.Object, mealPlanReqsIntgRepositoryMock.Object, termRepositoryMock.Object,
                    personRepositoryMock.Object, baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            private void BuildData()
            {
                mealPlanRequest = new MealPlanRequests()
                {
                    Id = "2cb5e697-8168-4203-b48b-c667556cfb8a",
                    Person = new GuidObject2("2f8d6c4d-86bb-44cd-b506-d955ef797270"),
                    MealPlan = new GuidObject2("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    AcademicPeriod = new GuidObject2("b9691210-8516-45ca-9cd1-7e5aa1777234"),
                    Status = Dtos.EnumProperties.MealPlanRequestsStatus.Submitted
                };

                acadPeriods = new List<Domain.Student.Entities.AcademicPeriod>() {
                    new Domain.Student.Entities.AcademicPeriod("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "2016 Spring", new DateTime(2016, 01, 01), new DateTime(2016, 05, 01), 2016, 1, "Spring", "", "", null)
                };

                mealPlans = new List<MealPlan>() { new Domain.Student.Entities.MealPlan("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic") };

                personRepositoryMock.SetupSequence(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult("2f8d6c4d-86bb-44cd-b506-d955ef797270"));

                mealPlanRequestsCollection = new List<MealPlanReqsIntg>()
                {
                    new MealPlanReqsIntg("e4d2c15e-e771-4639-81ab-2c01ad00e29t", "5", "14", "UC"){ Status = "W"},
                    new MealPlanReqsIntg("2cb5e697-8168-4203-b48b-c667556cfb8a", "1", "11", "AT"){ Term = "2016/Spr", Status = "S", StartDate = DateTime.Today, StatusDate = DateTime.Today, SubmittedDate = DateTime.Today.AddDays(1)}
                };

            }

            [TestCleanup]
            public void Cleanup()
            {
                mealPlanRequestsService = null;
                currentUserFactory = null;
                studentReferenceRepositoryMock = null;
                mealPlanReqsIntgRepositoryMock = null;
                termRepositoryMock = null;
                personRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                mealPlanRequestsCollection = null;
            }

            #endregion

            #region EXCEPTIONS

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequests_PutMealPlanRequestsAsync_ArgumentNullException()
            {
                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequests_PutMealPlanRequestsAsync_ArgumentNullException_When_Id_Is_Null()
            {
                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), new MealPlanRequests() { Id = null });
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_PermissionsException()
            {
                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), new MealPlanRequests() { Id = Guid.NewGuid().ToString() });
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_Person_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_Person_Null()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });

                mealPlanRequest.Person = null;

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_MeanPlan_Null()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");

                mealPlanRequest.MealPlan = null;

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_MeanPlans_Empty()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(true)).ReturnsAsync((IEnumerable<Ellucian.Colleague.Domain.Student.Entities.MealPlan>)null);

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_MeanPlan_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                mealPlanRequest.MealPlan = new GuidObject2(Guid.NewGuid().ToString());

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_AcademicPeriods_Empty()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(null)).Returns((IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod>)null);

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_AcademicPeriod_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                mealPlanRequest.AcademicPeriod = new GuidObject2(Guid.NewGuid().ToString());

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Dto_Domain_Exception_When_Status_NotSet()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                mealPlanRequest.Status = Dtos.EnumProperties.MealPlanRequestsStatus.NotSet;

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_RepositoryException()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.UpdateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ThrowsAsync(new RepositoryException());

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Domain_Dto_Exception_When_AcademicPeriods_Empty()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.SetupSequence(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>()))
                                  .Returns(acadPeriods)
                                  .Returns((IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod>)null);

                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.UpdateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ReturnsAsync(mealPlanRequestsCollection.LastOrDefault());

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Domain_Dto_Exception_When_AcademicPeriod_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                var record = mealPlanRequestsCollection.LastOrDefault();
                record.Term = Guid.NewGuid().ToString();

                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.UpdateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ReturnsAsync(record);

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Domain_Dto_Exception_When_MealPlan_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.UpdateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ReturnsAsync(mealPlanRequestsCollection.FirstOrDefault());

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync_Convert_Domain_Dto_Exception_When_Person_NotFound()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                var record = mealPlanRequestsCollection.LastOrDefault();

                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.UpdateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ReturnsAsync(record);

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);
            }

            #endregion

            [TestMethod]
            public async Task MealPlanRequestsService_PutMealPlanRequestsAsync()
            {
                createMealPlanRequest.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanRequest));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createMealPlanRequest });
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("16");
                personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("2f8d6c4d-86bb-44cd-b506-d955ef797270");
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(new List<Term>());
                studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlans);

                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Domain.Student.Entities.Term>>())).Returns(acadPeriods);

                var record = mealPlanRequestsCollection.LastOrDefault();


                mealPlanReqsIntgRepositoryMock.Setup(repo => repo.UpdateMealPlanReqsIntgAsync(It.IsAny<MealPlanReqsIntg>())).ReturnsAsync(record);

                var actualResult = await mealPlanRequestsService.PutMealPlanRequestsAsync(It.IsAny<string>(), mealPlanRequest);

                Assert.AreEqual(mealPlanRequest.Id, actualResult.Id);
                Assert.AreEqual(mealPlanRequest.MealPlan.Id, actualResult.MealPlan.Id);
                Assert.AreEqual(mealPlanRequest.Person.Id, actualResult.Person.Id);
                Assert.AreEqual(mealPlanRequest.Status, actualResult.Status);
                Assert.AreEqual(mealPlanRequest.AcademicPeriod.Id, actualResult.AcademicPeriod.Id);
                Assert.AreEqual(mealPlanRequest.EndOn, actualResult.EndOn);
            }
        }
    }
}