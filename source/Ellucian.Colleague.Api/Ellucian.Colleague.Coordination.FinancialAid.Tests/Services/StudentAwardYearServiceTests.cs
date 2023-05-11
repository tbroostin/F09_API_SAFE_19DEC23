//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    public class BaseStudentAwardYearServiceSetup : FinancialAidServiceTestsSetup
    {
        public TestFinancialAidOfficeRepository testOfficeRepository;
        public AutoMapperAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.FinancialAid.StudentAwardYear> StudentAwardYearDtoAdapter;
        public AutoMapperAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.Student.StudentAwardYear2> StudentAwardYearEntityToDto2Adapter;

        public Mock<IStudentAwardYearRepository> StudentAwardYearRepositoryMock;
        public Mock<IFinancialAidOfficeRepository> OfficeRepositoryMock;

        public StudentAwardYearService StudentAwardYearService;

        public void StudentAwardYearsTestInitialize()
        {
            BaseInitialize();

            testOfficeRepository = new TestFinancialAidOfficeRepository();

            StudentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();

            StudentAwardYearDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.FinancialAid.StudentAwardYear>(adapterRegistryMock.Object, loggerMock.Object);
            StudentAwardYearEntityToDto2Adapter = new StudentAwardYearEntityToDto2Adapter(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup<ITypeAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.FinancialAid.StudentAwardYear>>(
                    a => a.GetAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.FinancialAid.StudentAwardYear>()
                    ).Returns(StudentAwardYearDtoAdapter);

            adapterRegistryMock.Setup<ITypeAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.Student.StudentAwardYear2>>(
                    a => a.GetAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.Student.StudentAwardYear2>()
                    ).Returns(StudentAwardYearEntityToDto2Adapter);

            OfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
            OfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());
        }

        public void StudentAwardTestCleanup()
        {
            BaseCleanup();
            StudentAwardYearDtoAdapter = null;
            StudentAwardYearRepositoryMock = null;
            StudentAwardYearService = null;
        }

    }
    [TestClass]
    public class StudentAwardYearServiceTests
    {
        [TestClass]
        public class GetStudentAwardYearsTests : BaseStudentAwardYearServiceSetup
        {
            private string studentId;
            private TestStudentAwardYearRepository testStudentAwardYearRepository;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> inputStudentAwardYearEntities;
            private List<Dtos.FinancialAid.StudentAwardYear> expectedStudentAwardYears;
            private IEnumerable<Dtos.FinancialAid.StudentAwardYear> actualStudentAwardYears;

            [TestInitialize]
            public void Initialize()
            {
                StudentAwardYearsTestInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                inputStudentAwardYearEntities = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
                StudentAwardYearRepositoryMock.Setup(l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), true)).ReturnsAsync(inputStudentAwardYearEntities);

                expectedStudentAwardYears = new List<Dtos.FinancialAid.StudentAwardYear>();
                foreach (var letterEntity in inputStudentAwardYearEntities.Where(y => y.IsActive))
                {
                    expectedStudentAwardYears.Add(StudentAwardYearDtoAdapter.MapToType(letterEntity));
                }

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
#pragma warning disable 618
                actualStudentAwardYears = StudentAwardYearService.GetStudentAwardYears(studentId);
#pragma warning restore 618
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardTestCleanup();
                studentId = null;
                expectedStudentAwardYears = null;
                actualStudentAwardYears = null;
                testStudentAwardYearRepository = null;
                inputStudentAwardYearEntities = null;
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedStudentAwardYears);
                Assert.IsNotNull(actualStudentAwardYears);
            }

            [TestMethod]
            public void NumStudentAwardYearsAreEqualTest()
            {
                Assert.IsTrue(expectedStudentAwardYears.Count() > 0);
                Assert.IsTrue(actualStudentAwardYears.Count() > 0);
                Assert.AreEqual(expectedStudentAwardYears.Count(), actualStudentAwardYears.Count());
            }

            [TestMethod]
            public void StudentAwardYearProperties_EqualsTest()
            {
                var StudentAwardYearProperties = typeof(Dtos.FinancialAid.StudentAwardYear).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.IsTrue(StudentAwardYearProperties.Length > 0);
                foreach (var expectedLetter in expectedStudentAwardYears)
                {
                    var actualLetter = expectedStudentAwardYears.First(a => a.Code == expectedLetter.Code);
                    foreach (var property in StudentAwardYearProperties)
                    {
                        var expectedValue = property.GetValue(expectedLetter, null);
                        var actualValue = property.GetValue(actualLetter, null);
                        Assert.AreEqual(expectedValue, actualValue);
                    }
                }
            }

            [TestMethod]
            public void EmptyStudentAwardYearListTest()
            {
#pragma warning disable 618
                StudentAwardYearRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>>(
                    l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), true)).ReturnsAsync(new List<Domain.FinancialAid.Entities.StudentAwardYear>());

                actualStudentAwardYears = StudentAwardYearService.GetStudentAwardYears(studentId);
