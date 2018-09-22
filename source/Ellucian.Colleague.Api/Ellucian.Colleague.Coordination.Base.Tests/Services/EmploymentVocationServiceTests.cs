//Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Data.Base.Repositories;
using Vocation = Ellucian.Colleague.Domain.Base.Entities.Vocation;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;


namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class EmploymentVocationServiceTests
    {
        private const string vocationsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string vocationsCode = "AT";
        private IEnumerable<Vocation> _vocationsCollection;
        private EmploymentVocationService _employmentVocationsService;
        private Mock<ILogger> _loggerMock;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<IConfigurationRepository> _baseConfigurationRepoMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IRoleRepository> _roleRepoMock;

        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _baseConfigurationRepoMock = new Mock<IConfigurationRepository>();
            _roleRepoMock = new Mock<IRoleRepository>();

            _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();
            _vocationsCollection = new List<Vocation>()
                {
                    new Vocation("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Vocation("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Vocation("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            _employmentVocationsService = new EmploymentVocationService(_referenceRepositoryMock.Object, _baseConfigurationRepoMock.Object, _adapterRegistryMock.Object, _currentUserFactory, _roleRepoMock.Object, _loggerMock.Object);
            _referenceRepositoryMock.Setup(repo => repo.GetVocationsAsync(It.IsAny<bool>())).ReturnsAsync(_vocationsCollection);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _employmentVocationsService = null;
            _vocationsCollection = null;
            _referenceRepositoryMock = null;
            _adapterRegistryMock = null;
            _currentUserFactory = null;
            _roleRepoMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task VocationsService_GetVocationsAsync()
        {
            var results = await _employmentVocationsService.GetEmploymentVocationsAsync(true);
            Assert.IsTrue(results is IEnumerable<EmploymentVocation>);
            Assert.IsNotNull(results); 
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task VocationsService_GetVocationsByGuidAsync_Expected()
        {
            var expectedResults = _vocationsCollection.First(c => c.Guid == vocationsGuid);
            var actualResult = await _employmentVocationsService.GetEmploymentVocationByGuidAsync(vocationsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VocationsService_GetVocationsByGuidAsync_KeyNotFoundException()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetVocationsAsync(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            await _employmentVocationsService.GetEmploymentVocationByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VocationsService_GetVocationsByGuidAsync_InvalidOperationException()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetVocationsAsync(It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException());
            await _employmentVocationsService.GetEmploymentVocationByGuidAsync("123");
        }
    }
}
