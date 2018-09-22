//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{    
    [TestClass]
    public class PayClassesServiceTests
    {
        private const string payClassesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string payClassesCode = "AT";
        private ICollection<PayClass> _payClassesCollection;
        private PayClassesService _payClassesService;
        
        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _payClassesCollection = new List<PayClass>()
                {
                    new PayClass("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new PayClass("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new PayClass("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetPayClassesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_payClassesCollection);

            _payClassesService = new PayClassesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _payClassesService = null;
            _payClassesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesAsync()
        {
            var results = await _payClassesService.GetPayClassesAsync(true);
            Assert.IsTrue(results is IEnumerable<PayClasses>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesAsync_Count()
        {
            var results = await _payClassesService.GetPayClassesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesAsync_Properties()
        {
            var result =
                (await _payClassesService.GetPayClassesAsync(true)).FirstOrDefault(x => x.Code == payClassesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesAsync_Expected()
        {
            var expectedResults = _payClassesCollection.FirstOrDefault(c => c.Guid == payClassesGuid);
            var actualResult =
                (await _payClassesService.GetPayClassesAsync(true)).FirstOrDefault(x => x.Id == payClassesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task PayClassesService_GetPayClassesByGuidAsync_Empty()
        {
            await _payClassesService.GetPayClassesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task PayClassesService_GetPayClassesByGuidAsync_Null()
        {
            await _payClassesService.GetPayClassesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task PayClassesService_GetPayClassesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetPayClassesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _payClassesService.GetPayClassesByGuidAsync("99");
        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesByGuidAsync_Expected()
        {
            var expectedResults =
                _payClassesCollection.First(c => c.Guid == payClassesGuid);
            var actualResult =
                await _payClassesService.GetPayClassesByGuidAsync(payClassesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesByGuidAsync_Properties()
        {
            var result =
                await _payClassesService.GetPayClassesByGuidAsync(payClassesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }

    [TestClass]
    public class PayClasses2ServiceTests
    {
        private const string payClassesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string payClassesCode = "AT";
        private ICollection<PayClass> _payClassesCollection;
        private PayClassesService _payClassesService;

        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _payClassesCollection = new List<PayClass>()
                {
                    new PayClass("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new PayClass("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new PayClass("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetPayClassesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_payClassesCollection);

            _payClassesService = new PayClassesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _payClassesService = null;
            _payClassesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesAsync()
        {
            var results = await _payClassesService.GetPayClasses2Async(true);
            Assert.IsTrue(results is IEnumerable<PayClasses2>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesAsync_Count()
        {
            var results = await _payClassesService.GetPayClasses2Async(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesAsync_Properties()
        {
            var result =
                (await _payClassesService.GetPayClasses2Async(true)).FirstOrDefault(x => x.Code == payClassesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesAsync_Expected()
        {
            var expectedResults = _payClassesCollection.FirstOrDefault(c => c.Guid == payClassesGuid);
            var actualResult =
                (await _payClassesService.GetPayClasses2Async(true)).FirstOrDefault(x => x.Id == payClassesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayClassesService_GetPayClassesByGuidAsync_Empty()
        {
            await _payClassesService.GetPayClassesByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayClassesService_GetPayClassesByGuidAsync_Null()
        {
            await _payClassesService.GetPayClassesByGuid2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayClassesService_GetPayClassesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetPayClassesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _payClassesService.GetPayClassesByGuid2Async("99");
        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesByGuidAsync_Expected()
        {
            var expectedResults =
                _payClassesCollection.First(c => c.Guid == payClassesGuid);
            var actualResult =
                await _payClassesService.GetPayClassesByGuid2Async(payClassesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task PayClassesService_GetPayClassesByGuidAsync_Properties()
        {
            var result =
                await _payClassesService.GetPayClassesByGuid2Async(payClassesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}