#pragma warning restore 618
                Assert.IsNotNull(actualStudentAwardYears);
                Assert.IsTrue(actualStudentAwardYears.Count() == 0);
            }

            [TestMethod]
            public void NullStudentAwardYearListTest()
            {
                IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> nullList = null;
#pragma warning disable 618
                StudentAwardYearRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>>(
                    l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), true)).ReturnsAsync(nullList);

                actualStudentAwardYears = StudentAwardYearService.GetStudentAwardYears(studentId);
#pragma warning restore 618
                Assert.IsNotNull(actualStudentAwardYears);
                Assert.IsTrue(actualStudentAwardYears.Count() == 0);
            }

            [TestMethod]
            public void NullStudentAwardYearListLogsInfoTest()
            {
                IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> nullList = null;
#pragma warning disable 618
                StudentAwardYearRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>>(
                    l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), true)).ReturnsAsync(nullList);

                actualStudentAwardYears = StudentAwardYearService.GetStudentAwardYears(studentId);
#pragma warning restore 618
                loggerMock.Verify(l => l.Debug(It.IsAny<string>()));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
#pragma warning disable 618
                StudentAwardYearService.GetStudentAwardYears(null);
#pragma warning restore 618
            }

            /// <summary>
            /// User is not self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserNotSelfNoPermissionsNotProxy_CannotAccessDataTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));
                studentId = "foobar";
#pragma warning disable 618
                StudentAwardYearService.GetStudentAwardYears(studentId);
#pragma warning restore 618
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            [TestMethod]
            public void CurrentUserHasPermissions_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
#pragma warning disable 618
                actualStudentAwardYears = StudentAwardYearService.GetStudentAwardYears(studentId);
#pragma warning restore 618
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserHasNoPermissions_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();                

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
#pragma warning disable 618
                StudentAwardYearService.GetStudentAwardYears(studentId);
#pragma warning restore 618
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void CurrentIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
#pragma warning disable 618
                actualStudentAwardYears = StudentAwardYearService.GetStudentAwardYears(studentId);
#pragma warning restore 618
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserIsProxyForDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
#pragma warning disable 618
                StudentAwardYearService.GetStudentAwardYears(studentId);
