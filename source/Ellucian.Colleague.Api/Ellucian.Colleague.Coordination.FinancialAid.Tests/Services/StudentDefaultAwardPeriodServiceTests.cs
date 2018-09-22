/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Moq;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class StudentDefaultAwardPeriodServiceTests : FinancialAidServiceTestsSetup
    {
        private TestStudentDefaultAwardPeriodRepository testStudentDefaultAwardPeriodRepository;
        private TestStudentAwardYearRepository testStudentAwardYearRepository;
        private TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;

        private Mock<IStudentDefaultAwardPeriodRepository> studentDefaultAwardPeriodRepositoryMock;
        private Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
        private Mock<IFinancialAidOfficeRepository> financialAidOfficeRepositoryMock;

        private AutoMapperAdapter<Ellucian.Colleague.Domain.FinancialAid.Entities.StudentDefaultAwardPeriod, StudentDefaultAwardPeriod> defaultAwardPeriodEntityToDtoAdapter;

        private StudentDefaultAwardPeriodService studentDefaultAwardPeriodService;

        private string studentId;
        private IEnumerable<Colleague.Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears;
        private IEnumerable<StudentDefaultAwardPeriod> expectedPeriods;
        private IEnumerable<StudentDefaultAwardPeriod> actualPeriods;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


        [TestInitialize]
        public async void Initialize()
        {
            BaseInitialize();
            studentId = currentUserFactory.CurrentUser.PersonId;

            testStudentDefaultAwardPeriodRepository = new TestStudentDefaultAwardPeriodRepository();
            testStudentAwardYearRepository = new TestStudentAwardYearRepository();
            testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();

            var currentOfficeService = new CurrentOfficeService(testFinancialAidOfficeRepository.GetFinancialAidOffices());
            studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

            studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
            financialAidOfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
            studentDefaultAwardPeriodRepositoryMock = new Mock<IStudentDefaultAwardPeriodRepository>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            studentAwardYearRepositoryMock.Setup(m => m.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                .ReturnsAsync(studentAwardYears);

            financialAidOfficeRepositoryMock.Setup(m => m.GetFinancialAidOfficesAsync()).ReturnsAsync(testFinancialAidOfficeRepository.GetFinancialAidOffices());

            studentDefaultAwardPeriodRepositoryMock.Setup(m => m.GetStudentDefaultAwardPeriodsAsync(studentId, It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                .Returns(testStudentDefaultAwardPeriodRepository.GetStudentDefaultAwardPeriodsAsync(studentId, studentAwardYears));

            defaultAwardPeriodEntityToDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.StudentDefaultAwardPeriod, StudentDefaultAwardPeriod>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(m => m.GetAdapter<Domain.FinancialAid.Entities.StudentDefaultAwardPeriod, StudentDefaultAwardPeriod>()).Returns(defaultAwardPeriodEntityToDtoAdapter);

            expectedPeriods = await GetDefaultAwardPeriodDtos();

            BuildService();
        }

        private void BuildService()
        {
            studentDefaultAwardPeriodService = new StudentDefaultAwardPeriodService(adapterRegistryMock.Object,
                            studentAwardYearRepositoryMock.Object, financialAidOfficeRepositoryMock.Object, studentDefaultAwardPeriodRepositoryMock.Object, baseConfigurationRepository,
                            currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            BaseCleanup();
            testStudentDefaultAwardPeriodRepository = null;
            testStudentAwardYearRepository = null;
            testFinancialAidOfficeRepository = null;
            studentAwardYearRepositoryMock = null;
            studentDefaultAwardPeriodRepositoryMock = null;
            financialAidOfficeRepositoryMock = null;
            studentId = null;
            studentAwardYears = null;
            expectedPeriods = null;
            actualPeriods = null;
            studentDefaultAwardPeriodService = null;
            defaultAwardPeriodEntityToDtoAdapter = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentIdIsNull_ArgumentNullExceptionThrownTest()
        {
            await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task StudentIdIsEmpty_ArgumentNullExceptionThrownTest()
        {
            await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(string.Empty);
        }

        /// <summary>
        /// User is counselor
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task UserIsCounselor_HasDataAccessTest()
        {
            currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
            counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
            roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
            BuildService();
            var actualAwardPeriods = (await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId)).ToList();
            var expectedAwardPeriods = expectedPeriods.ToList();
            for (var i = 0; i < expectedPeriods.Count(); i++)
            {
                Assert.AreEqual(expectedAwardPeriods[i].AwardYear, actualAwardPeriods[i].AwardYear);
                Assert.AreEqual(expectedAwardPeriods[i].StudentId, actualAwardPeriods[i].StudentId);
                Assert.AreEqual(expectedAwardPeriods[i].DefaultAwardPeriods.Count, actualAwardPeriods[i].DefaultAwardPeriods.Count);
            }
        }

        /// <summary>
        /// User is counselor with no permissions
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task UserIsCounselorNoPermissions_NoDataAccessTest()
        {
            currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
            
            BuildService();
            await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId);
            
        }

        /// <summary>
        /// User is proxy
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task UserIsProxy_HasDataAccessTest()
        {
            currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
            BuildService();
            var actualAwardPeriods = (await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId)).ToList();
            var expectedAwardPeriods = expectedPeriods.ToList();
            for (var i = 0; i < expectedPeriods.Count(); i++)
            {
                Assert.AreEqual(expectedAwardPeriods[i].AwardYear, actualAwardPeriods[i].AwardYear);
                Assert.AreEqual(expectedAwardPeriods[i].StudentId, actualAwardPeriods[i].StudentId);
                Assert.AreEqual(expectedAwardPeriods[i].DefaultAwardPeriods.Count, actualAwardPeriods[i].DefaultAwardPeriods.Count);
            }
        }

        /// <summary>
        /// User is proxy for a different person
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task UserIsProxyForDifferentPerson_NoDataAccessTest()
        {
            currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();
            BuildService();
            await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId);

        }

        /// <summary>
        /// User is neither self, nor proxy, nor admin
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task UserDoesNotHavePermission_PermissionsExceptionThrownTest()
        {
            await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync("foo");
        }

        [TestMethod]        
        public async Task UserDoesNotHavePermission_AppropriateMessageLoggedTest()
        {
            try
            {
                await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync("foo");
            }
            catch(PermissionsException){
                loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to access award period information for foo", currentUserFactory.CurrentUser.PersonId)));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task NoStudentAwardYears_InvalidOperationExceptionThrownTest()
        {
            studentAwardYearRepositoryMock.Setup(m => m.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<Domain.FinancialAid.Entities.StudentAwardYear>());
            await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task NullStudentAwardYears_InvalidOperationExceptionThrownTest()
        {
            studentAwardYearRepositoryMock.Setup(m => m.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                .ReturnsAsync((IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>)null);
            await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId);
        }

        [TestMethod]
        public async Task NoStudentAwardYears_AppropriateMessageLoggedTest()
        {
            studentAwardYearRepositoryMock.Setup(m => m.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<Domain.FinancialAid.Entities.StudentAwardYear>());
            try
            {
                await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId);
            }
            catch (InvalidOperationException)
            {
                loggerMock.Verify(l => l.Error(string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId)));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task NoDefaultStudentAwardPeriods_InvalidOperationExceptionThrownTest()
        {
            studentDefaultAwardPeriodRepositoryMock.Setup(m => m.GetStudentDefaultAwardPeriodsAsync(studentId, It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                .ReturnsAsync(new List<Domain.FinancialAid.Entities.StudentDefaultAwardPeriod>());
            await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task NullDefaultStudentAwardPeriods_InvalidOperationExceptionThrownTest()
        {
            studentDefaultAwardPeriodRepositoryMock.Setup(m => m.GetStudentDefaultAwardPeriodsAsync(studentId, It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                .ReturnsAsync((IEnumerable<Domain.FinancialAid.Entities.StudentDefaultAwardPeriod>)null);
            await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId);
        }

        [TestMethod]
        public async Task NoDefaultStudentAwardPeriods_AppropriateMessageLoggedTest()
        {
            studentDefaultAwardPeriodRepositoryMock.Setup(m => m.GetStudentDefaultAwardPeriodsAsync(studentId, It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                .ReturnsAsync(new List<Domain.FinancialAid.Entities.StudentDefaultAwardPeriod>());
            try
            {
                await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId);
            }
            catch (InvalidOperationException)
            {
                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no default award periods", studentId)));
            }
        }

        [TestMethod]
        public async Task ActualDefaultStudentAwardPeriodsCount_MatchesExpectedCountTest()
        {
            var expectedCount = expectedPeriods.Count();
            Assert.AreEqual(expectedCount, (await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId)).Count());
        }

        [TestMethod]
        public async Task ActualDefaultStudentAwardPeriods_EqualExpectedPeriodsTest()
        {
            var actualAwardPeriods = (await studentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId)).ToList();
            var expectedAwardPeriods = expectedPeriods.ToList();
            for (var i = 0; i < expectedPeriods.Count(); i++)
            {
                Assert.AreEqual(expectedAwardPeriods[i].AwardYear, actualAwardPeriods[i].AwardYear);
                Assert.AreEqual(expectedAwardPeriods[i].StudentId, actualAwardPeriods[i].StudentId);
                Assert.AreEqual(expectedAwardPeriods[i].DefaultAwardPeriods.Count, actualAwardPeriods[i].DefaultAwardPeriods.Count);
            }
        }

        /// <summary>
        /// Helper to build expected default period dtos
        /// </summary>
        /// <returns>list of default period dtos</returns>
        private async Task<IEnumerable<StudentDefaultAwardPeriod>> GetDefaultAwardPeriodDtos()
        {
            var defaultPeriods = new List<StudentDefaultAwardPeriod>();
            var periodEntities = await testStudentDefaultAwardPeriodRepository.GetStudentDefaultAwardPeriodsAsync(studentId, studentAwardYears);
            foreach (var entity in periodEntities)
            {
                defaultPeriods.Add(defaultAwardPeriodEntityToDtoAdapter.MapToType(entity));
            }
            return defaultPeriods;
        }

    }
}
