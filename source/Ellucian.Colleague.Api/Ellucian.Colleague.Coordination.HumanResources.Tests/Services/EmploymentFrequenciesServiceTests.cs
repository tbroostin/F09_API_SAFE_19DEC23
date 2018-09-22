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
    public class EmploymentFrequenciesServiceTests
    {
        private const string employmentFrequenciesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string employmentFrequenciesCode = "AT";
        private ICollection<EmploymentFrequency> _employmentFrequenciesCollection;
        private EmploymentFrequenciesService _employmentFrequenciesService;
        
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
           

            _employmentFrequenciesCollection = new List<EmploymentFrequency>()
                {
                    new EmploymentFrequency("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "1"),
                    new EmploymentFrequency("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "1"),
                    new EmploymentFrequency("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "1")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetEmploymentFrequenciesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_employmentFrequenciesCollection);

            _employmentFrequenciesService = new EmploymentFrequenciesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _employmentFrequenciesService = null;
            _employmentFrequenciesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task EmploymentFrequenciesService_GetEmploymentFrequenciesAsync()
        {
            var results = await _employmentFrequenciesService.GetEmploymentFrequenciesAsync(true);
            Assert.IsTrue(results is IEnumerable<EmploymentFrequencies>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task EmploymentFrequenciesService_GetEmploymentFrequenciesAsync_Count()
        {
            var results = await _employmentFrequenciesService.GetEmploymentFrequenciesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task EmploymentFrequenciesService_GetEmploymentFrequenciesAsync_Properties()
        {
            var result =
                (await _employmentFrequenciesService.GetEmploymentFrequenciesAsync(true)).FirstOrDefault(x => x.Code == employmentFrequenciesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task EmploymentFrequenciesService_GetEmploymentFrequenciesAsync_Expected()
        {
            var expectedResults = _employmentFrequenciesCollection.FirstOrDefault(c => c.Guid == employmentFrequenciesGuid);
            var actualResult =
                (await _employmentFrequenciesService.GetEmploymentFrequenciesAsync(true)).FirstOrDefault(x => x.Id == employmentFrequenciesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EmploymentFrequenciesService_GetEmploymentFrequenciesByGuidAsync_Empty()
        {
            await _employmentFrequenciesService.GetEmploymentFrequenciesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EmploymentFrequenciesService_GetEmploymentFrequenciesByGuidAsync_Null()
        {
            await _employmentFrequenciesService.GetEmploymentFrequenciesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EmploymentFrequenciesService_GetEmploymentFrequenciesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetEmploymentFrequenciesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _employmentFrequenciesService.GetEmploymentFrequenciesByGuidAsync("99");
        }

        [TestMethod]
        public async Task EmploymentFrequenciesService_GetEmploymentFrequenciesByGuidAsync_Expected()
        {
            var expectedResults =
                _employmentFrequenciesCollection.First(c => c.Guid == employmentFrequenciesGuid);
            var actualResult =
                await _employmentFrequenciesService.GetEmploymentFrequenciesByGuidAsync(employmentFrequenciesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task EmploymentFrequenciesService_GetEmploymentFrequenciesByGuidAsync_Properties()
        {
            var result =
                await _employmentFrequenciesService.GetEmploymentFrequenciesByGuidAsync(employmentFrequenciesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}