/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class FinancialAidApplicationServiceTests
    {
        [TestClass]
        public class GetFinancialAidApplicationsTests : FinancialAidServiceTestsSetup
        {
            public string studentId;

            public TestFinancialAidOfficeRepository testOfficeRepository;
            public TestStudentAwardYearRepository testStudentAwardYearRepository;
            public TestFafsaRepository testFafsaRepository;
            public TestProfileApplicationRepository testProfileApplicationRepository;

            public IEnumerable<FinancialAidOffice> expectedOffices
            {
                get { return testOfficeRepository.GetFinancialAidOffices(); }
            }
            public IEnumerable<StudentAwardYear> expectedStudentAwardYears
            {
                get { return testStudentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(expectedOffices)); }
            }
            public IEnumerable<Fafsa> expectedFafsas
            {
                get { return testFafsaRepository.GetFafsasAsync(new List<string>() { studentId }, expectedStudentAwardYears.Select(y => y.Code)).Result; }
            }
            public IEnumerable<ProfileApplication> expectedProfileApplications
            {
                get { return testProfileApplicationRepository.GetProfileApplicationsAsync(studentId, expectedStudentAwardYears).Result; }
            }


            public List<Dtos.FinancialAid.FinancialAidApplication> actualFinancialAidApplications
            {
                get { return FinancialAidApplicationService.GetFinancialAidApplications(studentId).ToList(); }
            }

            public Mock<IFinancialAidOfficeRepository> officeRepositoryMock;
            public Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
            public Mock<IFafsaRepository> fafsaRepositoryMock;
            public Mock<IProfileApplicationRepository> profileApplicationRepositoryMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            public FinancialAidApplicationService FinancialAidApplicationService;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                studentId = currentUserFactory.CurrentUser.PersonId;

                testOfficeRepository = new TestFinancialAidOfficeRepository();
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();
                testFafsaRepository = new TestFafsaRepository();
                testProfileApplicationRepository = new TestProfileApplicationRepository();

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                profileApplicationRepositoryMock = new Mock<IProfileApplicationRepository>();


                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();

                FinancialAidApplicationService = BuildService();

            }

            private FinancialAidApplicationService BuildService()
            {
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync())
                    .Returns(() => testOfficeRepository.GetFinancialAidOfficesAsync());

                studentAwardYearRepositoryMock.Setup(s => s.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, officeService, b) => Task.FromResult(testStudentAwardYearRepository.GetStudentAwardYears(id, officeService)));

                fafsaRepositoryMock.Setup(s => s.GetFafsasAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>, IEnumerable<string>>((ids, years) => testFafsaRepository.GetFafsasAsync(ids, years));

                profileApplicationRepositoryMock.Setup(s => s.GetProfileApplicationsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>((id, years) => testProfileApplicationRepository.GetProfileApplicationsAsync(id, years));

                return new FinancialAidApplicationService(
                    adapterRegistryMock.Object,
                    fafsaRepositoryMock.Object,
                    profileApplicationRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(actualFinancialAidApplications);
            }

            [TestMethod]
            public void NumFinancialAidApplicationsEqualToNumStudentAwardYearsTest()
            {
                Assert.IsTrue(expectedStudentAwardYears.Count() > 0);
                Assert.IsTrue(actualFinancialAidApplications.Count() > 0);
                Assert.AreEqual(expectedStudentAwardYears.Count(), actualFinancialAidApplications.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                FinancialAidApplicationService.GetFinancialAidApplications("");
            }

            /// <summary>
            /// User is not self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserNotSelfNoPermissionsTest()
            {
                studentId = "foobar";
                FinancialAidApplicationService.GetFinancialAidApplications(studentId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            [TestMethod]
            public void CurrentUserWithPermission_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                FinancialAidApplicationService = BuildService();

                var financialAidApplications = FinancialAidApplicationService.GetFinancialAidApplications(studentId).ToList();
                Assert.AreEqual(expectedStudentAwardYears.Count(), financialAidApplications.Count);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserIsCounselorWithNoPermission_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();

                FinancialAidApplicationService = BuildService();

                FinancialAidApplicationService.GetFinancialAidApplications(studentId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void CurrentUserIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                
                FinancialAidApplicationService = BuildService();

                var financialAidApplications = FinancialAidApplicationService.GetFinancialAidApplications(studentId).ToList();
                Assert.AreEqual(expectedStudentAwardYears.Count(), financialAidApplications.Count);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserIsProxyForDifferentPerson_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();

                FinancialAidApplicationService = BuildService();

                FinancialAidApplicationService.GetFinancialAidApplications(studentId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PermissionExceptionLogsErrorTest()
            {
                var currentUserId = studentId;
                studentId = "foobar";

                try
                {
                    FinancialAidApplicationService.GetFinancialAidApplications(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to access application information for {1}", currentUserId, studentId)));
                    throw;
                }
            }

            [TestMethod]
            public void NullFafsas_NoFafsasAreCompleteTest()
            {
                IEnumerable<Fafsa> nullFafsas = null;
                fafsaRepositoryMock.Setup(s => s.GetFafsasAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                    .ReturnsAsync(nullFafsas);

                Assert.IsTrue(actualFinancialAidApplications.All(app => !app.IsFafsaComplete));
            }

            [TestMethod]
            public void EmptyFafsas_NoFafsasAreCompleteTest()
            {
                testFafsaRepository.csStudentData = new List<TestFafsaRepository.CsStudentRecord>();

                Assert.IsTrue(actualFinancialAidApplications.All(app => !app.IsFafsaComplete));
            }


            [TestMethod]
            public void EmptyProfileApplications_NoProfileApplicationsAreCompleteTest()
            {
                testProfileApplicationRepository.csStudentData = new List<TestProfileApplicationRepository.CsStudentRecord>();

                Assert.IsTrue(actualFinancialAidApplications.All(app => !app.IsProfileComplete));
            }

            [TestMethod]
            public void NullProfileApplications_NoProfileApplicationsAreCompleteTest()
            {
                IEnumerable<ProfileApplication> nullProfiles = null;
                profileApplicationRepositoryMock.Setup(s => s.GetProfileApplicationsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .ReturnsAsync(nullProfiles);

                Assert.IsTrue(actualFinancialAidApplications.All(app => !app.IsProfileComplete));
            }

            [TestMethod]
            public void FafsaCompleteIfFafsaExistsForAwardYearTest()
            {
                foreach (var expectedStudentAwardYear in expectedStudentAwardYears)
                {
                    var expectedFafsa = expectedFafsas.FirstOrDefault(f => f.AwardYear == expectedStudentAwardYear.Code);
                    var actualApplication = actualFinancialAidApplications.FirstOrDefault(a => a.AwardYear == expectedStudentAwardYear.Code);

                    if (expectedFafsa == null)
                    {
                        Assert.IsFalse(actualApplication.IsFafsaComplete);
                    }
                    else
                    {
                        Assert.IsTrue(actualApplication.IsFafsaComplete);
                    }
                }
            }

            [TestMethod]
            public void ProfileCompleteIfProfileExistsForAwardYearTest()
            {
                foreach (var expectedStudentAwardYear in expectedStudentAwardYears)
                {
                    var expectedProfile = expectedProfileApplications.FirstOrDefault(p => p.AwardYear == expectedStudentAwardYear.Code);
                    var actualApplication = actualFinancialAidApplications.FirstOrDefault(a => a.AwardYear == expectedStudentAwardYear.Code);
                    Assert.IsNotNull(actualApplication);

                    if (expectedProfile == null)
                    {
                        Assert.IsFalse(actualApplication.IsProfileComplete);
                    }
                    else
                    {
                        Assert.IsTrue(actualApplication.IsProfileComplete);
                    }
                }
            }
        }
    }
}
