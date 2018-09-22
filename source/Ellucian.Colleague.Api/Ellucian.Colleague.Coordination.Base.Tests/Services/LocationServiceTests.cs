// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Dtos;
using Ellucian.Data.Colleague;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Base;
using FrequencyType = Ellucian.Colleague.Dtos.FrequencyType;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class LocationTypeServiceTests
    {
        // The service to be tested
        private LocationTypeService _locationTypesService;

        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;
        private Mock<IEventRepository> _eventRepoMock;
        private IEventRepository _eventRepo;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;

        private ICurrentUserFactory _currentUserFactory;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        // Emergency information data for one person for tests
        private const string personId = "S001";

        private IEnumerable<Domain.Base.Entities.LocationTypeItem> allLocationTypes;

        private string locationTypeGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

        [TestInitialize]
        public void Initialize()
        {
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
            _locationTypesService = new LocationTypeService(_adapterRegistry, _refRepo, _currentUserFactory, _roleRepo, _logger);
          
            allLocationTypes = new TestLocationTypeRepository().Get();
            _refRepoMock.Setup(repo => repo.GetLocationTypesAsync(It.IsAny<bool>())).ReturnsAsync(allLocationTypes);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _locationTypesService = null;
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
        public async Task GetLocationTypeByGuidAsync_CompareBuildingsAsync()
        {
            Ellucian.Colleague.Domain.Base.Entities.LocationTypeItem thisLocationType = allLocationTypes.Where(m => m.Guid == locationTypeGuid).FirstOrDefault();
            _refRepoMock.Setup(repo => repo.GetLocationTypesAsync(true)).ReturnsAsync(allLocationTypes.Where(m => m.Guid == locationTypeGuid));
            var locationType = await _locationTypesService.GetLocationTypeByGuidAsync(locationTypeGuid);
            Assert.AreEqual(thisLocationType.Guid, locationType.Id);
            Assert.AreEqual(thisLocationType.Code, locationType.Code);
            Assert.AreEqual(thisLocationType.Description, locationType.Title);
        }

        [TestMethod]
        public async Task GetLocationTypesAsync_CountLocationTypesAsync()
        {
            _refRepoMock.Setup(repo => repo.GetLocationTypesAsync(false)).ReturnsAsync(allLocationTypes);
            var locationTypes = await _locationTypesService.GetLocationTypesAsync();
            Assert.AreEqual(15, locationTypes.Count());
        }

        [TestMethod]
        public async Task GetLocationTypesAsync_CompareLocationTypesAsync_Cache()
        {
            _refRepoMock.Setup(repo => repo.GetLocationTypesAsync(false)).ReturnsAsync(allLocationTypes);
            var locationTypes = await _locationTypesService.GetLocationTypesAsync(It.IsAny<bool>());
            Assert.AreEqual(allLocationTypes.ElementAt(0).Guid, locationTypes.ElementAt(0).Id);
            Assert.AreEqual(allLocationTypes.ElementAt(0).Code, locationTypes.ElementAt(0).Code);
            Assert.AreEqual(allLocationTypes.ElementAt(0).Description, locationTypes.ElementAt(0).Title);
        }

        [TestMethod]
        public async Task GetLocationTypesAsync_CompareLocationTypesAsync_NoCache()
        {
            _refRepoMock.Setup(repo => repo.GetLocationTypesAsync(true)).ReturnsAsync(allLocationTypes);
            var buildings = await _locationTypesService.GetLocationTypesAsync(true);
            Assert.AreEqual(allLocationTypes.ElementAt(0).Guid, buildings.ElementAt(0).Id);
            Assert.AreEqual(allLocationTypes.ElementAt(0).Code, buildings.ElementAt(0).Code);
            Assert.AreEqual(allLocationTypes.ElementAt(0).Description, buildings.ElementAt(0).Title);
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task GetLocationTypeByGuidAsync_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetLocationTypesAsync(false)).ReturnsAsync(allLocationTypes);
            await _locationTypesService.GetLocationTypeByGuidAsync("dhjigodd");
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task GetLocationTypeByGuid2Async_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetLocationTypesAsync(true)).ReturnsAsync(allLocationTypes);
            await _locationTypesService.GetLocationTypeByGuidAsync("siuowurhf");
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
                        Roles = new List<string> {"Student"},
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}
