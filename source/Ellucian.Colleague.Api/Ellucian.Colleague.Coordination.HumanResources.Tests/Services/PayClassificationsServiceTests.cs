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
    public class PayClassificationsServiceTests
    {
        private const string payClassificationsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string payClassificationsCode = "AT";
        private ICollection<PayClassification> _payClassificationsCollection;
        private PayClassificationsService _payClassificationsService;

        private Mock<IPayClassificationsRepository> _payClassificationsRepositoryMock;
        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public async void Initialize()
        {
            _payClassificationsRepositoryMock = new Mock<IPayClassificationsRepository>();
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _payClassificationsCollection = new List<PayClassification>()
                {
                    new PayClassification("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new PayClassification("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new PayClassification("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _payClassificationsRepositoryMock.Setup(repo => repo.GetPayClassificationsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_payClassificationsCollection);

            _payClassificationsRepositoryMock.Setup(repo => repo.GetPayClassificationsByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_payClassificationsCollection.FirstOrDefault(pc => pc.Guid == payClassificationsGuid));

            _payClassificationsService = new PayClassificationsService(_payClassificationsRepositoryMock.Object,
				_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _payClassificationsService = null;
            _payClassificationsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsAsync()
        {
            var results = await _payClassificationsService.GetPayClassificationsAsync(true);
            Assert.IsTrue(results is IEnumerable<PayClassifications>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsAsync_Count()
        {
            var results = await _payClassificationsService.GetPayClassificationsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsAsync_Properties()
        {
            var result =
                (await _payClassificationsService.GetPayClassificationsAsync(true)).FirstOrDefault(x => x.Code == payClassificationsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNotNull(result.Title);
           
        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsAsync_Expected()
        {
            var expectedResults = _payClassificationsCollection.FirstOrDefault(c => c.Guid == payClassificationsGuid);
            var actualResult =
                (await _payClassificationsService.GetPayClassificationsAsync(true)).FirstOrDefault(x => x.Id == payClassificationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task PayClassificationsService_GetPayClassificationsByGuidAsync_Empty()
        {
            await _payClassificationsService.GetPayClassificationsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task PayClassificationsService_GetPayClassificationsByGuidAsync_Null()
        {
            await _payClassificationsService.GetPayClassificationsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task PayClassificationsService_GetPayClassificationsByGuidAsync_InvalidId()
        {
            _payClassificationsRepositoryMock.Setup(repo => repo.GetPayClassificationsByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            await _payClassificationsService.GetPayClassificationsByGuidAsync("99");
        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsByGuidAsync_Expected()
        {
            var expectedResults =
                _payClassificationsCollection.First(c => c.Guid == payClassificationsGuid);
            var actualResult =
                await _payClassificationsService.GetPayClassificationsByGuidAsync(payClassificationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsByGuidAsync_Properties()
        {
            var result =
                await _payClassificationsService.GetPayClassificationsByGuidAsync(payClassificationsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNotNull(result.Title);
            
        }
    }

    [TestClass]
    public class PayClassifications2ServiceTests
    {
        private const string payClassificationsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string payClassificationsCode = "AT";
        private ICollection<PayClassification> _payClassificationsCollection;
        private PayClassificationsService _payClassificationsService;

        private Mock<IPayClassificationsRepository> _payClassificationsRepositoryMock;
        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _payClassificationsRepositoryMock = new Mock<IPayClassificationsRepository>();
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _payClassificationsCollection = new List<PayClassification>()
                {
                    new PayClassification("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new PayClassification("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new PayClassification("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _payClassificationsRepositoryMock.Setup(repo => repo.GetPayClassificationsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_payClassificationsCollection);

            _payClassificationsRepositoryMock.Setup(repo => repo.GetPayClassificationsByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_payClassificationsCollection.FirstOrDefault(pc => pc.Guid == payClassificationsGuid));

            _payClassificationsService = new PayClassificationsService(_payClassificationsRepositoryMock.Object,
                _referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _payClassificationsService = null;
            _payClassificationsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsAsync()
        {
            var results = await _payClassificationsService.GetPayClassifications2Async(true);
            Assert.IsTrue(results is IEnumerable<PayClassifications2>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsAsync_Count()
        {
            var results = await _payClassificationsService.GetPayClassifications2Async(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsAsync_Properties()
        {
            var result =
                (await _payClassificationsService.GetPayClassifications2Async(true)).FirstOrDefault(x => x.Code == payClassificationsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNotNull(result.Title);

        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsAsync_Expected()
        {
            var expectedResults = _payClassificationsCollection.FirstOrDefault(c => c.Guid == payClassificationsGuid);
            var actualResult =
                (await _payClassificationsService.GetPayClassifications2Async(true)).FirstOrDefault(x => x.Id == payClassificationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PayClassificationsService_GetPayClassificationsByGuidAsync_Empty()
        {
            await _payClassificationsService.GetPayClassificationsByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PayClassificationsService_GetPayClassificationsByGuidAsync_Null()
        {
            await _payClassificationsService.GetPayClassificationsByGuid2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayClassificationsService_GetPayClassificationsByGuidAsync_InvalidId()
        {
            _payClassificationsRepositoryMock.Setup(repo => repo.GetPayClassificationsByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            await _payClassificationsService.GetPayClassificationsByGuid2Async("99");
        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsByGuidAsync_Expected()
        {
            var expectedResults =
                _payClassificationsCollection.First(c => c.Guid == payClassificationsGuid);
            var actualResult =
                await _payClassificationsService.GetPayClassificationsByGuid2Async(payClassificationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task PayClassificationsService_GetPayClassificationsByGuidAsync_Properties()
        {
            var result =
                await _payClassificationsService.GetPayClassificationsByGuid2Async(payClassificationsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNotNull(result.Title);

        }
    }
}