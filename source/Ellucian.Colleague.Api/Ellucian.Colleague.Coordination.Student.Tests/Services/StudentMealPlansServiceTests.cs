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
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentMealPlansServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewMealPlan = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.MEAL.PLAN.ASSIGNMENT");
            protected Ellucian.Colleague.Domain.Entities.Role putPostMealPlan = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.MEAL.PLAN.ASSIGNMENT");

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
                            Roles = new List<string>() { "VIEW.MEAL.PLAN.ASSIGNMENT", "CREATE.MEAL.PLAN.ASSIGNMENT" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class StudentMealPlansService_GET_Tests : CurrentUserSetup
        {
            private const string studentMealPlansGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string studentMealPlansCode = "AT";
            int offset = 0;
            int limit = 100;

            private ICollection<MealPlanAssignment> _studentMealPlansCollection;
            private Tuple<IEnumerable<MealPlanAssignment>, int> _studentMealPlansCollectionTuple;
            private StudentMealPlansService _studentMealPlansService;
            private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
            private Mock<IMealPlanAssignmentRepository> _mealPlanAssignmentRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<ITermRepository> _termRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Dictionary<string, string> personGuidCollection;
            private Dtos.StudentMealPlans2 mealPlan;

            [TestInitialize]
            public void Initialize()
            {
                _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _mealPlanAssignmentRepositoryMock = new Mock<IMealPlanAssignmentRepository>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _termRepositoryMock = new Mock<ITermRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                BuildData();

                _studentMealPlansService = new StudentMealPlansService(_studentReferenceRepositoryMock.Object, _mealPlanAssignmentRepositoryMock.Object, _termRepositoryMock.Object,
                    _personRepositoryMock.Object, baseConfigurationRepository, _adapterRegistryMock.Object, currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);
            }

            private void BuildData()
            {
                mealPlan = new StudentMealPlans2()
                {
                    Id = "699d2457-a030-4990-8e30-e469878d156f",
                    AcademicPeriod = new GuidObject2("49ea04ce-d26e-42af-909c-07deefab2c68"),
                    AssignedRates = new List<GuidObject2>()
                    {
                        new GuidObject2("e2052590-62d1-4da5-9f75-df2d1906a123")
                    },
                    NumberOfPeriods = 5,
                    EndOn = DateTime.Today.AddDays(30),
                    MealPlan = new GuidObject2("10f4bf71-a6d6-444a-86d2-ed4a89af7791"),
                    Person = new GuidObject2("bde4f128-8fca-42a2-9881-1d276ee2269f"),
                    StartOn = DateTime.Today,
                    Status = Dtos.EnumProperties.StudentMealPlansStatus.Assigned,
                    StatusDate = DateTime.Today,
                    MealCard = "xyz meal card",
                    Comment = "Owens Food Court",
                    Consumption = new StudentMealPlansConsumption()
                    {
                        Units = 2
                    }
                };
                _studentMealPlansCollection = new List<MealPlanAssignment>()
                {                    
                    new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "LNCH", DateTime.Today, 5, "A", DateTime.Today)
                    {
                       EndDate = DateTime.Today.AddMonths(2),
                       OverrideArCode = "111",
                       OverrideRate = 950.00m,
                       RateOverrideReason = "AT",
                       Term = "2016/Spr",
                       UsedRatePeriods = 1
                    },
                    new MealPlanAssignment("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "2", "DIN", DateTime.Today, 4, "T", DateTime.Today)
                    {
                       EndDate = DateTime.Today.AddMonths(1),
                       OverrideArCode = "111",
                       OverrideRate = null,
                       RateOverrideReason = "AC",
                       Term = "2017/Fall",
                       UsedRatePeriods = 1
                    }
                };
                //student meal plan tuple
                _studentMealPlansCollectionTuple = new Tuple<IEnumerable<MealPlanAssignment>, int>(_studentMealPlansCollection, _studentMealPlansCollection.Count());
                //acad period
                List<Domain.Student.Entities.AcademicPeriod> acadPeriods = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "2016 Spring", new DateTime(2016, 01, 01), new DateTime(2016, 05, 01), 2016, 1, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("7f3aac22-e0b5-4159-b4e2-da158362c41b", "2016/Fall", "2016 Fall", new DateTime(2016, 09, 01), new DateTime(2016, 10, 15), 2016, 2, "Fall", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("8f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Spr", "2017 Spring", new DateTime(2017, 01, 01), new DateTime(2017, 05, 01), 2017, 3, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("9f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Fall", "2017 Fall", new DateTime(2017, 09, 01), new DateTime(2017, 12, 31), 2017, 4, "Fall", "", "", null)
                };
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(acadPeriods);
                foreach (var academicPeriod in acadPeriods)
                {
                    _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(academicPeriod.Code)).ReturnsAsync(academicPeriod.Guid);
                }
                //accounting codes
                IEnumerable<Domain.Student.Entities.AccountingCode> accountingCodesEntities = new TestAccountingCodesRepository().Get();
                _studentReferenceRepositoryMock.Setup(repo => repo.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountingCodesEntities);

                //billing override reasons
                List<Domain.Student.Entities.BillingOverrideReasons> billingOverrideReasons = new List<Domain.Student.Entities.BillingOverrideReasons>() 
                {
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")

                };
                _studentReferenceRepositoryMock.Setup(repo => repo.GetBillingOverrideReasonsAsync(It.IsAny<bool>())).ReturnsAsync(billingOverrideReasons);
                _studentReferenceRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                List<Domain.Student.Entities.MealPlan> mealPlanCollection = new List<Domain.Student.Entities.MealPlan>()
                {
                        new Domain.Student.Entities.MealPlan("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "LNCH", "Lunch"){ RatePeriod = "D"},
                        new Domain.Student.Entities.MealPlan("949e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "DIN", "Dinner"){RatePeriod = "W"},
                        new Domain.Student.Entities.MealPlan("f2253ac7-9931-4560-b42f-1fccd43c952f", "YEAR", "Cultural"){RatePeriod = "Y"},
                        new Domain.Student.Entities.MealPlan("g2253ac7-9931-4560-b42f-1fccd43c952g", "TERM", "Cultural"){RatePeriod = "T"}
                };
                foreach (var mealplan in mealPlanCollection)
                {
                    _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanGuidAsync(mealplan.Code)).ReturnsAsync(mealplan.Guid);
                }
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlanCollection);
                List<Ellucian.Colleague.Domain.Student.Entities.MealPlanRates> mealPlanRatesCollection = new List<Domain.Student.Entities.MealPlanRates>()
                {
                    new Domain.Student.Entities.MealPlanRates("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "LNCH", "Lunch")
                    { 
                        MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now), 
                        MealRatePeriod = MealPlanRatePeriods.Day 
                    },
                    new Domain.Student.Entities.MealPlanRates("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "DIN", "Dinner")
                    { 
                        MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now.AddDays(-1)), 
                        MealRatePeriod = MealPlanRatePeriods.Meal 
                    },
                    new Domain.Student.Entities.MealPlanRates("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                    { 
                        MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now.AddDays(1)), 
                        MealRatePeriod = MealPlanRatePeriods.Month 
                    }
                };
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanRatesAsync(It.IsAny<bool>())).ReturnsAsync(mealPlanRatesCollection);

                _personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("dbe84d5b-a226-49b9-a029-4dc86ee135b3");
                personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("100", "dbe84d5b - a226 - 49b9 - a029 - 4dc86ee135b3");
                personGuidCollection.Add("1", "1");
                personGuidCollection.Add("2", "2");
                _personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _studentMealPlansService = null;
                _studentMealPlansCollection = null;
                _studentReferenceRepositoryMock = null;
                _mealPlanAssignmentRepositoryMock = null;
                _personRepositoryMock = null;
                _termRepositoryMock = null;
                _adapterRegistryMock = null;
                _roleRepositoryMock = null;
                _loggerMock = null;
                currentUserFactory = null;
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollectionTuple);

                var results = await _studentMealPlansService.GetStudentMealPlansAsync(offset, limit, It.IsAny<Dtos.StudentMealPlans>(),It.IsAny<bool>());
                Assert.IsNotNull(results);
                Assert.AreEqual(results.Item2, _studentMealPlansCollection.Count());

                foreach (var actual in results.Item1)
                {
                    var expected = _studentMealPlansCollection.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                }
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanByIdAsync()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                var expected = _studentMealPlansCollection.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanByIdAsync_Status_C()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "LNCH", DateTime.Today, 5, "C", DateTime.Today)
                    {
                        EndDate = DateTime.Today.AddMonths(2),
                        OverrideArCode = "111",
                        OverrideRate = 950.00m,
                        RateOverrideReason = "AT",
                        Term = "2016/Spr",
                        UsedRatePeriods = 1
                    };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanByIdAsync_Status_L()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "LNCH", DateTime.Today, 5, "L", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanByIdAsync_MealPlan_DIN()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "DIN", DateTime.Today, 5, "C", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanByIdAsync_RatesRatePeriod_Y()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "YEAR", DateTime.Today, 5, "C", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanByIdAsync_RatesRatePeriod_T()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "TERM", DateTime.Today, 5, "C", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentMealPlansService_GetStudentMealPlansByGuidAsync_Empty_ArgumentNullException()
            {
                await _studentMealPlansService.GetStudentMealPlansByGuidAsync("");
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task StudentMealPlansService_GetStudentMealPlansAsync_PermissionsException()
            //{
            //    await _studentMealPlansService.GetStudentMealPlansByGuidAsync("1");
            //}

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_AcademicPeriodsNull_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                List<Domain.Student.Entities.AcademicPeriod> mp = null;

                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(mp);
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_TermNull_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentMealPlansCollection.FirstOrDefault().Term = "BadTerm";
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException) )]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_AccountingCodesNull_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentReferenceRepositoryMock.Setup(repo => repo.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException) )]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_InvalidOverrideArCode_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentMealPlansCollection.FirstOrDefault().OverrideArCode = "BadCode";
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException) )]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_BillingOverrideReasons_Null_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentReferenceRepositoryMock.Setup(repo => repo.GetBillingOverrideReasonsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException) )]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_InvalidRateOverrideReason_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentMealPlansCollection.FirstOrDefault().RateOverrideReason = "BadReason";
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException) )]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_MealPlanNull_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException) )]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_InvalidMealPlan_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "BadCode", DateTime.Today, 5, "A", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_MealPlanRateNull_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanRatesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.ElementAt(1));

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_BadPersonId_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });
                var personGuidCollection = new Dictionary<string, string>();
                _personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.ElementAt(1));

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_KeyNotFoundException()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentMealPlansService_GetStudentMealPlansAsync_InvalidOperationException()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                var actual = await _studentMealPlansService.GetStudentMealPlansByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            #region 16.0.0

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollectionTuple);

                var results = await _studentMealPlansService.GetStudentMealPlans2Async(offset, limit, It.IsAny<Dtos.StudentMealPlans2>(), It.IsAny<bool>());
                Assert.IsNotNull(results);
                Assert.AreEqual(results.Item2, _studentMealPlansCollection.Count());

                foreach (var actual in results.Item1)
                {
                    var expected = _studentMealPlansCollection.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                }
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanById2Async()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                var expected = _studentMealPlansCollection.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanById2Async_Status_C()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "LNCH", DateTime.Today, 5, "C", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);
                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanById2Async_Status_L()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "LNCH", DateTime.Today, 5, "L", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanById2Async_MealPlan_DIN()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "DIN", DateTime.Today, 5, "C", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanById2Async_RatesRatePeriod_Y()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "YEAR", DateTime.Today, 5, "C", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            public async Task StudentMealPlansService_GetStudentMealPlanById2Async_RatesRatePeriod_T()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "TERM", DateTime.Today, 5, "C", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentMealPlansService_GetStudentMealPlansByGuid2Async_Empty_ArgumentNullException()
            {
                await _studentMealPlansService.GetStudentMealPlansByGuid2Async("");
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task StudentMealPlansService_GetStudentMealPlans2Async_PermissionsException()
            //{
            //    await _studentMealPlansService.GetStudentMealPlansByGuid2Async("1");
            //}

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_AcademicPeriodsNull_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                List<Domain.Student.Entities.AcademicPeriod> mp = null;

                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(mp);
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_TermNull_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentMealPlansCollection.FirstOrDefault().Term = "BadTerm";
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_AccountingCodesNull_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentReferenceRepositoryMock.Setup(repo => repo.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_InvalidOverrideArCode_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentMealPlansCollection.FirstOrDefault().OverrideArCode = "BadCode";
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_BillingOverrideReasons_Null_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentReferenceRepositoryMock.Setup(repo => repo.GetBillingOverrideReasonsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_InvalidRateOverrideReason_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentMealPlansCollection.FirstOrDefault().RateOverrideReason = "BadReason";
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_MealPlanNull_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.FirstOrDefault());

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_InvalidMealPlan_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var expected = new MealPlanAssignment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "1", "BadCode", DateTime.Today, 5, "A", DateTime.Today)
                {
                    EndDate = DateTime.Today.AddMonths(2),
                    OverrideArCode = "111",
                    OverrideRate = 950.00m,
                    RateOverrideReason = "AT",
                    Term = "2016/Spr",
                    UsedRatePeriods = 1
                };
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_MealPlanRateNull_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanRatesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.ElementAt(1));

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_BadPersonId_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });
                var personGuidCollection = new Dictionary<string, string>();
                _personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentMealPlansCollection.ElementAt(1));

                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentMealPlansService_GetStudentMealPlans2Async_KeyNotFoundException()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentMealPlansService_GetStudentMealPlansGuid2Async_InvalidOperationException()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                var actual = await _studentMealPlansService.GetStudentMealPlansByGuid2Async("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            }

            [TestMethod]
            public async Task GetStudentMealPlans2Async_Filter()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, mealPlan);
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task GetStudentMealPlans2Async_PersonId_Zero_Records()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, mealPlan);
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task GetStudentMealPlans2Async_MealPlans_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, mealPlan);
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task GetStudentMealPlans2Async_MealPlans_Zero_Records()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, mealPlan);
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task GetStudentMealPlans2Async_AcademicPeriod_Null_Records()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });
                mealPlan.MealPlan.Id = string.Empty;
                List<Domain.Student.Entities.AcademicPeriod> mp = null;
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(mp);
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, mealPlan);
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task GetStudentMealPlans2Async_AcademicPeriod_Zero_Records()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                mealPlan.MealPlan.Id = string.Empty;
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, mealPlan);
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task GetStudentMealPlans2Async_AcademicPeriod_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });
                mealPlan.MealPlan.Id = string.Empty;
                List<Domain.Student.Entities.AcademicPeriod> mp = null;
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Throws(new Exception());
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, mealPlan);
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task GetStudentMealPlans2Async_DateConversion()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });
                mealPlan.MealPlan.Id = string.Empty;
                mealPlan.AcademicPeriod = null;
                List<Domain.Student.Entities.AcademicPeriod> mp = null;
                _studentReferenceRepositoryMock.Setup(repo => repo.GetUnidataFormattedDate(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, mealPlan);
                Assert.IsNotNull(actual);
                Assert.AreEqual(0, actual.Item2);
            }

            [TestMethod]
            public async Task GetStudentMealPlans2Async_Null()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(() => null);
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, null);
            }

            [TestMethod]
            public async Task GetStudentMealPlans2Async_Empty()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new Tuple<IEnumerable<MealPlanAssignment>, int>(new List<MealPlanAssignment>(), 0));
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetStudentMealPlans2Async_RepositoryException()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new RepositoryException());
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetStudentMealPlans2Async_ArgumentException()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentException());
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetStudentMealPlans2Async_Exception()
            {
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                var actual = await _studentMealPlansService.GetStudentMealPlans2Async(0, 100, null);
            }

            #endregion
        }

        [TestClass]
        public class StudentMealPlanService_PUT_POST_Tests: CurrentUserSetup
        {
            private const string studentMealPlansGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string studentMealPlansCode = "AT";
            private StudentMealPlansService _studentMealPlansService;
            Dtos.StudentMealPlans expected;
            Ellucian.Colleague.Domain.Student.Entities.MealPlanAssignment mealPlanAssignment;
            List<Domain.Student.Entities.MealPlan> mealPlanCollection;
            List<Domain.Student.Entities.AcademicPeriod> acadPeriods;
            private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
            private Mock<IMealPlanAssignmentRepository> _mealPlanAssignmentRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<ITermRepository> _termRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Dictionary<string, string> personGuidCollection;

            [TestInitialize]
            public void Initialize()
            {
                _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _mealPlanAssignmentRepositoryMock = new Mock<IMealPlanAssignmentRepository>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _termRepositoryMock = new Mock<ITermRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                InitializeData();

                _studentMealPlansService = new StudentMealPlansService(_studentReferenceRepositoryMock.Object, _mealPlanAssignmentRepositoryMock.Object, _termRepositoryMock.Object,
                    _personRepositoryMock.Object, baseConfigurationRepository, _adapterRegistryMock.Object, currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _studentMealPlansService = null;
                _studentReferenceRepositoryMock = null;
                _mealPlanAssignmentRepositoryMock = null;
                _personRepositoryMock = null;
                _termRepositoryMock = null;
                _adapterRegistryMock = null;
                _roleRepositoryMock = null;
                _loggerMock = null;
                currentUserFactory = null;
            }

            [TestMethod]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync()
            {
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.Consumption.Units.Value, actual.Consumption.Units.Value);
                Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.MealCard, actual.MealCard);
                Assert.AreEqual(expected.MealPlan.Id, actual.MealPlan.Id);
                Assert.AreEqual(expected.NumberOfPeriods.Value, actual.NumberOfPeriods.Value);
                Assert.AreEqual(expected.OverrideRate.AccountingCode.Id, actual.OverrideRate.AccountingCode.Id);
                Assert.AreEqual(expected.OverrideRate.OverrideReason.Id, actual.OverrideRate.OverrideReason.Id);
                Assert.AreEqual(expected.OverrideRate.Rate.Currency.Value, actual.OverrideRate.Rate.Currency.Value);
                Assert.AreEqual(expected.OverrideRate.RatePeriod, actual.OverrideRate.RatePeriod);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.StatusDate.Value, actual.StatusDate.Value);
            }

            [TestMethod]
            public async Task StudentMealPlansService_PostStudentMealPlansAsync()
            {
                expected.Id = Guid.Empty.ToString();
                var actual = await _studentMealPlansService.PostStudentMealPlansAsync(expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.Consumption.Units.Value, actual.Consumption.Units.Value);
                Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                Assert.AreEqual(expected.MealCard, actual.MealCard);
                Assert.AreEqual(expected.MealPlan.Id, actual.MealPlan.Id);
                Assert.AreEqual(expected.NumberOfPeriods.Value, actual.NumberOfPeriods.Value);
                Assert.AreEqual(expected.OverrideRate.AccountingCode.Id, actual.OverrideRate.AccountingCode.Id);
                Assert.AreEqual(expected.OverrideRate.OverrideReason.Id, actual.OverrideRate.OverrideReason.Id);
                Assert.AreEqual(expected.OverrideRate.Rate.Currency.Value, actual.OverrideRate.Rate.Currency.Value);
                Assert.AreEqual(expected.OverrideRate.RatePeriod, actual.OverrideRate.RatePeriod);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.StatusDate.Value, actual.StatusDate.Value);
            }


            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_NullMealPlan_ArgumentNullException()
            {
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_NullMealPlanId_ArgumentNullException()
            {
                expected.Id = "";
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            //[TestMethod]
            //[ExpectedException(typeof(ColleagueWebApiException))]
            //public async Task StudentMealPlansService_PutStudentMealPlansAsync_NoPermission_ArgumentNullException()
            //{
            //    _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { null });
            //    var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            //}

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_NullPerson_ArgumentNullException()
            {
                expected.Person = null;
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_NullPersonId_ArgumentNullException()
            {
                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_NullMealPlanDto_ArgumentNullException()
            {
                expected.MealPlan = null;
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_MealPlanDto_BadId_ArgumentNullException()
            {
                expected.MealPlan.Id = "BadId";
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_NullMealPlanValCodes_ArgumentNullException()
            {
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_NotSet_Status_ArgumentNullException()
            {
                expected.Status = Dtos.EnumProperties.StudentMealPlansStatus.NotSet;
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_StartOn_Null_MinValue_ArgumentNullException()
            {
                expected.StartOn = DateTime.MinValue;
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_StatusDate_Null_MinValue_ArgumentNullException()
            {
                expected.StatusDate = DateTime.MinValue;
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_Null_AcademicPeriods_Codes_ArgumentNullException()
            {
                List<Domain.Student.Entities.AcademicPeriod> ac = null;
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(ac);
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_Null_MealPlan_ArgumentNullException()
            {
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentMealPlansService_PutStudentMealPlansAsync_Null_MealPlanRates_ArgumentNullException()
            {
                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PostStudentMealPlansAsync_Null_Argument_ArgumentNullException()
            {
                var actual = await _studentMealPlansService.PostStudentMealPlansAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentMealPlansService_PostStudentMealPlansAsync_Null_Id_ArgumentNullException()
            {
                expected.Id = string.Empty;
                var actual = await _studentMealPlansService.PostStudentMealPlansAsync(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task StudentMealPlansService_PostStudentMealPlansAsync_Null_Id_RepositoryException()
            {
                expected.Id = Guid.Empty.ToString();
                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var actual = await _studentMealPlansService.PostStudentMealPlansAsync(expected);
            }

            private void InitializeData()
            {
                //Permissions setup
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });


                //acad period
                acadPeriods = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "2016 Spring", new DateTime(2016, 01, 01), new DateTime(2016, 05, 01), 2016, 1, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("7f3aac22-e0b5-4159-b4e2-da158362c41b", "2016/Fall", "2016 Fall", new DateTime(2016, 09, 01), new DateTime(2016, 10, 15), 2016, 2, "Fall", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("8f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Spr", "2017 Spring", new DateTime(2017, 01, 01), new DateTime(2017, 05, 01), 2017, 3, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("9f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Fall", "2017 Fall", new DateTime(2017, 09, 01), new DateTime(2017, 12, 31), 2017, 4, "Fall", "", "", null)
                };
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(acadPeriods);
                foreach (var academicPeriod in acadPeriods)
                {
                    _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(academicPeriod.Code)).ReturnsAsync(academicPeriod.Guid);
                }
                //accounting codes
                IEnumerable<Domain.Student.Entities.AccountingCode> accountingCodesEntities = new TestAccountingCodesRepository().Get();
                _studentReferenceRepositoryMock.Setup(repo => repo.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountingCodesEntities);

                //billing override reasons
                List<Domain.Student.Entities.BillingOverrideReasons> billingOverrideReasons = new List<Domain.Student.Entities.BillingOverrideReasons>() 
                {
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("2a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")

                };
                _studentReferenceRepositoryMock.Setup(repo => repo.GetBillingOverrideReasonsAsync(It.IsAny<bool>())).ReturnsAsync(billingOverrideReasons);
                _studentReferenceRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                mealPlanCollection = new List<Domain.Student.Entities.MealPlan>()
                {
                        new Domain.Student.Entities.MealPlan("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "LNCH", "Lunch"){ RatePeriod = "D"},
                        new Domain.Student.Entities.MealPlan("949e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "DIN", "Dinner"){RatePeriod = "W"},
                        new Domain.Student.Entities.MealPlan("f2253ac7-9931-4560-b42f-1fccd43c952f", "YEAR", "Cultural"){RatePeriod = "Y"},
                        new Domain.Student.Entities.MealPlan("g2253ac7-9931-4560-b42f-1fccd43c952g", "TERM", "Cultural"){RatePeriod = "T"}
                };
                foreach (var mealplan in mealPlanCollection)
                {
                    _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanGuidAsync(mealplan.Code)).ReturnsAsync(mealplan.Guid);
                }
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlanCollection);
                List<Ellucian.Colleague.Domain.Student.Entities.MealPlanRates> mealPlanRatesCollection = new List<Domain.Student.Entities.MealPlanRates>()
                {
                    new Domain.Student.Entities.MealPlanRates("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "LNCH", "Lunch")
                    { 
                        MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now), 
                        MealRatePeriod = MealPlanRatePeriods.Day 
                    },
                    new Domain.Student.Entities.MealPlanRates("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "DIN", "Dinner")
                    { 
                        MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now.AddDays(-1)), 
                        MealRatePeriod = MealPlanRatePeriods.Meal 
                    },
                    new Domain.Student.Entities.MealPlanRates("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                    { 
                        MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now.AddDays(1)), 
                        MealRatePeriod = MealPlanRatePeriods.Month 
                    }
                };
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanRatesAsync(It.IsAny<bool>())).ReturnsAsync(mealPlanRatesCollection);

                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                _personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("dbe84d5b-a226-49b9-a029-4dc86ee135b3");
                personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("1", "dbe84d5b-a226-49b9-a029-4dc86ee135b3");
                _personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);

                expected = new StudentMealPlans()
                {
                    Id = studentMealPlansGuid,
                    AcademicPeriod = new GuidObject2("b9691210-8516-45ca-9cd1-7e5aa1777234"),
                    AssignedRate = new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    Comment = "Test Comment",
                    Consumption = new Dtos.DtoProperties.StudentMealPlansConsumption()
                    {
                        //Percent = 100,
                        Units = 1
                    },
                    EndOn = DateTime.Today.AddDays(30),
                    MealCard = "MealCard",
                    MealPlan = new GuidObject2("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    NumberOfPeriods = 1,
                    OverrideRate = new Dtos.DtoProperties.StudentMealPlansOverrideRateDtoProperty()
                    {
                        AccountingCode = new GuidObject2("4b7568dd-d4e5-41f6-9ac7-5da29bfda07a"),
                        OverrideReason = new GuidObject2("2a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                        Rate = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                            Value = 101.00m
                        },
                        RatePeriod = Dtos.EnumProperties.MealPlanRatesRatePeriod.Day
                    },
                    Person = new GuidObject2("dbe84d5b-a226-49b9-a029-4dc86ee135b3"),
                    StartOn = DateTime.Today.AddDays(1),
                    Status = Dtos.EnumProperties.StudentMealPlansStatus.Prorated,
                    StatusDate = DateTime.Today.AddDays(1)
                };
                mealPlanAssignment = new MealPlanAssignment(studentMealPlansGuid, "1", "1", "LNCH", DateTime.Today.AddDays(1), 1, "L", DateTime.Today.AddDays(1));
                mealPlanAssignment.MealComments = "Test Comment";
                mealPlanAssignment.MealCard = "MealCard";
                mealPlanAssignment.OverrideRate = 101.00m;
                mealPlanAssignment.OverrideArCode = "111";
                mealPlanAssignment.RateOverrideReason = "AT";                
                mealPlanAssignment.EndDate = DateTime.Today.AddDays(30);
                mealPlanAssignment.Term = "2016/Spr";
                mealPlanAssignment.UsedRatePeriods = 1;

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.UpdateMealPlanAssignmentAsync(It.IsAny<MealPlanAssignment>())).ReturnsAsync(mealPlanAssignment);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.CreateMealPlanAssignmentAsync(It.IsAny<MealPlanAssignment>())).ReturnsAsync(mealPlanAssignment);
            }
        }

        [TestClass]
        public class StudentMealPlanService_PUT_POST_16_Tests : CurrentUserSetup
        {
            private const string studentMealPlansGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string studentMealPlansCode = "AT";
            private StudentMealPlansService _studentMealPlansService;
            Dtos.StudentMealPlans2 expected;
            Ellucian.Colleague.Domain.Student.Entities.MealPlanAssignment mealPlanAssignment;
            List<Domain.Student.Entities.MealPlan> mealPlanCollection;
            List<Domain.Student.Entities.AcademicPeriod> acadPeriods;
            private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
            private Mock<IMealPlanAssignmentRepository> _mealPlanAssignmentRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<ITermRepository> _termRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Dictionary<string, string> personGuidCollection;

            [TestInitialize]
            public void Initialize()
            {
                _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _mealPlanAssignmentRepositoryMock = new Mock<IMealPlanAssignmentRepository>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _termRepositoryMock = new Mock<ITermRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                InitializeData();

                _studentMealPlansService = new StudentMealPlansService(_studentReferenceRepositoryMock.Object, _mealPlanAssignmentRepositoryMock.Object, _termRepositoryMock.Object,
                    _personRepositoryMock.Object, baseConfigurationRepository, _adapterRegistryMock.Object, currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _studentMealPlansService = null;
                _studentReferenceRepositoryMock = null;
                _mealPlanAssignmentRepositoryMock = null;
                _personRepositoryMock = null;
                _termRepositoryMock = null;
                _adapterRegistryMock = null;
                _roleRepositoryMock = null;
                _loggerMock = null;
                currentUserFactory = null;
            }

            [TestMethod]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async()
            {
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.Consumption.Units.Value, actual.Consumption.Units.Value);
                Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.MealCard, actual.MealCard);
                Assert.AreEqual(expected.MealPlan.Id, actual.MealPlan.Id);
                Assert.AreEqual(expected.NumberOfPeriods.Value, actual.NumberOfPeriods.Value);
                Assert.AreEqual(expected.OverrideRate.AccountingCode.Id, actual.OverrideRate.AccountingCode.Id);
                Assert.AreEqual(expected.OverrideRate.OverrideReason.Id, actual.OverrideRate.OverrideReason.Id);
                Assert.AreEqual(expected.OverrideRate.Rate.Currency.Value, actual.OverrideRate.Rate.Currency.Value);
                Assert.AreEqual(expected.OverrideRate.RatePeriod, actual.OverrideRate.RatePeriod);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.StatusDate.Value, actual.StatusDate.Value);
            }

            [TestMethod]
            public async Task StudentMealPlansService_PostStudentMealPlans2Async()
            {
                expected.Id = Guid.Empty.ToString();
                var actual = await _studentMealPlansService.PostStudentMealPlans2Async(expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.Comment, actual.Comment);
                Assert.AreEqual(expected.Consumption.Units.Value, actual.Consumption.Units.Value);
                Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                Assert.AreEqual(expected.MealCard, actual.MealCard);
                Assert.AreEqual(expected.MealPlan.Id, actual.MealPlan.Id);
                Assert.AreEqual(expected.NumberOfPeriods.Value, actual.NumberOfPeriods.Value);
                Assert.AreEqual(expected.OverrideRate.AccountingCode.Id, actual.OverrideRate.AccountingCode.Id);
                Assert.AreEqual(expected.OverrideRate.OverrideReason.Id, actual.OverrideRate.OverrideReason.Id);
                Assert.AreEqual(expected.OverrideRate.Rate.Currency.Value, actual.OverrideRate.Rate.Currency.Value);
                Assert.AreEqual(expected.OverrideRate.RatePeriod, actual.OverrideRate.RatePeriod);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.StartOn.Value, actual.StartOn.Value);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.StatusDate.Value, actual.StatusDate.Value);
            }


            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_NullMealPlan_ArgumentNullException()
            {
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, null);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_NullMealPlanId_ArgumentNullException()
            {
                expected.Id = "";
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            //[TestMethod]
            //[ExpectedException(typeof( IntegrationApiException ) )]
            //public async Task StudentMealPlansService_PutStudentMealPlans2Async_NoPermission_ArgumentNullException()
            //{
            //    _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { null });
            //    var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            //}

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_NullPerson_ArgumentNullException()
            {
                expected.Person = null;
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_NullPersonId_ArgumentNullException()
            {
                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_NullMealPlanDto_ArgumentNullException()
            {
                expected.MealPlan = null;
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_MealPlanDto_BadId_ArgumentNullException()
            {
                expected.MealPlan.Id = "BadId";
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_NullMealPlanValCodes_ArgumentNullException()
            {
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_NotSet_Status_ArgumentNullException()
            {
                expected.Status = Dtos.EnumProperties.StudentMealPlansStatus.NotSet;
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_StartOn_Null_MinValue_ArgumentNullException()
            {
                expected.StartOn = DateTime.MinValue;
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_StatusDate_Null_MinValue_ArgumentNullException()
            {
                expected.StatusDate = DateTime.MinValue;
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_Null_AcademicPeriods_Codes_ArgumentNullException()
            {
                List<Domain.Student.Entities.AcademicPeriod> ac = null;
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(ac);
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_Null_MealPlan_ArgumentNullException()
            {
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            //Un comment out the code once the catch is implemeneted in the service layer.

            //[TestMethod] 
            //[ExpectedException(typeof(ColleagueWebApiException))]
            //public async Task StudentMealPlansService_PutStudentMealPlansAsync_Null_MealPlanRates_Valcode_ArgumentNullException()
            //{
            //    _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanRatesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
            //    var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            //}

            //[TestMethod]            
            //[ExpectedException(typeof(ColleagueWebApiException))]
            //public async Task StudentMealPlansService_PutStudentMealPlansAsync_Null_MealPlanRates_ArgumentNullException()
            //{
            //    expected.AssignedRate.Id = "Bad ID";
            //    var actual = await _studentMealPlansService.PutStudentMealPlansAsync(studentMealPlansGuid, expected);
            //}

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PutStudentMealPlans2Async_Null_MealPlanRates_ArgumentNullException()
            {
                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var actual = await _studentMealPlansService.PutStudentMealPlans2Async(studentMealPlansGuid, expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PostStudentMealPlans2Async_Null_Argument_ArgumentNullException()
            {
                var actual = await _studentMealPlansService.PostStudentMealPlans2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PostStudentMealPlans2Async_Null_Id_ArgumentNullException()
            {
                expected.Id = string.Empty;
                var actual = await _studentMealPlansService.PostStudentMealPlans2Async(expected);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentMealPlansService_PostStudentMealPlans2Async_Null_Id_RepositoryException()
            {
                expected.Id = Guid.Empty.ToString();
                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var actual = await _studentMealPlansService.PostStudentMealPlans2Async(expected);
            }

            private void InitializeData()
            {
                //Permissions setup
                viewMealPlan.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateMealPlanAssignment));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewMealPlan });


                //acad period
                acadPeriods = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "2016 Spring", new DateTime(2016, 01, 01), new DateTime(2016, 05, 01), 2016, 1, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("7f3aac22-e0b5-4159-b4e2-da158362c41b", "2016/Fall", "2016 Fall", new DateTime(2016, 09, 01), new DateTime(2016, 10, 15), 2016, 2, "Fall", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("8f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Spr", "2017 Spring", new DateTime(2017, 01, 01), new DateTime(2017, 05, 01), 2017, 3, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("9f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Fall", "2017 Fall", new DateTime(2017, 09, 01), new DateTime(2017, 12, 31), 2017, 4, "Fall", "", "", null)
                };
                _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(It.IsAny<IEnumerable<Term>>())).Returns(acadPeriods);
                foreach (var academicPeriod in acadPeriods)
                {
                    _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(academicPeriod.Code)).ReturnsAsync(academicPeriod.Guid);
                }
                //accounting codes
                IEnumerable<Domain.Student.Entities.AccountingCode> accountingCodesEntities = new TestAccountingCodesRepository().Get();
                _studentReferenceRepositoryMock.Setup(repo => repo.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountingCodesEntities);

                //billing override reasons
                List<Domain.Student.Entities.BillingOverrideReasons> billingOverrideReasons = new List<Domain.Student.Entities.BillingOverrideReasons>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("2a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")

                };
                _studentReferenceRepositoryMock.Setup(repo => repo.GetBillingOverrideReasonsAsync(It.IsAny<bool>())).ReturnsAsync(billingOverrideReasons);
                _studentReferenceRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                mealPlanCollection = new List<Domain.Student.Entities.MealPlan>()
                {
                        new Domain.Student.Entities.MealPlan("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "LNCH", "Lunch"){ RatePeriod = "D"},
                        new Domain.Student.Entities.MealPlan("949e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "DIN", "Dinner"){RatePeriod = "W"},
                        new Domain.Student.Entities.MealPlan("f2253ac7-9931-4560-b42f-1fccd43c952f", "YEAR", "Cultural"){RatePeriod = "Y"},
                        new Domain.Student.Entities.MealPlan("g2253ac7-9931-4560-b42f-1fccd43c952g", "TERM", "Cultural"){RatePeriod = "T"}
                };
                foreach (var mealplan in mealPlanCollection)
                {
                    _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanGuidAsync(mealplan.Code)).ReturnsAsync(mealplan.Guid);
                }
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlansAsync(It.IsAny<bool>())).ReturnsAsync(mealPlanCollection);
                List<Ellucian.Colleague.Domain.Student.Entities.MealPlanRates> mealPlanRatesCollection = new List<Domain.Student.Entities.MealPlanRates>()
                {
                    new Domain.Student.Entities.MealPlanRates("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "LNCH", "Lunch")
                    {
                        MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now),
                        MealRatePeriod = MealPlanRatePeriods.Day
                    },
                    new Domain.Student.Entities.MealPlanRates("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "DIN", "Dinner")
                    {
                        MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now.AddDays(-1)),
                        MealRatePeriod = MealPlanRatePeriods.Meal
                    },
                    new Domain.Student.Entities.MealPlanRates("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                    {
                        MealPlansMealPlanRates = new MealPlansMealPlanRates(175, DateTime.Now.AddDays(1)),
                        MealRatePeriod = MealPlanRatePeriods.Month
                    }
                };
                _studentReferenceRepositoryMock.Setup(repo => repo.GetMealPlanRatesAsync(It.IsAny<bool>())).ReturnsAsync(mealPlanRatesCollection);

                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                _personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("dbe84d5b-a226-49b9-a029-4dc86ee135b3");
                personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("1", "dbe84d5b-a226-49b9-a029-4dc86ee135b3");
                _personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);

                expected = new StudentMealPlans2()
                {
                    Id = studentMealPlansGuid,
                    AcademicPeriod = new GuidObject2("b9691210-8516-45ca-9cd1-7e5aa1777234"),
                    AssignedRates = new List<GuidObject2>() { new GuidObject2("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc") },
                    Comment = "Test Comment",
                    Consumption = new Dtos.DtoProperties.StudentMealPlansConsumption()
                    {
                        Units = 1
                    },
                    EndOn = DateTime.Today.AddDays(30),
                    MealCard = "MealCard",
                    MealPlan = new GuidObject2("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    NumberOfPeriods = 1,
                    OverrideRate = new Dtos.DtoProperties.StudentMealPlansOverrideRateDtoProperty()
                    {
                        AccountingCode = new GuidObject2("4b7568dd-d4e5-41f6-9ac7-5da29bfda07a"),
                        OverrideReason = new GuidObject2("2a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                        Rate = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Currency = Dtos.EnumProperties.CurrencyIsoCode.USD,
                            Value = 101.00m
                        },
                        RatePeriod = Dtos.EnumProperties.MealPlanRatesRatePeriod.Day
                    },
                    Person = new GuidObject2("dbe84d5b-a226-49b9-a029-4dc86ee135b3"),
                    StartOn = DateTime.Today.AddDays(1),
                    Status = Dtos.EnumProperties.StudentMealPlansStatus.Prorated,
                    StatusDate = DateTime.Today.AddDays(1)
                };
                mealPlanAssignment = new MealPlanAssignment(studentMealPlansGuid, "1", "1", "LNCH", DateTime.Today.AddDays(1), 1, "L", DateTime.Today.AddDays(1));
                mealPlanAssignment.MealComments = "Test Comment";
                mealPlanAssignment.MealCard = "MealCard";
                mealPlanAssignment.OverrideRate = 101.00m;
                mealPlanAssignment.OverrideArCode = "111";
                mealPlanAssignment.RateOverrideReason = "AT";
                mealPlanAssignment.EndDate = DateTime.Today.AddDays(30);
                mealPlanAssignment.Term = "2016/Spr";
                mealPlanAssignment.UsedRatePeriods = 1;

                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.UpdateMealPlanAssignmentAsync(It.IsAny<MealPlanAssignment>())).ReturnsAsync(mealPlanAssignment);
                _mealPlanAssignmentRepositoryMock.Setup(repo => repo.CreateMealPlanAssignmentAsync(It.IsAny<MealPlanAssignment>())).ReturnsAsync(mealPlanAssignment);
            }
        }
    }
}