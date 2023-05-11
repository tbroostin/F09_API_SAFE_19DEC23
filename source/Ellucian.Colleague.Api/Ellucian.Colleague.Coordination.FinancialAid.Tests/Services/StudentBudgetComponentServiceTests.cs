/*Copyright 2015-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class StudentBudgetComponentServiceTests : FinancialAidServiceTestsSetup
    {
        public string studentId;

        public TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;
        public Mock<IFinancialAidOfficeRepository> financialAidOfficeRepositoryMock;

        public TestStudentAwardYearRepository testStudentAwardYearRepository;
        public Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;

        public TestStudentBudgetComponentRepository testStudentBudgetComponentRepository;
        public Mock<IStudentBudgetComponentRepository> studentBudgetComponentRepositoryMock;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public ITypeAdapter<Colleague.Domain.FinancialAid.Entities.StudentBudgetComponent, StudentBudgetComponent> studentBudgetComponentDtoAdapter;

        public FunctionEqualityComparer<StudentBudgetComponent> studentBudgetComponentDtoComparer;

        public void StudentBudgetComponentsTestsInitialize()
        {
            BaseInitialize();

            studentId = currentUserFactory.CurrentUser.PersonId;

            studentBudgetComponentDtoComparer = new FunctionEqualityComparer<StudentBudgetComponent>(
                (sbc1, sbc2) => (sbc1.AwardYear == sbc2.AwardYear && sbc1.StudentId == sbc2.StudentId && sbc1.BudgetComponentCode == sbc2.BudgetComponentCode),
                (sbc) => (sbc.AwardYear.GetHashCode() ^ sbc.StudentId.GetHashCode() ^ sbc.BudgetComponentCode.GetHashCode()));

            testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            testStudentAwardYearRepository = new TestStudentAwardYearRepository();
            testStudentBudgetComponentRepository = new TestStudentBudgetComponentRepository();
            studentBudgetComponentDtoAdapter = new AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.StudentBudgetComponent, StudentBudgetComponent>(adapterRegistryMock.Object, loggerMock.Object);

            financialAidOfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
            studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
            studentBudgetComponentRepositoryMock = new Mock<IStudentBudgetComponentRepository>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
        }

        [TestClass]
        public class GetStudentBudgetComponentsTests : StudentBudgetComponentServiceTests
        {

            public IEnumerable<StudentBudgetComponent> expectedBudgets
            {
                get
                {
                    return testStudentBudgetComponentRepository
                        .GetStudentBudgetComponentsAsync(
                            studentId,
                            testStudentAwardYearRepository.GetStudentAwardYears(
                                studentId,
                                new CurrentOfficeService(testFinancialAidOfficeRepository.GetFinancialAidOffices()))).Result
                        .Select(budgetEntity => studentBudgetComponentDtoAdapter.MapToType(budgetEntity));
                }
            }

            //Initialize this
            public StudentBudgetComponentService actualService;

            public IEnumerable<StudentBudgetComponent> actualBudgets;

            private bool getActiveYearsOnly;
            
            [TestInitialize]
            public void Initialize()
            {
                StudentBudgetComponentsTestsInitialize();

                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
                    .Returns(() => testFinancialAidOfficeRepository.GetFinancialAidOfficesAsync());

                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, officeService, b) => Task.FromResult(testStudentAwardYearRepository.GetStudentAwardYears(id, officeService)));

                studentBudgetComponentRepositoryMock.Setup(r => r.GetStudentBudgetComponentsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear>>((id, studentAwardYears) =>
                        testStudentBudgetComponentRepository.GetStudentBudgetComponentsAsync(id, studentAwardYears));

                adapterRegistryMock.Setup(r => r.GetAdapter<Colleague.Domain.FinancialAid.Entities.StudentBudgetComponent, StudentBudgetComponent>())
                    .Returns(() => studentBudgetComponentDtoAdapter);

                BuildService();
            }

            private void BuildService()
            {
                actualService = new StudentBudgetComponentService(
                                    adapterRegistryMock.Object,
                                    studentBudgetComponentRepositoryMock.Object,
                                    studentAwardYearRepositoryMock.Object,
                                    financialAidOfficeRepositoryMock.Object,
                                    baseConfigurationRepository,
                                    currentUserFactory,
                                    roleRepositoryMock.Object,
                                    loggerMock.Object);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualBudgets = await actualService.GetStudentBudgetComponentsAsync(studentId);
                Assert.IsNotNull(actualBudgets);
                Assert.IsTrue(actualBudgets.Count() > 0);
                CollectionAssert.AreEqual(expectedBudgets.ToList(), actualBudgets.ToList(), studentBudgetComponentDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await actualService.GetStudentBudgetComponentsAsync(null);
            }

            //The studentId is initialized to the current user id
            [TestMethod]
            public async Task StudentIdIsCurrentUserTest()
            {
                actualBudgets = await actualService.GetStudentBudgetComponentsAsync(studentId);
                Assert.IsTrue(actualBudgets.All(budget => budget.StudentId == studentId));
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsCounselorTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildService();
                actualBudgets = await actualService.GetStudentBudgetComponentsAsync(studentId);

                CollectionAssert.AreEqual(expectedBudgets.ToList(), actualBudgets.ToList(), studentBudgetComponentDtoComparer);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsCounselorWithNoPermissionsTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                BuildService();
                await actualService.GetStudentBudgetComponentsAsync(studentId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsProxyTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildService();
                actualBudgets = await actualService.GetStudentBudgetComponentsAsync(studentId);

                CollectionAssert.AreEqual(expectedBudgets.ToList(), actualBudgets.ToList(), studentBudgetComponentDtoComparer);
            }

            /// <summary>
            /// User is proxy for a different user
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsProxyForDifferentUserTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();
                BuildService();
                await actualService.GetStudentBudgetComponentsAsync(studentId);
            }

            /// <summary>
            /// User is neither self, nor counselor, nor proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserNotRequestingSelfAndNotCounselorOrProxyTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));
                Assert.IsFalse(currentUserFactory.CurrentUser.ProxySubjects.Any());

                try
                {
                    await actualService.GetStudentBudgetComponentsAsync("foobar");
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("User {0} does not have permission to get StudentBudgetComponents for student {1}", currentUserFactory.CurrentUser.PersonId, "foobar")));
                    throw;
                }
            }

            [TestMethod]
            public async Task NullStudentAwardYears_ReturnEmptyListTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, officeService, b) => Task.FromResult((new List<Domain.FinancialAid.Entities.StudentAwardYear>()).AsEnumerable()));
                actualBudgets = await actualService.GetStudentBudgetComponentsAsync(studentId);

                Assert.AreEqual(0, actualBudgets.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no award years for which to get budget components", studentId)));
            }

            [TestMethod]
            public async Task NoBudgetComponents_ReturnEmptyListTest()
            {
                studentBudgetComponentRepositoryMock.Setup(r => r.GetStudentBudgetComponentsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear>>((id, studentAwardYears) => 
                    Task.FromResult((new List<Domain.FinancialAid.Entities.StudentBudgetComponent>()).AsEnumerable()));
                actualBudgets = await actualService.GetStudentBudgetComponentsAsync(studentId);

                Assert.AreEqual(0, actualBudgets.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no budget components for any award years", studentId)));
            }

        }

        [TestClass]
        public class GetStudentBudgetComponentsForYearTests : StudentBudgetComponentServiceTests
        {
            public string awardYear;
            public IEnumerable<StudentBudgetComponent> expectedBudgets
            {
                get
                {
                    return testStudentBudgetComponentRepository
                        .GetStudentBudgetComponentsAsync(
                            studentId,
                            new List<Domain.FinancialAid.Entities.StudentAwardYear>()
                            { new Domain.FinancialAid.Entities.StudentAwardYear(studentId, testStudentBudgetComponentRepository.responses.First().year)}).Result
                        .Select(budgetEntity => studentBudgetComponentDtoAdapter.MapToType(budgetEntity));
                }
            }

            //Initialize this
            public StudentBudgetComponentService actualService;

            public IEnumerable<StudentBudgetComponent> actualBudgets;

            [TestInitialize]
            public void Initialize()
            {
                StudentBudgetComponentsTestsInitialize();
                awardYear = testStudentBudgetComponentRepository.responses.First().year;

                studentBudgetComponentRepositoryMock.Setup(r => r.GetStudentBudgetComponentsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear>>((id, studentAwardYears) =>
                        testStudentBudgetComponentRepository.GetStudentBudgetComponentsAsync(id, studentAwardYears));

                adapterRegistryMock.Setup(r => r.GetAdapter<Colleague.Domain.FinancialAid.Entities.StudentBudgetComponent, StudentBudgetComponent>())
                    .Returns(() => studentBudgetComponentDtoAdapter);

                BuildService();
            }

            private void BuildService()
            {
                actualService = new StudentBudgetComponentService(
                                    adapterRegistryMock.Object,
                                    studentBudgetComponentRepositoryMock.Object,
                                    studentAwardYearRepositoryMock.Object,
                                    financialAidOfficeRepositoryMock.Object,
                                    baseConfigurationRepository,
                                    currentUserFactory,
                                    roleRepositoryMock.Object,
                                    loggerMock.Object);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualBudgets = await actualService.GetStudentBudgetComponentsForYearAsync(studentId, awardYear);
                Assert.IsNotNull(actualBudgets);
                Assert.IsTrue(actualBudgets.Count() > 0);
                CollectionAssert.AreEqual(expectedBudgets.ToList(), actualBudgets.ToList(), studentBudgetComponentDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await actualService.GetStudentBudgetComponentsForYearAsync(null, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardYearRequiredTest()
            {
                await actualService.GetStudentBudgetComponentsForYearAsync(studentId, null);
            }

            //The studentId is initialized to the current user id
            [TestMethod]
            public async Task StudentIdIsCurrentUserTest()
            {
                actualBudgets = await actualService.GetStudentBudgetComponentsForYearAsync(studentId, awardYear);
                Assert.IsTrue(actualBudgets.All(budget => budget.StudentId == studentId));
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsCounselorTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildService();
                actualBudgets = await actualService.GetStudentBudgetComponentsForYearAsync(studentId, awardYear);

                CollectionAssert.AreEqual(expectedBudgets.ToList(), actualBudgets.ToList(), studentBudgetComponentDtoComparer);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsCounselorWithNoPermissionsTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                BuildService();
                await actualService.GetStudentBudgetComponentsForYearAsync(studentId, awardYear);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsProxyTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildService();
                actualBudgets = await actualService.GetStudentBudgetComponentsForYearAsync(studentId, awardYear);

                CollectionAssert.AreEqual(expectedBudgets.ToList(), actualBudgets.ToList(), studentBudgetComponentDtoComparer);
            }

            /// <summary>
            /// User is proxy for a different user
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsProxyForDifferentUserTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();
                BuildService();
                await actualService.GetStudentBudgetComponentsForYearAsync(studentId, awardYear);
            }

            /// <summary>
            /// User is neither self, nor counselor, nor proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserNotRequestingSelfAndNotCounselorOrProxyTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));
                Assert.IsFalse(currentUserFactory.CurrentUser.ProxySubjects.Any());

                try
                {
                    await actualService.GetStudentBudgetComponentsForYearAsync("foobar", awardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("User {0} does not have permission to get StudentBudgetComponents for student {1}", currentUserFactory.CurrentUser.PersonId, "foobar")));
                    throw;
                }
            }

            [TestMethod]
            public async Task NoBudgetComponents_ReturnEmptyListTest()
            {
                studentBudgetComponentRepositoryMock.Setup(r => r.GetStudentBudgetComponentsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear>>((id, studentAwardYears) =>
                    Task.FromResult((new List<Domain.FinancialAid.Entities.StudentBudgetComponent>()).AsEnumerable()));
                actualBudgets = await actualService.GetStudentBudgetComponentsForYearAsync(studentId, awardYear);

                Assert.AreEqual(0, actualBudgets.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no budget components for {1} year", studentId, awardYear)));
            }

        }
    }
}
