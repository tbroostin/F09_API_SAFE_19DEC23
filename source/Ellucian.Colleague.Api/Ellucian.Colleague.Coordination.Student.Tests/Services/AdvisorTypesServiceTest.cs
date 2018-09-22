//Copyright 2017 Ellucian Company L.P. and its affiliates.


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
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AdvisorTypesServiceTests : SectionCoordinationServiceTests.CurrentUserSetup
    {
        private const string advisorTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string advisorTypesCode = "AT";
        private ICollection<AdvisorType> _advisorTypesCollection;
        private AdvisorTypesService _advisorTypesService;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IStudentRepository> _studentRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _studentRepositoryMock = new Mock<IStudentRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;
            _loggerMock = new Mock<ILogger>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            // Set up current user
            _currentUserFactory = new SectionCoordinationServiceTests.CurrentUserSetup.StudentUserFactory();

            _advisorTypesCollection = new List<AdvisorType>()
                {
                    new AdvisorType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "1"),
                    new AdvisorType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "2"),
                    new AdvisorType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "3")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetAdvisorTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_advisorTypesCollection);

            _advisorTypesService = new AdvisorTypesService(_referenceRepositoryMock.Object, _studentRepositoryMock.Object, _adapterRegistryMock.Object,
                _currentUserFactory, _roleRepo, _loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _advisorTypesService = null;
            _advisorTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task AdvisorTypesService_GetAdvisorTypesAsync()
        {
            var results = await _advisorTypesService.GetAdvisorTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<AdvisorTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AdvisorTypesService_GetAdvisorTypesAsync_Count()
        {
            var results = await _advisorTypesService.GetAdvisorTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task AdvisorTypesService_GetAdvisorTypesAsync_Properties()
        {
            var result =
                (await _advisorTypesService.GetAdvisorTypesAsync(true)).FirstOrDefault(x => x.Code == advisorTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task AdvisorTypesService_GetAdvisorTypesAsync_Expected()
        {
            var expectedResults = _advisorTypesCollection.FirstOrDefault(c => c.Guid == advisorTypesGuid);
            var actualResult =
                (await _advisorTypesService.GetAdvisorTypesAsync(true)).FirstOrDefault(x => x.Id == advisorTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AdvisorTypesService_GetAdvisorTypesByGuidAsync_Empty()
        {
            await _advisorTypesService.GetAdvisorTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AdvisorTypesService_GetAdvisorTypesByGuidAsync_Null()
        {
            await _advisorTypesService.GetAdvisorTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AdvisorTypesService_GetAdvisorTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAdvisorTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _advisorTypesService.GetAdvisorTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AdvisorTypesService_GetAdvisorTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _advisorTypesCollection.First(c => c.Guid == advisorTypesGuid);
            var actualResult =
                await _advisorTypesService.GetAdvisorTypesByGuidAsync(advisorTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task AdvisorTypesService_GetAdvisorTypesByGuidAsync_Properties()
        {
            var result =
                await _advisorTypesService.GetAdvisorTypesByGuidAsync(advisorTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}