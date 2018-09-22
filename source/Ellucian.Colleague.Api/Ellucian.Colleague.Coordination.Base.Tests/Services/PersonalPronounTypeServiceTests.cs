using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.Base.Services;
using Moq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonalPronounTypeServiceTests
    {
        private PersonalPronounTypeService _personalPronounTypesService;

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
        private IEnumerable<Domain.Base.Entities.PersonalPronounType> allPersonalPronounTypes;

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
            _personalPronounTypesService = new PersonalPronounTypeService(_adapterRegistry, _refRepo, personRepositoryMock.Object, _currentUserFactory, _roleRepo, _logger);

            allPersonalPronounTypes = new List<Domain.Base.Entities.PersonalPronounType>
            {
                new Domain.Base.Entities.PersonalPronounType("1", "HE", "He/Him/His" ),
                new Domain.Base.Entities.PersonalPronounType("2", "SHE", "She/Her/Hers" ),
                new Domain.Base.Entities.PersonalPronounType("3", "ZE", "Ze/Zir/Zirs" ),
            };
            var personalPronounTypeEntityToDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.PersonalPronounType, Dtos.Base.PersonalPronounType>(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.Base.Entities.PersonalPronounType, Dtos.Base.PersonalPronounType>()).Returns(personalPronounTypeEntityToDtoAdapter);

            _refRepoMock.Setup(repo => repo.GetPersonalPronounTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonalPronounTypes);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _personalPronounTypesService = null;
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
        public async Task GetPersonalPronounTypesAsync_CountPersonalPronounTypesAsyns()
        {
            _refRepoMock.Setup(repo => repo.GetPersonalPronounTypesAsync(false)).ReturnsAsync(allPersonalPronounTypes);
            var personalPronounTypes = await _personalPronounTypesService.GetBasePersonalPronounTypesAsync();
            Assert.AreEqual(3, personalPronounTypes.Count());
        }
    }
}
