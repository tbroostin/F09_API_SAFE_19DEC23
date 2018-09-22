//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class StudentLoanLimitationServiceTests
    {
        [TestClass]
        public class GetStudentLoanLimitationTests : FinancialAidServiceTestsSetup
        {
            private string studentId;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYearEntities;

            private TestStudentAwardYearRepository testStudentAwardYearRepository;
            private TestFinancialAidOfficeRepository testOfficeRepository;

            private TestStudentLoanLimitationRepository testStudentLoanLimitationRepository;
            private IEnumerable<Domain.FinancialAid.Entities.StudentLoanLimitation> inputStudentLoanLimitationEntities;
            private AutoMapperAdapter<Domain.FinancialAid.Entities.StudentLoanLimitation, Dtos.FinancialAid.StudentLoanLimitation> StudentLoanLimitationDtoAdapter;

            private List<Dtos.FinancialAid.StudentLoanLimitation> expectedStudentLoanLimitations;
            private IEnumerable<Dtos.FinancialAid.StudentLoanLimitation> actualStudentLoanLimitations;

            private Mock<IStudentLoanLimitationRepository> StudentLoanLimitationRepositoryMock;
            private Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
            private Mock<IFinancialAidOfficeRepository> officeRepositoryMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private StudentLoanLimitationService StudentLoanLimitationService;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                studentId = currentUserFactory.CurrentUser.PersonId;

                testStudentAwardYearRepository = new TestStudentAwardYearRepository();
                testOfficeRepository = new TestFinancialAidOfficeRepository();

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYearEntities = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                //var studentAwardYear = studentAwardYearEntities.OrderByDescending(say => say.Code).FirstOrDefault();

                testStudentLoanLimitationRepository = new TestStudentLoanLimitationRepository();
                inputStudentLoanLimitationEntities = testStudentLoanLimitationRepository.GetStudentLoanLimitationsAsync(studentId, studentAwardYearEntities).Result;

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));

                StudentLoanLimitationRepositoryMock = new Mock<IStudentLoanLimitationRepository>();
                //StudentLoanLimitationRepositoryMock.Setup(l => l.GetStudentLoanLimitations(studentId, It.Is<IEnumerable<StudentAwardYear>>(ls => ls.First().Equals(studentAwardYear)))).Returns(inputStudentLoanLimitationEntities);
                StudentLoanLimitationRepositoryMock.Setup(l => l.GetStudentLoanLimitationsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>())).ReturnsAsync(inputStudentLoanLimitationEntities);

                StudentLoanLimitationDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.StudentLoanLimitation, Dtos.FinancialAid.StudentLoanLimitation>(adapterRegistryMock.Object, loggerMock.Object);
                expectedStudentLoanLimitations = new List<Dtos.FinancialAid.StudentLoanLimitation>();
                foreach (var letterEntity in inputStudentLoanLimitationEntities)
                {
                    expectedStudentLoanLimitations.Add(StudentLoanLimitationDtoAdapter.MapToType(letterEntity));
                }

                adapterRegistryMock.Setup<ITypeAdapter<Domain.FinancialAid.Entities.StudentLoanLimitation, Dtos.FinancialAid.StudentLoanLimitation>>(
                    a => a.GetAdapter<Domain.FinancialAid.Entities.StudentLoanLimitation, Dtos.FinancialAid.StudentLoanLimitation>()
                    ).Returns(StudentLoanLimitationDtoAdapter);

                BuildService();

                actualStudentLoanLimitations = await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId);
            }

            private void BuildService()
            {
                StudentLoanLimitationService = new StudentLoanLimitationService(adapterRegistryMock.Object,
                                    StudentLoanLimitationRepositoryMock.Object,
                                    studentAwardYearRepositoryMock.Object,
                                    officeRepositoryMock.Object,
                                    baseConfigurationRepository,
                                    currentUserFactory,
                                    roleRepositoryMock.Object,
                                    loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testStudentLoanLimitationRepository = null;
                inputStudentLoanLimitationEntities = null;
                StudentLoanLimitationDtoAdapter = null;
                expectedStudentLoanLimitations = null;
                actualStudentLoanLimitations = null;
                StudentLoanLimitationRepositoryMock = null;
                StudentLoanLimitationService = null;
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedStudentLoanLimitations);
                Assert.IsNotNull(actualStudentLoanLimitations);
            }

            [TestMethod]
            public void NumStudentLoanLimitationsAreEqualTest()
            {
                Assert.IsTrue(expectedStudentLoanLimitations.Count() > 0);
                Assert.IsTrue(actualStudentLoanLimitations.Count() > 0);
                Assert.AreEqual(expectedStudentLoanLimitations.Count(), actualStudentLoanLimitations.Count());
            }

            [TestMethod]
            public void StudentLoanLimitationProperties_EqualsTest()
            {
                var StudentLoanLimitationProperties = typeof(Dtos.FinancialAid.StudentLoanLimitation).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.IsTrue(StudentLoanLimitationProperties.Length > 0);
                foreach (var expectedLetter in expectedStudentLoanLimitations)
                {
                    var actualLetter = expectedStudentLoanLimitations.First(a => a.AwardYear == expectedLetter.AwardYear);
                    foreach (var property in StudentLoanLimitationProperties)
                    {
                        var expectedValue = property.GetValue(expectedLetter, null);
                        var actualValue = property.GetValue(actualLetter, null);
                        Assert.AreEqual(expectedValue, actualValue);
                    }
                }
            }

            [TestMethod]
            public async Task EmptyStudentLoanLimitationListTest()
            {
                var studentAwardYear = studentAwardYearEntities.OrderByDescending(say => say.Code).FirstOrDefault();

                StudentLoanLimitationRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.StudentLoanLimitation>>>(
                    l => l.GetStudentLoanLimitationsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>())).ReturnsAsync(new List<Domain.FinancialAid.Entities.StudentLoanLimitation>());

                actualStudentLoanLimitations = await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId);

                Assert.IsNotNull(actualStudentLoanLimitations);
                Assert.IsTrue(actualStudentLoanLimitations.Count() == 0);
            }

            [TestMethod]
            public async Task NullStudentLoanLimitationListTest()
            {
                var studentAwardYear = studentAwardYearEntities.OrderByDescending(say => say.Code).FirstOrDefault();
                IEnumerable<Domain.FinancialAid.Entities.StudentLoanLimitation> nullList = null;
                StudentLoanLimitationRepositoryMock.Setup(
                    l => l.GetStudentLoanLimitationsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>()))
                    .ReturnsAsync(nullList);

                actualStudentLoanLimitations = await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId);

                Assert.IsNotNull(actualStudentLoanLimitations);
                Assert.IsTrue(actualStudentLoanLimitations.Count() == 0);

                loggerMock.Verify(l => l.Info(string.Format("No LoanLimitations exist for student {0}", studentId)));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(null);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserNotSelfNoPermissions_CannotAccessDataTest()
            {
                studentId = "foobar";
                await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserHasPermissions_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                StudentLoanLimitationService = new StudentLoanLimitationService(adapterRegistryMock.Object,
                    StudentLoanLimitationRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                Assert.IsNotNull(await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId));
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsCounselorHasNoPermissions_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                BuildService();

                await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildService();

                Assert.IsNotNull(await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsProxyForDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();
                BuildService();

                await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId);
            }

            [TestMethod]
            public async Task PermissionExceptionLogsErrorTest()
            {
                var currentUserId = studentId;
                studentId = "foobar";

                bool permissionExceptionCaught = false;
                var message = string.Format("{0} does not have permission to access loan summary information for {1}", currentUserId, studentId);
                try
                {
                    await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId);
                }
                catch (PermissionsException)
                {
                    permissionExceptionCaught = true;
                }

                Assert.IsTrue(permissionExceptionCaught);

                loggerMock.Verify(l => l.Error(message));
            }
        }
    }
}
