using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class GenderIdentityTypeServiceTests
    {
        // The service to be tested
        private GenderIdentityTypeService _genderIdentityTypeService;

        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ILogger _logger;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;
        private Mock<IPersonRepository> personRepositoryMock;

        private ICurrentUserFactory _currentUserFactory;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        private const string personId = "S001";

        private string genderIdentityTypeGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

        private IEnumerable<Domain.Base.Entities.GenderIdentityType> allGenderIdentityTypes;

        [TestInitialize]
        public void Initialize()
        {
            personRepositoryMock = new Mock<IPersonRepository>();

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
            _genderIdentityTypeService = new GenderIdentityTypeService(_adapterRegistry, _refRepo, _currentUserFactory, _roleRepo, _logger);

            var genderIdentityDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.GenderIdentityType, Dtos.Base.GenderIdentityType> (_adapterRegistryMock.Object, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.GenderIdentityType, Dtos.Base.GenderIdentityType>()).Returns(genderIdentityDtoAdapter);

            allGenderIdentityTypes = new TestGenderIdentityTypeRepository().Get();
            _refRepoMock.Setup(repo => repo.GetGenderIdentityTypesAsync(It.IsAny<bool>())).ReturnsAsync(allGenderIdentityTypes);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _genderIdentityTypeService = null;
            personRepositoryMock = null;
            _refRepoMock = null;
            _refRepo = null;
            _configurationRepository = null;
            _configurationRepositoryMock = null;
            _adapterRegistry = null;
            _adapterRegistryMock = null;
            _logger = null;
            _roleRepoMock = null;
            _roleRepo = null;
            _currentUserFactory = null;
        }

        [TestMethod]
        public async Task GetGenderIdentityTypesAsync_CountGenderIdentityTypesAsync()
        {
            _refRepoMock.Setup(repo => repo.GetGenderIdentityTypesAsync(false)).ReturnsAsync(allGenderIdentityTypes);
            var genderIdentityTypes = await _genderIdentityTypeService.GetBaseGenderIdentityTypesAsync();
            Assert.AreEqual(6, genderIdentityTypes.Count());
        }

        [TestMethod]
        public async Task GetGenderIdentityTypesAsync_CompareGenderIdentityTypesAsync_Cache()
        {
            _refRepoMock.Setup(repo => repo.GetGenderIdentityTypesAsync(false)).ReturnsAsync(allGenderIdentityTypes);
            var emailTypes = await _genderIdentityTypeService.GetBaseGenderIdentityTypesAsync(It.IsAny<bool>());
            Assert.AreEqual(allGenderIdentityTypes.ElementAt(0).Guid, allGenderIdentityTypes.ElementAt(0).Guid);
            Assert.AreEqual(allGenderIdentityTypes.ElementAt(0).Code, allGenderIdentityTypes.ElementAt(0).Code);
            Assert.AreEqual(allGenderIdentityTypes.ElementAt(0).Description, allGenderIdentityTypes.ElementAt(0).Description);
        }

        [TestMethod]
        public async Task GetGenderIdentityTypesAsync_CompareGenderIdentityTypesAsync_NoCache()
        {
            _refRepoMock.Setup(repo => repo.GetGenderIdentityTypesAsync(true)).ReturnsAsync(allGenderIdentityTypes);
            var emailTypes = await _genderIdentityTypeService.GetBaseGenderIdentityTypesAsync(true);
            Assert.AreEqual(allGenderIdentityTypes.ElementAt(0).Guid, allGenderIdentityTypes.ElementAt(0).Guid);
            Assert.AreEqual(allGenderIdentityTypes.ElementAt(0).Code, allGenderIdentityTypes.ElementAt(0).Code);
            Assert.AreEqual(allGenderIdentityTypes.ElementAt(0).Description, allGenderIdentityTypes.ElementAt(0).Description);
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
                        // Only the PersonId is part of the test
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
