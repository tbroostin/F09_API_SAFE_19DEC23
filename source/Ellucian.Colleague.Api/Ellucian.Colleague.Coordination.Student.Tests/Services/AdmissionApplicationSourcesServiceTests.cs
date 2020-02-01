//Copyright 2019 Ellucian Company L.P. and its affiliates.
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

namespace Ellucian.Colleague.Coordination.Recruitment.Tests.Services
{
    [TestClass]
    public class AdmissionApplicationSourcesServiceTests
    {
        private const string admissionApplicationSourcesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string admissionApplicationSourcesCode = "AT";
        private ICollection<ApplicationSource> _admissionApplicationSourcesCollection;
        private AdmissionApplicationSourcesService _admissionApplicationSourcesService;

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


            _admissionApplicationSourcesCollection = new List<ApplicationSource>()
                {
                    new ApplicationSource("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new ApplicationSource("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new ApplicationSource("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetApplicationSourcesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_admissionApplicationSourcesCollection);

            _admissionApplicationSourcesService = new AdmissionApplicationSourcesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _admissionApplicationSourcesService = null;
            _admissionApplicationSourcesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task AdmissionApplicationSourcesService_GetAdmissionApplicationSourcesAsync()
        {
            var results = await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesAsync(true);
            Assert.IsTrue(results is IEnumerable<AdmissionApplicationSources>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AdmissionApplicationSourcesService_GetAdmissionApplicationSourcesAsync_Count()
        {
            var results = await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task AdmissionApplicationSourcesService_GetAdmissionApplicationSourcesAsync_Properties()
        {
            var result =
                (await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesAsync(true)).FirstOrDefault(x => x.Code == admissionApplicationSourcesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task AdmissionApplicationSourcesService_GetAdmissionApplicationSourcesAsync_Expected()
        {
            var expectedResults = _admissionApplicationSourcesCollection.FirstOrDefault(c => c.Guid == admissionApplicationSourcesGuid);
            var actualResult =
                (await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesAsync(true)).FirstOrDefault(x => x.Id == admissionApplicationSourcesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationSourcesService_GetAdmissionApplicationSourcesByGuidAsync_Empty()
        {
            await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationSourcesService_GetAdmissionApplicationSourcesByGuidAsync_Null()
        {
            await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationSourcesService_GetAdmissionApplicationSourcesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetApplicationSourcesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AdmissionApplicationSourcesService_GetAdmissionApplicationSourcesByGuidAsync_Expected()
        {
            var expectedResults =
                _admissionApplicationSourcesCollection.First(c => c.Guid == admissionApplicationSourcesGuid);
            var actualResult =
                await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesByGuidAsync(admissionApplicationSourcesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AdmissionApplicationSourcesService_GetAdmissionApplicationSourcesByGuidAsync_Properties()
        {
            var result =
                await _admissionApplicationSourcesService.GetAdmissionApplicationSourcesByGuidAsync(admissionApplicationSourcesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}