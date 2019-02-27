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
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonalPronounTypeServiceTests
    {
        private PersonalPronounTypeService _personalPronounTypesService;
        private const string personalPronounsGuid = "1";
        private const string personalPronounsCode = "HE";
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
        private IEnumerable<Domain.Base.Entities.PersonalPronounType> allPersonalPronounTypes;
        private Mock<IConfigurationRepository> _configurationRepoMock;

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

            _configurationRepoMock = new Mock<IConfigurationRepository>();

            // Set up current user
            _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();
            _personalPronounTypesService = new PersonalPronounTypeService(_adapterRegistry, _refRepo, _currentUserFactory, _roleRepo, _configurationRepository, _logger);

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

        [TestMethod]
        public async Task PersonalPronounsService_GetPersonalPronounsAsync()
        {
            var results = await _personalPronounTypesService.GetPersonalPronounsAsync(true);
            Assert.IsTrue(results is IEnumerable<PersonalPronouns>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task PersonalPronounsService_GetPersonalPronounsAsync_Count()
        {
            var results = await _personalPronounTypesService.GetPersonalPronounsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task PersonalPronounsService_GetPersonalPronounsAsync_Properties()
        {
            var result =
                (await _personalPronounTypesService.GetPersonalPronounsAsync(true)).FirstOrDefault(x => x.Code == personalPronounsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task PersonalPronounsService_GetPersonalPronounsAsync_Expected()
        {
            var expectedResults = allPersonalPronounTypes.FirstOrDefault(c => c.Guid == personalPronounsGuid);
            var actualResult =
                (await _personalPronounTypesService.GetPersonalPronounsAsync(true)).FirstOrDefault(x => x.Id == personalPronounsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonalPronounsService_GetPersonalPronounsByGuidAsync_Empty()
        {
            await _personalPronounTypesService.GetPersonalPronounsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonalPronounsService_GetPersonalPronounsByGuidAsync_Null()
        {
            await _personalPronounTypesService.GetPersonalPronounsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonalPronounsService_GetPersonalPronounsByGuidAsync_InvalidId()
        {
            _refRepoMock.Setup(repo => repo.GetPersonalPronounTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _personalPronounTypesService.GetPersonalPronounsByGuidAsync("99");
        }

        [TestMethod]
        public async Task PersonalPronounsService_GetPersonalPronounsByGuidAsync_Expected()
        {
            var expectedResults =
                allPersonalPronounTypes.First(c => c.Guid == personalPronounsGuid);
            var actualResult =
                await _personalPronounTypesService.GetPersonalPronounsByGuidAsync(personalPronounsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task PersonalPronounsService_GetPersonalPronounsByGuidAsync_Properties()
        {
            var result =
                await _personalPronounTypesService.GetPersonalPronounsByGuidAsync(personalPronounsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}