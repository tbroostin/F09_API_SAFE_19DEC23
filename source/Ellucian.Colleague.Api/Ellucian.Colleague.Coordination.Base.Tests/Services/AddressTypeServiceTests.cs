// Copyright 2015-16 Ellucian Company L.P. and its affiliates.

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
    public class AddressTypeServiceTests
    {
        // The service to be tested
        private AddressTypeService _addressTypesService;

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

        private IEnumerable<Domain.Base.Entities.AddressType2> allAddressTypes;

        private string addressTypeGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

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
            _addressTypesService = new AddressTypeService(_adapterRegistry, _refRepo, _currentUserFactory, _configurationRepository, _roleRepo, _logger);

            allAddressTypes = new TestAddressTypeRepository().Get();
            _refRepoMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>())).ReturnsAsync(allAddressTypes);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _addressTypesService = null;
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
        public async Task GetAddressTypeByGuidAsync_CompareBuildingsAsync()
        {
            Ellucian.Colleague.Domain.Base.Entities.AddressType2 thisAddressType = allAddressTypes.Where(m => m.Guid == addressTypeGuid).FirstOrDefault();
            _refRepoMock.Setup(repo => repo.GetAddressTypes2Async(true)).ReturnsAsync(allAddressTypes.Where(m => m.Guid == addressTypeGuid));
            var addressType = await _addressTypesService.GetAddressTypeByGuidAsync(addressTypeGuid);
            Assert.AreEqual(thisAddressType.Guid, addressType.Id);
            Assert.AreEqual(thisAddressType.Code, addressType.Code);
            Assert.AreEqual(thisAddressType.Description, addressType.Title);
        }

        [TestMethod]
        public async Task GetAddressTypesAsync_CountAddressTypesAsync()
        {
            _refRepoMock.Setup(repo => repo.GetAddressTypes2Async(false)).ReturnsAsync(allAddressTypes);
            var addressTypes = await _addressTypesService.GetAddressTypesAsync();
            Assert.AreEqual(16, addressTypes.Count());
        }

        [TestMethod]
        public async Task GetAddressTypesAsync_CompareAddressTypesAsync_Cache()
        {
            _refRepoMock.Setup(repo => repo.GetAddressTypes2Async(false)).ReturnsAsync(allAddressTypes);
            var addressTypes = await _addressTypesService.GetAddressTypesAsync(It.IsAny<bool>());
            Assert.AreEqual(allAddressTypes.ElementAt(0).Guid, addressTypes.ElementAt(0).Id);
            Assert.AreEqual(allAddressTypes.ElementAt(0).Code, addressTypes.ElementAt(0).Code);
            Assert.AreEqual(allAddressTypes.ElementAt(0).Description, addressTypes.ElementAt(0).Title);
        }

        [TestMethod]
        public async Task GetAddressTypesAsync_CompareAddressTypesAsync_NoCache()
        {
            _refRepoMock.Setup(repo => repo.GetAddressTypes2Async(true)).ReturnsAsync(allAddressTypes);
            var buildings = await _addressTypesService.GetAddressTypesAsync(true);
            Assert.AreEqual(allAddressTypes.ElementAt(0).Guid, buildings.ElementAt(0).Id);
            Assert.AreEqual(allAddressTypes.ElementAt(0).Code, buildings.ElementAt(0).Code);
            Assert.AreEqual(allAddressTypes.ElementAt(0).Description, buildings.ElementAt(0).Title);
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task GetAddressTypeByGuidAsync_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetAddressTypes2Async(false)).ReturnsAsync(allAddressTypes);
            await _addressTypesService.GetAddressTypeByGuidAsync("dhjigodd");
        }

        [ExpectedException(typeof(KeyNotFoundException))]
        [TestMethod]
        public async Task GetAddressTypeByGuid2Async_InvalidAsync()
        {
            _refRepoMock.Setup(repo => repo.GetAddressTypes2Async(true)).ReturnsAsync(allAddressTypes);
            await _addressTypesService.GetAddressTypeByGuidAsync("siuowurhf");
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
