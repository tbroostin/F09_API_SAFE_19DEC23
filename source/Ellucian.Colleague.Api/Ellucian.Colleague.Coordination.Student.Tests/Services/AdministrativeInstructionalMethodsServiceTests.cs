//Copyright 2018 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{    
    [TestClass]
    public class AdministrativeInstructionalMethodsServiceTests
    {
        private const string administrativeInstructionalMethodsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string administrativeInstructionalMethodsCode = "LEC";
        private ICollection<AdministrativeInstructionalMethod> _administrativeInstructionalMethodsCollection;
        private AdministrativeInstructionalMethodsService _administrativeInstructionalMethodsService;
        
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _administrativeInstructionalMethodsCollection = new List<AdministrativeInstructionalMethod>()
                {
                    new AdministrativeInstructionalMethod("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "LEC", "Lecture", "D8CED21A-F220-4F79-9544-706E13B51972"),
                    new AdministrativeInstructionalMethod("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "LAB", "Laboratory", "05F052C-7B63-492D-A7CA-5769CE003274"),
                    new AdministrativeInstructionalMethod("d2253ac7-9931-4560-b42f-1fccd43c952e", "OL", "Online Learning", "67B0664B-0650-4C88-ACC6-FB0C689CB519")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetAdministrativeInstructionalMethodsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_administrativeInstructionalMethodsCollection);

            _administrativeInstructionalMethodsService = new AdministrativeInstructionalMethodsService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _administrativeInstructionalMethodsService = null;
            _administrativeInstructionalMethodsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task AdministrativeInstructionalMethodsService_GetAdministrativeInstructionalMethodsAsync()
        {
            var results = await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsAsync(true);
            Assert.IsTrue(results is IEnumerable<AdministrativeInstructionalMethods>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AdministrativeInstructionalMethodsService_GetAdministrativeInstructionalMethodsAsync_Count()
        {
            var results = await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task AdministrativeInstructionalMethodsService_GetAdministrativeInstructionalMethodsAsync_Properties()
        {
            var result =
                (await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsAsync(true)).FirstOrDefault(x => x.Code == administrativeInstructionalMethodsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task AdministrativeInstructionalMethodsService_GetAdministrativeInstructionalMethodsAsync_Expected()
        {
            var expectedResults = _administrativeInstructionalMethodsCollection.FirstOrDefault(c => c.Guid == administrativeInstructionalMethodsGuid);
            var actualResult =
                (await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsAsync(true)).FirstOrDefault(x => x.Id == administrativeInstructionalMethodsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AdministrativeInstructionalMethodsService_GetAdministrativeInstructionalMethodsByGuidAsync_Empty()
        {
            await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AdministrativeInstructionalMethodsService_GetAdministrativeInstructionalMethodsByGuidAsync_Null()
        {
            await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AdministrativeInstructionalMethodsService_GetAdministrativeInstructionalMethodsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAdministrativeInstructionalMethodsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsByGuidAsync("99");
        }

        [TestMethod]
        public async Task AdministrativeInstructionalMethodsService_GetAdministrativeInstructionalMethodsByGuidAsync_Expected()
        {
            var expectedResults =
                _administrativeInstructionalMethodsCollection.First(c => c.Guid == administrativeInstructionalMethodsGuid);
            var actualResult =
                await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsByGuidAsync(administrativeInstructionalMethodsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task AdministrativeInstructionalMethodsService_GetAdministrativeInstructionalMethodsByGuidAsync_Properties()
        {
            var result =
                await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsByGuidAsync(administrativeInstructionalMethodsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}