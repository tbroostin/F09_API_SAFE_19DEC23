/*Copyright 2016-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Moq;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class AwardYearCoordinationServiceTests : FinancialAidServiceTestsSetup
    {
        public class AwardYearCoordinationServiceUnderTest : AwardYearCoordinationService
        {
            public AwardYearCoordinationServiceUnderTest(IAdapterRegistry adapterRegistry,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, financialAidOfficeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
            { }

            public async Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>> getActiveStudentAwardYearEntitiesAsync(string studentId) { return await base.GetActiveStudentAwardYearEntitiesAsync(studentId); }

            public async Task<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>> getStudentAwardYearEntitiesAsync(string studentId) { return await base.GetStudentAwardYearEntitiesAsync(studentId); }

            public async Task<Domain.FinancialAid.Entities.StudentAwardYear> getStudentAwardYearEntityAsync(string studentId, string awardYearCode) { return await base.GetStudentAwardYearEntityAsync(studentId, awardYearCode);}
        }

        /// <summary>
        /// Mini test role repository
        /// </summary>
        public class TestRoleRepository : IRoleRepository
        {
            public IEnumerable<Ellucian.Colleague.Domain.Entities.Role> roles = new List<Ellucian.Colleague.Domain.Entities.Role>()
            {
                new Ellucian.Colleague.Domain.Entities.Role(1, "FINANCIAL AID COUNSELOR"),                
                new Ellucian.Colleague.Domain.Entities.Role(2, "STUDENT")
            };

            public IEnumerable<Ellucian.Colleague.Domain.Entities.Role> Roles { get { return roles; } }
            public async Task<IEnumerable<Ellucian.Colleague.Domain.Entities.Role>> GetRolesAsync() { return (await Task.FromResult(new List<Ellucian.Colleague.Domain.Entities.Role>())); }
        }

        private AwardYearCoordinationServiceUnderTest awardYearCoordinationService;
        private TestRoleRepository testRoleRepository;
        private TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;
        private TestStudentAwardYearRepository testStudentAwardYearRepository;

        private Mock<IFinancialAidOfficeRepository> officeRepositoryMock;
        private Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        private CurrentOfficeService currentOfficeService;

        private string studentId, awardYearCode;

        [TestInitialize]
        public void Initialize()
        {
            BaseInitialize();
            studentId = "0004791";

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
            
            testRoleRepository = new TestRoleRepository();
            roleRepositoryMock.Setup(r => r.Roles).Returns(testRoleRepository.roles);

            testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
            var faOffices = testFinancialAidOfficeRepository.GetFinancialAidOfficesAsync();
            officeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync()).Returns(faOffices);
            
            currentOfficeService = new CurrentOfficeService(faOffices.Result);
            testStudentAwardYearRepository = new TestStudentAwardYearRepository();

            awardYearCode = testStudentAwardYearRepository.CsStudentData.First().AwardYear;
            studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
            studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                .Returns(testStudentAwardYearRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService));
            studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                .ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));
            studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearAsync(studentId, awardYearCode, It.IsAny<CurrentOfficeService>()))
                .Returns(testStudentAwardYearRepository.GetStudentAwardYearAsync(studentId, awardYearCode, currentOfficeService));

            awardYearCoordinationService = new AwardYearCoordinationServiceUnderTest(adapterRegistryMock.Object, 
                officeRepositoryMock.Object,
                studentAwardYearRepositoryMock.Object,
                baseConfigurationRepository,
                currentUserFactory,
                roleRepositoryMock.Object,
                loggerMock.Object);

        }

        #region Async method tests
        [TestMethod]
        public async Task GetActiveStudentAwardEntitiesAsync_ReturnsExpectedNumberOfYearsTest()
        {
            var expectedCount = testStudentAwardYearRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService).Result.Count();
            Assert.AreEqual(expectedCount, (await awardYearCoordinationService.getActiveStudentAwardYearEntitiesAsync(studentId)).Count());
        }

        [TestMethod]
        public async Task GetActiveStudentAwardEntitiesAsync_ReturnsNoAwardYearsTest()
        {
            var faOffices = testFinancialAidOfficeRepository.GetFinancialAidOfficesAsync().Result.ToList();
            faOffices.ForEach(o => o.Configurations.ToList().ForEach(c => c.IsSelfServiceActive = false));

            studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                .Returns(testStudentAwardYearRepository.GetStudentAwardYearsAsync(studentId, new CurrentOfficeService(faOffices)));

            Assert.AreEqual(0, (await awardYearCoordinationService.getActiveStudentAwardYearEntitiesAsync(studentId)).Count());
        }

        [TestMethod]
        public async Task GetActiveStudentAwardEntitiesAsync_ReturnsNullTest()
        {
            studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), true))
                .ReturnsAsync(() => null);
            Assert.IsNull(await awardYearCoordinationService.getActiveStudentAwardYearEntitiesAsync(studentId));
        }

        [TestMethod]
        public async Task GetStudentAwardYearEntitiesAsync_ReturnsExpectedNumberOfEntitiesTest()
        {
            var expectedCount = testStudentAwardYearRepository.GetStudentAwardYearsAsync(studentId, currentOfficeService).Result.Count();
            Assert.AreEqual(expectedCount, (await awardYearCoordinationService.getStudentAwardYearEntitiesAsync(studentId)).Count());
        }

        [TestMethod]
        public async Task GetStudentAwardYearEntitiesAsync_ReturnsNullTest()
        {
            studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                .ReturnsAsync(() => null);
            Assert.IsNull(await awardYearCoordinationService.getStudentAwardYearEntitiesAsync(studentId));
        }

        [TestMethod]
        public async Task GetStudentAwardYearEntitiesAsync_ReturnsNoRecordsTest()
        {
            studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<StudentAwardYear>());
            Assert.IsFalse((await awardYearCoordinationService.getStudentAwardYearEntitiesAsync(studentId)).Any());
        }

        [TestMethod]
        public async Task GetStudentAwardYearEntityAsync_ReturnsAwardYearEntityTest()
        {
            Assert.IsNotNull(await awardYearCoordinationService.getStudentAwardYearEntityAsync(studentId, awardYearCode));
        }

        [TestMethod]
        public async Task GetStudentAwardYearEntityAsync_ReturnsNullTest()
        {
            studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearAsync(studentId, awardYearCode, It.IsAny<CurrentOfficeService>()))
                .ReturnsAsync(() => null);
            Assert.IsNull(await awardYearCoordinationService.getStudentAwardYearEntityAsync(studentId, awardYearCode));
        }
        
        #endregion
    }
}
