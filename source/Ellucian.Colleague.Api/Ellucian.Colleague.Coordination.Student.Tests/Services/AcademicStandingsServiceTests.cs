// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Coordination.Base.Tests.Services;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AcademicStandingsServiceTests
    {
        [TestClass]
        public class GET
        {
            private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
            private IStudentReferenceDataRepository _refRepo;
            private Mock<IEventRepository> _eventRepoMock;
            private IEventRepository _eventRepo;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IRoleRepository> _roleRepoMock;
            private IRoleRepository _roleRepo;
            private Mock<ILogger> loggerMock;
            private ILogger _logger;
            private ICurrentUserFactory _currentUserFactory;
            private IConfigurationRepository _configurationRepository;
            private Mock<IConfigurationRepository> _configurationRepositoryMock;


            AcademicStandingsService academicStandingsService;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicStanding2> academicStandings;

            [TestInitialize]
            public void Initialize()
            {
                _eventRepoMock = new Mock<IEventRepository>();
                _eventRepo = _eventRepoMock.Object;

                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                _configurationRepository = _configurationRepositoryMock.Object;

                _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _refRepo = _studentReferenceDataRepositoryMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepoMock = new Mock<IRoleRepository>();
                _roleRepo = _roleRepoMock.Object;
                _logger = new Mock<ILogger>().Object;

                // Set up current user
                _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();
                academicStandingsService = new AcademicStandingsService(_adapterRegistry, _refRepo, _currentUserFactory, _configurationRepository, _roleRepo, _logger);

                academicStandings = new TestStudentReferenceDataRepository().GetAcademicStandings2Async(false).Result;
            }

            [TestCleanup]
            public void Cleanup()
            {
                academicStandingsService = null;
                academicStandings = null;
                _studentReferenceDataRepositoryMock = null;
                _refRepo = null;
                _configurationRepository = null;
                _configurationRepositoryMock = null;
                _eventRepo = null;
                _eventRepoMock = null;
                _adapterRegistry = null;
                _adapterRegistryMock = null;
                _logger = null;
                _roleRepoMock = null;
                _roleRepo = null;
                _currentUserFactory = null;
            }

            [TestMethod]
            public async Task AcademicStandingsService__GetAllAsync()
            {
                _studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicStandings2Async(It.IsAny<bool>())).ReturnsAsync(academicStandings);

                var results = await academicStandingsService.GetAcademicStandingsAsync(It.IsAny<bool>());
                Assert.AreEqual(academicStandings.ToList().Count, (results.Count()));

                foreach (var academicStanding in academicStandings)
                {
                    var result = results.FirstOrDefault(i => i.Id == academicStanding.Guid);

                    Assert.AreEqual(academicStanding.Code, result.Code);
                    Assert.AreEqual(academicStanding.Description, result.Title);
                    Assert.AreEqual(academicStanding.Guid, result.Id);
                }
            }

            [TestMethod]
            public async Task AcademicStandingsService__GetByIdAsync()
            {
                _studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicStandings2Async(It.IsAny<bool>())).ReturnsAsync(academicStandings);

                string id = "9C3B805D-CFE6-483B-86C3-4C20562F8C15".ToLower();
                var academicStanding = academicStandings.FirstOrDefault(i => i.Guid == id);

                var result = await academicStandingsService.GetAcademicStandingByIdAsync(id);

                Assert.AreEqual(academicStanding.Code, result.Code);
                Assert.AreEqual(academicStanding.Description, result.Title);
                Assert.AreEqual(academicStanding.Guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicStandingsService__GetByIdAsync_KeyNotFoundException()
            {
                _studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicStandings2Async(true)).ReturnsAsync(academicStandings);
                var result = await academicStandingsService.GetAcademicStandingByIdAsync("123");
            }
        }
    }
}