#pragma warning restore 618
            }

            [TestMethod]
            public void PermissionExceptionLogsErrorTest()
            {
                var currentUserId = studentId;
                studentId = "foobar";

                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));

                bool permissionExceptionCaught = false;
                var message = string.Format("{0} does not have permission to access award year information for {1}", currentUserId, studentId);
                try
                {
#pragma warning disable 618
                    StudentAwardYearService.GetStudentAwardYears(studentId);
#pragma warning restore 618
                }
                catch (PermissionsException)
                {
                    permissionExceptionCaught = true;
                }

                Assert.IsTrue(permissionExceptionCaught);

                loggerMock.Verify(l => l.Error(message));
            }
        }
        
        [TestClass]
        public class GetStudentAwardYears2AsyncTests : BaseStudentAwardYearServiceSetup
        {
            private string studentId;
            private TestStudentAwardYearRepository testStudentAwardYearRepository;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> inputStudentAwardYearEntities;
            private List<Dtos.Student.StudentAwardYear2> expectedStudentAwardYears;
            private IEnumerable<Dtos.Student.StudentAwardYear2> actualStudentAwardYears;

            [TestInitialize]
            public async void Initialize()
            {
                StudentAwardYearsTestInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                inputStudentAwardYearEntities = await testStudentAwardYearRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService);
                StudentAwardYearRepositoryMock.Setup(l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(inputStudentAwardYearEntities);

                expectedStudentAwardYears = new List<Dtos.Student.StudentAwardYear2>();
                foreach (var letterEntity in inputStudentAwardYearEntities.Where(y => y.IsActive))
                {
                    expectedStudentAwardYears.Add(StudentAwardYearEntityToDto2Adapter.MapToType(letterEntity));
                }

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualStudentAwardYears = await StudentAwardYearService.GetStudentAwardYears2Async(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardTestCleanup();
                studentId = null;
                expectedStudentAwardYears = null;
                actualStudentAwardYears = null;
                testStudentAwardYearRepository = null;
                inputStudentAwardYearEntities = null;
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedStudentAwardYears);
                Assert.IsNotNull(actualStudentAwardYears);
            }

            [TestMethod]
            public void NumStudentAwardYearsAreEqualTest()
            {
                Assert.IsTrue(expectedStudentAwardYears.Count() > 0);
                Assert.IsTrue(actualStudentAwardYears.Count() > 0);
                Assert.AreEqual(expectedStudentAwardYears.Count(), actualStudentAwardYears.Count());
            }

            [TestMethod]
            public void StudentAwardYearProperties_EqualsTest()
            {
                var StudentAwardYearProperties = typeof(Dtos.Student.StudentAwardYear2).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.IsTrue(StudentAwardYearProperties.Length > 0);
                foreach (var expectedYear in expectedStudentAwardYears)
                {
                    var actualLetter = expectedStudentAwardYears.First(a => a.Code == expectedYear.Code);
                    foreach (var property in StudentAwardYearProperties)
                    {
                        var expectedValue = property.GetValue(expectedYear, null);
                        var actualValue = property.GetValue(actualLetter, null);
                        Assert.AreEqual(expectedValue, actualValue);
                    }
                }
            }

            [TestMethod]
            public async Task EmptyStudentAwardYearListTest()
            {
                StudentAwardYearRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>>(
                    l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(new List<Domain.FinancialAid.Entities.StudentAwardYear>());

                actualStudentAwardYears = await StudentAwardYearService.GetStudentAwardYears2Async(studentId);

                Assert.IsNotNull(actualStudentAwardYears);
                Assert.IsTrue(actualStudentAwardYears.Count() == 0);
            }

            [TestMethod]
            public async Task NullStudentAwardYearListTest()
            {
                IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> nullList = null;
                StudentAwardYearRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>>(
                    l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(nullList);

                actualStudentAwardYears = await StudentAwardYearService.GetStudentAwardYears2Async(studentId);

                Assert.IsNotNull(actualStudentAwardYears);
                Assert.IsTrue(actualStudentAwardYears.Count() == 0);
            }

            [TestMethod]
            public async Task NullStudentAwardYearListLogsInfoTest()
            {
                IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> nullList = null;
                StudentAwardYearRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>>(
                    l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(nullList);

                actualStudentAwardYears = await StudentAwardYearService.GetStudentAwardYears2Async(studentId);

                var message = string.Format("No studentAwardYears are available for student {0}", studentId);

                //loggerMock.Verify(l => l.Debug(It.IsAny<string>()));
                Assert.IsTrue(actualStudentAwardYears.Count() == 0);
                loggerMock.Verify(l => l.Debug("No studentAwardYears are available for student {0}", It.IsAny<string>()));
            }

            /// <summary>
            /// Tests if all student award years are returned no matter if the SelfService flag is on or off
            /// </summary>
            [TestMethod]
            public async Task AllStudentAwardYearsAreReturnedTest()
            {
                inputStudentAwardYearEntities.ToList().First().CurrentConfiguration.IsSelfServiceActive = false;
                StudentAwardYearRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>>(
                    l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(inputStudentAwardYearEntities);

                actualStudentAwardYears = await StudentAwardYearService.GetStudentAwardYears2Async(studentId);
                Assert.AreEqual(inputStudentAwardYearEntities.Count(), actualStudentAwardYears.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await StudentAwardYearService.GetStudentAwardYears2Async(null);
            }

            /// <summary>
            /// User is not self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserNotSelfNoPermissionsNotProxy_CannotAccessDataTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));
                studentId = "foobar";
                await StudentAwardYearService.GetStudentAwardYears2Async(studentId);
            }

            /// <summary>
            /// User is a counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserHasPermissions_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                Assert.IsNotNull(await StudentAwardYearService.GetStudentAwardYears2Async(studentId));
            }

            /// <summary>
            /// User is a counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserHasNoPermissions_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await StudentAwardYearService.GetStudentAwardYears2Async(studentId);
            }

            /// <summary>
            /// User is a proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                Assert.IsNotNull(await StudentAwardYearService.GetStudentAwardYears2Async(studentId));
            }

            /// <summary>
            /// User is a proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsProxyForDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await StudentAwardYearService.GetStudentAwardYears2Async(studentId);
            }

            [TestMethod]
            public async Task PermissionExceptionLogsErrorTest()
            {
                var currentUserId = studentId;
                studentId = "foobar";

                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));

                bool permissionExceptionCaught = false;
                var message = string.Format("{0} does not have permission to access award year information for {1}", currentUserId, studentId);
                try
                {
                    await StudentAwardYearService.GetStudentAwardYears2Async(studentId);
                }
                catch (PermissionsException)
                {
                    permissionExceptionCaught = true;
                }

                Assert.IsTrue(permissionExceptionCaught);

                loggerMock.Verify(l => l.Error(message));
            }
        }

        [TestClass]
        public class GetStudentAwardYearTests : BaseStudentAwardYearServiceSetup
        {
            private string studentId;
            private TestStudentAwardYearRepository testStudentAwardYearRepository;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> inputStudentAwardYearEntities;
            private Dtos.FinancialAid.StudentAwardYear expectedStudentAwardYear;
            private Dtos.FinancialAid.StudentAwardYear actualStudentAwardYear;

            [TestInitialize]
            public void Initialize()
            {
                StudentAwardYearsTestInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                inputStudentAwardYearEntities = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                StudentAwardYearRepositoryMock.Setup(l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), true)).ReturnsAsync(inputStudentAwardYearEntities);

                expectedStudentAwardYear = StudentAwardYearDtoAdapter.MapToType(inputStudentAwardYearEntities.First());

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
#pragma warning disable 618
                actualStudentAwardYear = StudentAwardYearService.GetStudentAwardYear(studentId, expectedStudentAwardYear.Code);
