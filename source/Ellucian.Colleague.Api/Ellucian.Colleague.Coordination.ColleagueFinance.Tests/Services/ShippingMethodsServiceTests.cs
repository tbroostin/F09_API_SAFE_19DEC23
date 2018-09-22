//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{    
    [TestClass]
    public class ShippingMethodsServiceTests : CurrentUserSetup
    {
        private const string shippingMethodsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string shippingMethodsCode = "AT";
        private ICollection<ShippingMethod> _shippingMethodsCollection;
        private ShippingMethodsService _shippingMethodsService;
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

            _shippingMethodsCollection = new List<ShippingMethod>()
                {
                    new ShippingMethod("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new ShippingMethod("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new ShippingMethod("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetShippingMethodsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_shippingMethodsCollection);

            _shippingMethodsService = new ShippingMethodsService(_referenceRepositoryMock.Object, _configurationRepositoryMock.Object, adapterRegistry, currentUserFactory, roleRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _shippingMethodsService = null;
            _shippingMethodsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task ShippingMethodsService_GetShippingMethodsAsync()
        {
            var results = await _shippingMethodsService.GetShippingMethodsAsync(true);
            Assert.IsTrue(results is IEnumerable<ShippingMethods>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task ShippingMethodsService_GetShippingMethodsAsync_Count()
        {
            var results = await _shippingMethodsService.GetShippingMethodsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task ShippingMethodsService_GetShippingMethodsAsync_Properties()
        {
            var result =
                (await _shippingMethodsService.GetShippingMethodsAsync(true)).FirstOrDefault(x => x.Code == shippingMethodsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task ShippingMethodsService_GetShippingMethodsAsync_Expected()
        {
            var expectedResults = _shippingMethodsCollection.FirstOrDefault(c => c.Guid == shippingMethodsGuid);
            var actualResult =
                (await _shippingMethodsService.GetShippingMethodsAsync(true)).FirstOrDefault(x => x.Id == shippingMethodsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task ShippingMethodsService_GetShippingMethodsByGuidAsync_Empty()
        {
            await _shippingMethodsService.GetShippingMethodsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task ShippingMethodsService_GetShippingMethodsByGuidAsync_Null()
        {
            await _shippingMethodsService.GetShippingMethodsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task ShippingMethodsService_GetShippingMethodsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetShippingMethodsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _shippingMethodsService.GetShippingMethodsByGuidAsync("99");
        }

        [TestMethod]
        public async Task ShippingMethodsService_GetShippingMethodsByGuidAsync_Expected()
        {
            var expectedResults =
                _shippingMethodsCollection.First(c => c.Guid == shippingMethodsGuid);
            var actualResult =
                await _shippingMethodsService.GetShippingMethodsByGuidAsync(shippingMethodsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task ShippingMethodsService_GetShippingMethodsByGuidAsync_Properties()
        {
            var result =
                await _shippingMethodsService.GetShippingMethodsByGuidAsync(shippingMethodsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}