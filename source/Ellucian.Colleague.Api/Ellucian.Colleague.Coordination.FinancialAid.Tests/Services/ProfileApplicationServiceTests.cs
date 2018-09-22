/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
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
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class ProfileApplicationServiceTests : FinancialAidServiceTestsSetup
    {
        public Mock<IProfileApplicationRepository> profileApplicationRepositoryMock;
        public Mock<IFinancialAidOfficeRepository> financialAidOfficeRepositoryMock;
        public Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;

        public TestFinancialAidOfficeRepository expectedFinancialAidOfficeRepository;
        public TestStudentAwardYearRepository expectedStudentAwardYearRepository;
        public TestProfileApplicationRepository expectedProfileApplicationRepository;

        public ITypeAdapter<Colleague.Domain.FinancialAid.Entities.ProfileApplication, ProfileApplication> profileApplicationDtoAdapter;

        public FunctionEqualityComparer<ProfileApplication> profileApplicationDtoComparer;

        public void ProfileApplicationServiceTestsInitialize()
        {
            BaseInitialize();

            profileApplicationRepositoryMock = new Mock<IProfileApplicationRepository>();
            financialAidOfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
            studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();

            expectedFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            expectedStudentAwardYearRepository = new TestStudentAwardYearRepository();
            expectedProfileApplicationRepository = new TestProfileApplicationRepository();

            profileApplicationDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.ProfileApplication, ProfileApplication>(adapterRegistryMock.Object, loggerMock.Object);

            profileApplicationDtoComparer = new FunctionEqualityComparer<ProfileApplication>(
                (p1, p2) => p1.Id == p2.Id,
                (p) => p.Id.GetHashCode());
        }

        [TestClass]
        public class GetProfileApplicationsTests : ProfileApplicationServiceTests
        {
            public string studentId;

            //Domain entities can be modified for tests by changing the record representations in the test repositories
            public IEnumerable<Domain.FinancialAid.Entities.FinancialAidOffice> financialAidOfficeEntities
            {
                get { return expectedFinancialAidOfficeRepository.GetFinancialAidOffices(); }
            }
            public IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYearEntities
            {
                get { return expectedStudentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeEntities)); }
            }
            public IEnumerable<Domain.FinancialAid.Entities.ProfileApplication> profileApplicationEntities
            {
                get { return expectedProfileApplicationRepository.GetProfileApplicationsAsync(studentId, studentAwardYearEntities).Result; }
            }

            //Dtos
            public List<ProfileApplication> expectedProfileApplications
            {
                get { return profileApplicationEntities.Select(profile => profileApplicationDtoAdapter.MapToType(profile)).ToList(); }
            }
            public IEnumerable<ProfileApplication> actualProfileApplications;
           
            public ProfileApplicationService profileApplicationService;

            [TestInitialize]
            public void Initialize()
            {
                ProfileApplicationServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
                    .Returns(() => expectedFinancialAidOfficeRepository.GetFinancialAidOfficesAsync());

                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, currentOfficeService, b) => Task.FromResult(expectedStudentAwardYearRepository.GetStudentAwardYears(id, currentOfficeService)));

                profileApplicationRepositoryMock.Setup(r => r.GetProfileApplicationsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>(
                        (id, studentAwardYears) => expectedProfileApplicationRepository.GetProfileApplicationsAsync(id, studentAwardYears));

                adapterRegistryMock.Setup(r => r.GetAdapter<Domain.FinancialAid.Entities.ProfileApplication, ProfileApplication>())
                    .Returns(profileApplicationDtoAdapter);

                BuildService();
            }

            private void BuildService()
            {
                profileApplicationService = new ProfileApplicationService(adapterRegistryMock.Object, profileApplicationRepositoryMock.Object, financialAidOfficeRepositoryMock.Object, studentAwardYearRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualProfileApplications = await profileApplicationService.GetProfileApplicationsAsync(studentId);
                CollectionAssert.AreEqual(expectedProfileApplications, actualProfileApplications.ToList(), profileApplicationDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await profileApplicationService.GetProfileApplicationsAsync(null);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CounselorCanAccessStudentDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildService();

                actualProfileApplications = await profileApplicationService.GetProfileApplicationsAsync(studentId);

                CollectionAssert.AreEqual(expectedProfileApplications, actualProfileApplications.ToList(), profileApplicationDtoComparer);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CounselorNoPermissions_CannotAccessStudentDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                
                BuildService();

                await profileApplicationService.GetProfileApplicationsAsync(studentId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ProxyrCanAccessStudentDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildService();

                actualProfileApplications = await profileApplicationService.GetProfileApplicationsAsync(studentId);

                CollectionAssert.AreEqual(expectedProfileApplications, actualProfileApplications.ToList(), profileApplicationDtoComparer);
            }

            /// <summary>
            /// User is proxy for different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyWithDifferentProxySubjectId_CannotAccessStudentDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                BuildService();

                await profileApplicationService.GetProfileApplicationsAsync(studentId);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserNotRequestingSelfAndUserNotCounselorNorProxyTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));

                try
                {
                    await profileApplicationService.GetProfileApplicationsAsync("foobar");
                }
                catch (Exception e)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to access profile applications for {1}", currentUserFactory.CurrentUser.PersonId, "foobar")));
                    throw;
                }
            }

            [TestMethod]
            public async Task StudentAwardYearsIsNull_ReturnEmptyListTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, currentOfficeService, b) => Task.FromResult((new List<Domain.FinancialAid.Entities.StudentAwardYear>()).AsEnumerable()));
                actualProfileApplications = await profileApplicationService.GetProfileApplicationsAsync(studentId);

                Assert.AreEqual(0, actualProfileApplications.Count());
                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no award years", studentId)));
            }

            [TestMethod]
            public async Task StudentAwardYearsIsEmpty_ReturnEmptyListTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, currentOfficeService, b) => Task.FromResult((new List<Domain.FinancialAid.Entities.StudentAwardYear>()).AsEnumerable()));
                actualProfileApplications = await profileApplicationService.GetProfileApplicationsAsync(studentId);

                Assert.AreEqual(0, actualProfileApplications.Count());
                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no award years", studentId)));
            }

            [TestMethod]
            public async Task ProfileApplicationEntitiesIsNull_ReturnEmptyListTest()
            {
                profileApplicationRepositoryMock.Setup(r => r.GetProfileApplicationsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>(
                        (id, studentAwardYears) => Task.FromResult((IEnumerable<Domain.FinancialAid.Entities.ProfileApplication>)null));
                actualProfileApplications = await profileApplicationService.GetProfileApplicationsAsync(studentId);

                Assert.AreEqual(0, actualProfileApplications.Count());
                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no profile applications", studentId)));
            }

            [TestMethod]
            public async Task ProfileApplicationEntitiesIsEmpty_ReturnEmptyListTest()
            {
                profileApplicationRepositoryMock.Setup(r => r.GetProfileApplicationsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>(
                        (id, studentAwardYears) => Task.FromResult((new List<Domain.FinancialAid.Entities.ProfileApplication>()).AsEnumerable()));
                actualProfileApplications = await profileApplicationService.GetProfileApplicationsAsync(studentId);

                Assert.AreEqual(0, actualProfileApplications.Count());
                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no profile applications", studentId)));
            }
        }
    }
}
