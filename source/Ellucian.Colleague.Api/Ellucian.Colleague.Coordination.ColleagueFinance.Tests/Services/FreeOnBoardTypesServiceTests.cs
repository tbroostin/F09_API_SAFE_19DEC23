//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.


using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class FreeOnBoardTypesServiceTests : CurrentUserSetup
    {
        private const string freeOnBoardTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string freeOnBoardTypesCode = "AT";
        private ICollection<FreeOnBoardType> _freeOnBoardTypesCollection;
        private FreeOnBoardTypesService _freeOnBoardTypesService;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ICurrentUserFactory currentUserFactory;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ILogger> _loggerMock;
        private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;
        private Domain.Entities.Permission permissionViewAnyPerson;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();
            // Mock permissions
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            _freeOnBoardTypesCollection = new List<FreeOnBoardType>()
                {
                    new FreeOnBoardType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new FreeOnBoardType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new FreeOnBoardType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetFreeOnBoardTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_freeOnBoardTypesCollection);

            _freeOnBoardTypesService = new FreeOnBoardTypesService(_referenceRepositoryMock.Object, _configurationRepositoryMock.Object, adapterRegistry, currentUserFactory, roleRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _freeOnBoardTypesService = null;
            _freeOnBoardTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task FreeOnBoardTypesService_GetFreeOnBoardTypesAsync()
        {
            var results = await _freeOnBoardTypesService.GetFreeOnBoardTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<FreeOnBoardTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FreeOnBoardTypesService_GetFreeOnBoardTypesAsync_Count()
        {
            var results = await _freeOnBoardTypesService.GetFreeOnBoardTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task FreeOnBoardTypesService_GetFreeOnBoardTypesAsync_Properties()
        {
            var result =
                (await _freeOnBoardTypesService.GetFreeOnBoardTypesAsync(true)).FirstOrDefault(x => x.Code == freeOnBoardTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task FreeOnBoardTypesService_GetFreeOnBoardTypesAsync_Expected()
        {
            var expectedResults = _freeOnBoardTypesCollection.FirstOrDefault(c => c.Guid == freeOnBoardTypesGuid);
            var actualResult =
                (await _freeOnBoardTypesService.GetFreeOnBoardTypesAsync(true)).FirstOrDefault(x => x.Id == freeOnBoardTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FreeOnBoardTypesService_GetFreeOnBoardTypesByGuidAsync_Empty()
        {
            await _freeOnBoardTypesService.GetFreeOnBoardTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FreeOnBoardTypesService_GetFreeOnBoardTypesByGuidAsync_Null()
        {
            await _freeOnBoardTypesService.GetFreeOnBoardTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FreeOnBoardTypesService_GetFreeOnBoardTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetFreeOnBoardTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _freeOnBoardTypesService.GetFreeOnBoardTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task FreeOnBoardTypesService_GetFreeOnBoardTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _freeOnBoardTypesCollection.First(c => c.Guid == freeOnBoardTypesGuid);
            var actualResult =
                await _freeOnBoardTypesService.GetFreeOnBoardTypesByGuidAsync(freeOnBoardTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task FreeOnBoardTypesService_GetFreeOnBoardTypesByGuidAsync_Properties()
        {
            var result =
                await _freeOnBoardTypesService.GetFreeOnBoardTypesByGuidAsync(freeOnBoardTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}