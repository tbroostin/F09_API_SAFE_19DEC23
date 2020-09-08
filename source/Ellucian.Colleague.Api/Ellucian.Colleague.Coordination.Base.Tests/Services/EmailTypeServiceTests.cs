// Copyright 2015-16 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class EmailTypeServiceTests
    {
        // The service to be tested
        private EmailTypeService _emailTypesService;

        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;
        private Mock<IEventRepository> _eventRepoMock;
        private IEventRepository _eventRepo;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;
        private Mock<IPersonRepository> personRepositoryMock;

        private ICurrentUserFactory _currentUserFactory;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        // Emergency information data for one person for tests
        private const string personId = "S001";

        private IEnumerable<Domain.Base.Entities.EmailType> allEmailTypes;

        private string emailTypeGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

        [TestInitialize]
        public void Initialize()
        {
            personRepositoryMock = new Mock<IPersonRepository>();
            _eventRepoMock = new Mock<IEventRepository>();
            _eventRepo = _eventRepoMock.Object;

            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;

            _refRepoMock = new Mock<IReferenceDataRepository>();
            _refRepo = _refRepoMock.Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;
            _logger = new Mock<ILogger>().Object;

            // Set up current user
            _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();
            _emailTypesService = new EmailTypeService(_adapterRegistry, _refRepo, personRepositoryMock.Object, _currentUserFactory, _roleRepo, _logger, _configurationRepository);

            allEmailTypes = new TestEmailTypeRepository().Get();
            _refRepoMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>())).ReturnsAsync(allEmailTypes);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _emailTypesService = null;
            personRepositoryMock = null;
            _refRepoMock = null;
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
        public async Task GetEmailTypeByGuidAsync_CompareEmailTypesAsync()
        {
            Ellucian.Colleague.Domain.Base.Entities.EmailType thisEmailType = allEmailTypes.Where(m => m.Guid == emailTypeGuid).FirstOrDefault();
            _refRepoMock.Setup(repo => repo.GetEmailTypesAsync(true)).ReturnsAsync(allEmailTypes.Where(m => m.Guid == emailTypeGuid));
            var emailType = await _emailTypesService.GetEmailTypeByGuidAsync(emailTypeGuid);
            Assert.AreEqual(thisEmailType.Guid, emailType.Id);
            Assert.AreEqual(thisEmailType.Code, emailType.Code);
            Assert.AreEqual(thisEmailType.Description, emailType.Title);
        }

        [TestMethod]
        public async Task GetEmailTypesAsync_CountEmailTypesAsync()
        {
            _refRepoMock.Setup(repo => repo.GetEmailTypesAsync(false)).ReturnsAsync(allEmailTypes);
            var emailTypes = await _emailTypesService.GetEmailTypesAsync();
            Assert.AreEqual(14, emailTypes.Count());
        }

        [TestMethod]
        public async Task GetEmailTypesAsync_CompareEmailTypesAsync_Cache()
        {
            _refRepoMock.Setup(repo => repo.GetEmailTypesAsync(false)).ReturnsAsync(allEmailTypes);
            var emailTypes = await _emailTypesService.GetEmailTypesAsync(It.IsAny<bool>());
            Assert.AreEqual(allEmailTypes.ElementAt(0).Guid, emailTypes.ElementAt(0).Id);
            Assert.AreEqual(allEmailTypes.ElementAt(0).Code, emailTypes.ElementAt(0).Code);
            Assert.AreEqual(allEmailTypes.ElementAt(0).Description, emailTypes.ElementAt(0).Title);
        }

        [TestMethod]
        public async Task GetEmailTypesAsync_CompareEmailTypesAsync_NoCache()
        {
            _refRepoMock.Setup(repo => repo.GetEmailTypesAsync(true)).ReturnsAsync(allEmailTypes);
            var emailTypes = await _emailTypesService.GetEmailTypesAsync(true);
            Assert.AreEqual(allEmailTypes.ElementAt(0).Guid, emailTypes.ElementAt(0).Id);
            Assert.AreEqual(allEmailTypes.ElementAt(0).Code, emailTypes.ElementAt(0).Code);
            Assert.AreEqual(allEmailTypes.ElementAt(0).Description, emailTypes.ElementAt(0).Title);
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task GetEmailTypeByGuidAsync_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetEmailTypesAsync(false)).ReturnsAsync(allEmailTypes);
            await _emailTypesService.GetEmailTypeByGuidAsync("dhjigodd");
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task GetEmailTypeByGuid2Async_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetEmailTypesAsync(true)).ReturnsAsync(allEmailTypes);
            await _emailTypesService.GetEmailTypeByGuidAsync("siuowurhf");
        }

        // Fake an ICurrentUserFactory
        public class Person001UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims
                    {
                        // Only the PersonId is part of the test, whether it matches the ID of the person whose 
                        // emergency information is requested. The remaining fields are arbitrary.
                        ControlId = "123",
                        Name = "Fred",
                        PersonId = personId,
                        /* From the test data of the test class */
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string> { "Student" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}