#pragma warning restore 618
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardTestCleanup();
                studentId = null;
                expectedStudentAwardYear = null;
                actualStudentAwardYear = null;
                testStudentAwardYearRepository = null;
                inputStudentAwardYearEntities = null;
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedStudentAwardYear);
                Assert.IsNotNull(actualStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullStudentId_ExceptionThrownTest()
            {
#pragma warning disable 618
                actualStudentAwardYear = StudentAwardYearService.GetStudentAwardYear(null, expectedStudentAwardYear.Code);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullAwardYearCode_ExceptionThrownTest()
            {
#pragma warning disable 618
                actualStudentAwardYear = StudentAwardYearService.GetStudentAwardYear(studentId, null);
#pragma warning restore 618
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void NullStudentAwardYearDto_ExceptionThrownTest()
            {
                var badAwardYear = "bad year";
                try
                {
#pragma warning disable 618
                    StudentAwardYearService.GetStudentAwardYear(studentId, badAwardYear);
#pragma warning restore 618
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No student award year exists for {0} for student {1}", badAwardYear, studentId)));
                    throw;
                }
            }
        }

        [TestClass]
        public class GetStudentAwardYear2AsyncTests : BaseStudentAwardYearServiceSetup
        {
            private string studentId;
            private TestStudentAwardYearRepository testStudentAwardYearRepository;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> inputStudentAwardYearEntities;
            private Dtos.Student.StudentAwardYear2 expectedStudentAwardYear;
            private Dtos.Student.StudentAwardYear2 actualStudentAwardYear;

            [TestInitialize]
            public async void Initialize()
            {
                StudentAwardYearsTestInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                inputStudentAwardYearEntities = await testStudentAwardYearRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService);

                StudentAwardYearRepositoryMock.Setup(l => l.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(inputStudentAwardYearEntities);

                expectedStudentAwardYear = StudentAwardYearEntityToDto2Adapter.MapToType(inputStudentAwardYearEntities.First());

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualStudentAwardYear = await StudentAwardYearService.GetStudentAwardYear2Async(studentId, expectedStudentAwardYear.Code);
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardTestCleanup();
                studentId = null;
                expectedStudentAwardYear = null;
                actualStudentAwardYear = null;
                testStudentAwardYearRepository = null;
                inputStudentAwardYearEntities = null;
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedStudentAwardYear);
                Assert.IsNotNull(actualStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentId_ExceptionThrownTest()
            {
                await StudentAwardYearService.GetStudentAwardYear2Async(null, expectedStudentAwardYear.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardYearCode_ExceptionThrownTest()
            {
                await StudentAwardYearService.GetStudentAwardYear2Async(studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullStudentAwardYearDto_ExceptionThrownTest()
            {
                var badAwardYear = "bad year";
                try
                {
                    await StudentAwardYearService.GetStudentAwardYear2Async(studentId, badAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No student award year exists for {0} for student {1}", badAwardYear, studentId)));
                    throw;
                }
            }
        }

        [TestClass]
        public class UpdateStudentAwardYear : BaseStudentAwardYearServiceSetup
        {
            private string studentId;
            private TestStudentAwardYearRepository testStudentAwardYearRepository;
            private Domain.FinancialAid.Entities.StudentAwardYear inputStudentAwardYearEntity;
            private Dtos.FinancialAid.StudentAwardYear inputStudentAwardYearDto;
            private Dtos.FinancialAid.StudentAwardYear expectedStudentAwardYear;
            private Dtos.FinancialAid.StudentAwardYear actualStudentAwardYear;

            private StudentAwardYearDtoToEntityAdapter studentAwardYearDtoAdapter;

            [TestInitialize]
            public void Initialize()
            {
                StudentAwardYearsTestInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();

                inputStudentAwardYearDto = new Dtos.FinancialAid.StudentAwardYear()
                {
                    StudentId = "0003914",
                    Code = "2014",
                    IsPaperCopyOptionSelected = true
                };

                studentAwardYearDtoAdapter = new FinancialAid.Adapters.StudentAwardYearDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                inputStudentAwardYearEntity = studentAwardYearDtoAdapter.MapToType(inputStudentAwardYearDto);

                StudentAwardYearRepositoryMock.Setup(ae => ae.UpdateStudentAwardYear(It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>())).Returns(inputStudentAwardYearEntity);

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.FinancialAid.StudentAwardYear>())
                    .Returns(new AutoMapperAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.FinancialAid.StudentAwardYear>(adapterRegistryMock.Object, loggerMock.Object));

                adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.FinancialAid.StudentAwardYear, Domain.FinancialAid.Entities.StudentAwardYear>()).Returns(studentAwardYearDtoAdapter);

                var studentAwardYearEntityAdapter = adapterRegistryMock.Object.GetAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.FinancialAid.StudentAwardYear>();
                expectedStudentAwardYear = studentAwardYearEntityAdapter.MapToType(inputStudentAwardYearEntity);

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardTestCleanup();
                studentId = null;
                expectedStudentAwardYear = null;
                actualStudentAwardYear = null;
                testStudentAwardYearRepository = null;
                inputStudentAwardYearEntity = null;
                inputStudentAwardYearDto = null;
            }

            /// <summary>
            /// Tests if attributes of the actual award year equal to the
            /// corresponding expected ones
            /// </summary>
            [TestMethod]
            public void ActualEqualsExpectedTest()
            {
#pragma warning disable 618
                actualStudentAwardYear = StudentAwardYearService.UpdateStudentAwardYear(inputStudentAwardYearDto);
#pragma warning restore 618
                Assert.AreEqual(expectedStudentAwardYear.StudentId, actualStudentAwardYear.StudentId);
                Assert.AreEqual(expectedStudentAwardYear.Code, actualStudentAwardYear.Code);
                Assert.AreEqual(expectedStudentAwardYear.IsPaperCopyOptionSelected, actualStudentAwardYear.IsPaperCopyOptionSelected);
            }

            /// <summary>
            /// Tests if ArgumentNullException is thrown when input student award year
            /// dto is null
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullStudentAwardYearDto_ExceptionThrownTest()
            {
#pragma warning disable 618
                StudentAwardYearService.UpdateStudentAwardYear(null);
#pragma warning restore 618
            }

            /// <summary>
            /// Tests if ArgumentException is thrown when the student id
            /// property of the input student award year is null
            /// </summary>
            [TestMethod]
            public void StudentIdIsNull_ExceptionThrownTest()
            {
                inputStudentAwardYearDto.StudentId = null;
                var exceptionCaught = false;
                try
                {
#pragma warning disable 618
                    StudentAwardYearService.UpdateStudentAwardYear(inputStudentAwardYearDto);
#pragma warning restore 618
                }
                catch (ArgumentException)
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error("Input argument studentAwardYear is invalid. StudentId cannot be null or empty"));
            }

            /// <summary>
            /// Tests if PermissionException is thrown when the student id of the input award year dto
            /// does not match the current user's one
            /// </summary>
            [TestMethod]
            public void StudentIdIsNotCurrentUsers_ExceptionThrownTest()
            {
                inputStudentAwardYearDto.StudentId = "foobar";
                var exceptionCaught = false;
                try
                {
#pragma warning disable 618
                    StudentAwardYearService.UpdateStudentAwardYear(inputStudentAwardYearDto);
#pragma warning restore 618
                }
                catch (PermissionsException)
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            /// <summary>
            /// Tests if Exception is thrown when null is received from the update
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public void UpdatedAwardYearIsNull_ExceptionThrownTest()
            {
                StudentAwardYear updatedStudentAwardYearEntity = null;
                StudentAwardYearRepositoryMock.Setup(r => r.UpdateStudentAwardYear(It.IsAny<StudentAwardYear>())).Returns(updatedStudentAwardYearEntity);

                try
                {
#pragma warning disable 618
                    StudentAwardYearService.UpdateStudentAwardYear(inputStudentAwardYearDto);
#pragma warning restore 618
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Null student award year object returned by repository update method for student {0} award year {1}", inputStudentAwardYearEntity.StudentId, inputStudentAwardYearEntity.Code)));
                    throw;
                }
            }
        }

        [TestClass]
        public class UpdateStudentAwardYear2AsyncTests : BaseStudentAwardYearServiceSetup
        {
            private string studentId;
            private TestStudentAwardYearRepository testStudentAwardYearRepository;
            private Domain.FinancialAid.Entities.StudentAwardYear inputStudentAwardYearEntity;
            private StudentAwardYear2 inputStudentAwardYearDto;
            private Dtos.Student.StudentAwardYear2 expectedStudentAwardYear;
            private Dtos.Student.StudentAwardYear2 actualStudentAwardYear;

            private StudentAwardYear2DtoToEntityAdapter studentAwardYear2DtoToEntityAdapter;

            [TestInitialize]
            public async void Initialize()
            {
                StudentAwardYearsTestInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();

                inputStudentAwardYearDto = new Dtos.Student.StudentAwardYear2()
                {
                    StudentId = "0003914",
                    Code = "2014",
                    IsPaperCopyOptionSelected = true
                };

                studentAwardYear2DtoToEntityAdapter = new FinancialAid.Adapters.StudentAwardYear2DtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                inputStudentAwardYearEntity = studentAwardYear2DtoToEntityAdapter.MapToType(inputStudentAwardYearDto);

                StudentAwardYearRepositoryMock.Setup(ae => ae.UpdateStudentAwardYearAsync(It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>())).ReturnsAsync(inputStudentAwardYearEntity);

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.Student.StudentAwardYear2>())
                    .Returns(new AutoMapperAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.Student.StudentAwardYear2>(adapterRegistryMock.Object, loggerMock.Object));

                adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.Student.StudentAwardYear2, Domain.FinancialAid.Entities.StudentAwardYear>()).Returns(studentAwardYear2DtoToEntityAdapter);

                var studentAwardYearEntityAdapter = adapterRegistryMock.Object.GetAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.Student.StudentAwardYear2>();
                expectedStudentAwardYear = studentAwardYearEntityAdapter.MapToType(inputStudentAwardYearEntity);

                StudentAwardYearService = new StudentAwardYearService(adapterRegistryMock.Object,
                    StudentAwardYearRepositoryMock.Object,
                    OfficeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardTestCleanup();
                studentId = null;
                expectedStudentAwardYear = null;
                actualStudentAwardYear = null;
                testStudentAwardYearRepository = null;
                inputStudentAwardYearEntity = null;
                inputStudentAwardYearDto = null;
            }

            /// <summary>
            /// Tests if attributes of the actual award year equal to the
            /// corresponding expected ones
            /// </summary>
            [TestMethod]
            public async Task ActualEqualsExpectedTest()
            {
                actualStudentAwardYear = await StudentAwardYearService.UpdateStudentAwardYear2Async(inputStudentAwardYearDto);
                Assert.AreEqual(expectedStudentAwardYear.StudentId, actualStudentAwardYear.StudentId);
                Assert.AreEqual(expectedStudentAwardYear.Code, actualStudentAwardYear.Code);
                Assert.AreEqual(expectedStudentAwardYear.IsPaperCopyOptionSelected, actualStudentAwardYear.IsPaperCopyOptionSelected);
            }

            /// <summary>
            /// Tests if ArgumentNullException is thrown when input student award year
            /// dto is null
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentAwardYearDto_ExceptionThrownTest()
            {
                await StudentAwardYearService.UpdateStudentAwardYear2Async(null);
            }

            /// <summary>
            /// Tests if ArgumentException is thrown when the student id
            /// property of the input student award year is null
            /// </summary>
            [TestMethod]
            public async Task StudentIdIsNull_ExceptionThrownTest()
            {
                inputStudentAwardYearDto.StudentId = null;
                var exceptionCaught = false;
                try
                {
                    await StudentAwardYearService.UpdateStudentAwardYear2Async(inputStudentAwardYearDto);
                }
                catch (ArgumentException)
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error("Input argument studentAwardYear is invalid. StudentId cannot be null or empty"));
            }

            /// <summary>
            /// Tests if PermissionException is thrown when the student id of the input award year dto
            /// does not match the current user's one
            /// </summary>
            [TestMethod]
            public async Task StudentIdIsNotCurrentUsers_ExceptionThrownTest()
            {
                inputStudentAwardYearDto.StudentId = "foobar";
                var exceptionCaught = false;
                try
                {
                    await StudentAwardYearService.UpdateStudentAwardYear2Async(inputStudentAwardYearDto);
                }
                catch (PermissionsException)
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            /// <summary>
            /// Tests if Exception is thrown when null is received from the update
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task UpdatedAwardYearIsNull_ExceptionThrownTest()
            {
                StudentAwardYear updatedStudentAwardYearEntity = null;
                StudentAwardYearRepositoryMock.Setup(r => r.UpdateStudentAwardYearAsync(It.IsAny<StudentAwardYear>())).ReturnsAsync(updatedStudentAwardYearEntity);

                try
                {
                    await StudentAwardYearService.UpdateStudentAwardYear2Async(inputStudentAwardYearDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Null student award year object returned by repository update method for student {0} award year {1}", inputStudentAwardYearEntity.StudentId, inputStudentAwardYearEntity.Code)));
                    throw;
                }
            }
        }
    }
}
