//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Moq;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class AverageAwardPackageServiceTests
    {
        [TestClass]
        public class GetAverageAwardPackageTests : FinancialAidServiceTestsSetup
        {
            private string studentId;
            //private Domain.FinancialAid.Entities.StudentAwardYear studentAwardYear;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYearEntities;

            private TestStudentAwardYearRepository testStudentAwardYearRepository;
            private TestFinancialAidOfficeRepository testOfficeRepository;
            private TestAverageAwardPackageRepository testAverageAwardPackageRepository;

            private List<Domain.FinancialAid.Entities.AverageAwardPackage> inputAverageAwardPackageEntities;
            private AutoMapperAdapter<Domain.FinancialAid.Entities.AverageAwardPackage, Dtos.FinancialAid.AverageAwardPackage> averageAwardPackageDtoAdapter;

            private List<Dtos.FinancialAid.AverageAwardPackage> expectedAverageAwardPackages;
            private IEnumerable<Dtos.FinancialAid.AverageAwardPackage> actualAverageAwardPackages;

            private Mock<IAverageAwardPackageRepository> averageAwardPackageRepositoryMock;
            private Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
            private Mock<IFinancialAidOfficeRepository> officeRepositoryMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private AverageAwardPackageService AverageAwardPackageService;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                testAverageAwardPackageRepository = new TestAverageAwardPackageRepository();
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();
                testOfficeRepository = new TestFinancialAidOfficeRepository();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYearEntities = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                var studentAwardYear = studentAwardYearEntities.OrderByDescending(say => say.Code).FirstOrDefault();

                inputAverageAwardPackageEntities = testAverageAwardPackageRepository.GetAverageAwardPackagesAsync(studentId, studentAwardYearEntities).Result.ToList();

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));
                

                averageAwardPackageRepositoryMock = new Mock<IAverageAwardPackageRepository>();
                averageAwardPackageRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.AverageAwardPackage>>>(l => l.GetAverageAwardPackagesAsync(studentId, studentAwardYearEntities))
                    .ReturnsAsync(inputAverageAwardPackageEntities);

                averageAwardPackageDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.AverageAwardPackage, Dtos.FinancialAid.AverageAwardPackage>(adapterRegistryMock.Object, loggerMock.Object);

                expectedAverageAwardPackages = new List<Dtos.FinancialAid.AverageAwardPackage>();
                foreach (var inputAverageAwardPackage in inputAverageAwardPackageEntities)
                {
                    expectedAverageAwardPackages.Add(averageAwardPackageDtoAdapter.MapToType(inputAverageAwardPackage));
                }

                adapterRegistryMock.Setup<ITypeAdapter<Domain.FinancialAid.Entities.AverageAwardPackage, Dtos.FinancialAid.AverageAwardPackage>>(
                    a => a.GetAdapter<Domain.FinancialAid.Entities.AverageAwardPackage, Dtos.FinancialAid.AverageAwardPackage>()
                    ).Returns(averageAwardPackageDtoAdapter);

                AverageAwardPackageService = new AverageAwardPackageService(adapterRegistryMock.Object,
                    averageAwardPackageRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAverageAwardPackages = await AverageAwardPackageService.GetAverageAwardPackagesAsync(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testAverageAwardPackageRepository = null;
                inputAverageAwardPackageEntities = null;
                averageAwardPackageDtoAdapter = null;
                expectedAverageAwardPackages = null;
                actualAverageAwardPackages = null;
                averageAwardPackageRepositoryMock = null;
                AverageAwardPackageService = null;
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedAverageAwardPackages);
                Assert.IsNotNull(actualAverageAwardPackages);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await AverageAwardPackageService.GetAverageAwardPackagesAsync("");
            }

            /// <summary>
            /// User is not self nor counselor nor proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserNotSelfNorProxyNoPermissionsTest()
            {
                studentId = "mountaineers";
                await AverageAwardPackageService.GetAverageAwardPackagesAsync(studentId);
            }

            /// <summary>
            /// Current user is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserWithPermissionCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                AverageAwardPackageService = new AverageAwardPackageService(adapterRegistryMock.Object,
                    averageAwardPackageRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
                bool exceptionThrown = false;
                try
                {
                    await AverageAwardPackageService.GetAverageAwardPackagesAsync(studentId);
                } catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            /// <summary>
            /// Current user is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsProxyCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                AverageAwardPackageService = new AverageAwardPackageService(adapterRegistryMock.Object,
                    averageAwardPackageRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
                bool exceptionThrown = false;
                try
                {
                    await AverageAwardPackageService.GetAverageAwardPackagesAsync(studentId);
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            [TestMethod]
            public async Task PermissionExceptionLogsErrorTest()
            {
                var currentUserId = studentId;
                studentId = "mountaineers";

                bool permissionExceptionCaught = false;
                var message = string.Format("{0} does not have permission to access average award package information for {1}", currentUserId, studentId);
                try
                {
                    await AverageAwardPackageService.GetAverageAwardPackagesAsync(studentId);
                }
                catch (PermissionsException)
                {
                    permissionExceptionCaught = true;
                }

                Assert.IsTrue(permissionExceptionCaught);

                loggerMock.Verify(l => l.Error(message));
            }

            /// <summary>
            /// Tests if a message is logged if the repository did not return any
            /// average award package objects
            /// </summary>
            [TestMethod]
            public async Task NoAveragePackagesReturnedMessageLoggedTest()
            {
                var message = "No AverageAwardPackages returned by repository";
                inputAverageAwardPackageEntities = null;

                averageAwardPackageRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.AverageAwardPackage>>>(l => l.GetAverageAwardPackagesAsync(studentId, studentAwardYearEntities))
                    .ReturnsAsync(inputAverageAwardPackageEntities);
                actualAverageAwardPackages = await AverageAwardPackageService.GetAverageAwardPackagesAsync(studentId);

                loggerMock.Verify(l => l.Info(message));
                Assert.IsTrue(actualAverageAwardPackages.Count() == 0);
            }

            /// <summary>
            /// Tests if the number of returned dtos matches the number of received non-null
            /// average award packages from the repository
            /// </summary>
            [TestMethod]
            public async Task NumberOfReturnedDtos_MatchesNumberOfPackageEntitiesTest()
            {
                var packageEntitiesCount = inputAverageAwardPackageEntities.ToList().FindAll(iap => iap != null).Count();
                var packageDtos = await AverageAwardPackageService.GetAverageAwardPackagesAsync(studentId);

                Assert.AreEqual(packageEntitiesCount, packageDtos.Count());

            }

            /// <summary>
            /// Tests if an appropriate message is logged whenever an average award package
            /// returned from the repository is null
            /// </summary>
            [TestMethod]
            public async Task NullAwardPackageMessageLoggedTest()
            {
                var message = "Null AverageAwardPackage returned by repository";
                inputAverageAwardPackageEntities.Add(null);

                averageAwardPackageRepositoryMock.Setup<Task<IEnumerable<Domain.FinancialAid.Entities.AverageAwardPackage>>>(l => l.GetAverageAwardPackagesAsync(studentId, studentAwardYearEntities))
                    .ReturnsAsync(inputAverageAwardPackageEntities);
                await AverageAwardPackageService.GetAverageAwardPackagesAsync(studentId);

                loggerMock.Verify(l => l.Info(message));

            }

        }
    }
}