//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AdmissionApplicationInfluencesServiceTests
    {
        private const string admissionApplicationInfluencesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string admissionApplicationInfluencesCode = "AT";
        private ICollection<ApplicationInfluence> _admissionApplicationInfluencesCollection;
        private AdmissionApplicationInfluencesService _admissionApplicationInfluencesService;

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


            _admissionApplicationInfluencesCollection = new List<ApplicationInfluence>()
                {
                    new ApplicationInfluence("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new ApplicationInfluence("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new ApplicationInfluence("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetApplicationInfluencesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_admissionApplicationInfluencesCollection);

            _admissionApplicationInfluencesService = new AdmissionApplicationInfluencesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _admissionApplicationInfluencesService = null;
            _admissionApplicationInfluencesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task AdmissionApplicationInfluencesService_GetAdmissionApplicationInfluencesAsync()
        {
            var results = await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesAsync(true);
            Assert.IsTrue(results is IEnumerable<AdmissionApplicationInfluences>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AdmissionApplicationInfluencesService_GetAdmissionApplicationInfluencesAsync_Count()
        {
            var results = await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task AdmissionApplicationInfluencesService_GetAdmissionApplicationInfluencesAsync_Properties()
        {
            var result =
                (await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesAsync(true)).FirstOrDefault(x => x.Code == admissionApplicationInfluencesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task AdmissionApplicationInfluencesService_GetAdmissionApplicationInfluencesAsync_Expected()
        {
            var expectedResults = _admissionApplicationInfluencesCollection.FirstOrDefault(c => c.Guid == admissionApplicationInfluencesGuid);
            var actualResult =
                (await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesAsync(true)).FirstOrDefault(x => x.Id == admissionApplicationInfluencesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationInfluencesService_GetAdmissionApplicationInfluencesByGuidAsync_Empty()
        {
            await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationInfluencesService_GetAdmissionApplicationInfluencesByGuidAsync_Null()
        {
            await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationInfluencesService_GetAdmissionApplicationInfluencesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetApplicationInfluencesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AdmissionApplicationInfluencesService_GetAdmissionApplicationInfluencesByGuidAsync_Expected()
        {
            var expectedResults =
                _admissionApplicationInfluencesCollection.First(c => c.Guid == admissionApplicationInfluencesGuid);
            var actualResult =
                await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesByGuidAsync(admissionApplicationInfluencesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AdmissionApplicationInfluencesService_GetAdmissionApplicationInfluencesByGuidAsync_Properties()
        {
            var result =
                await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesByGuidAsync(admissionApplicationInfluencesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}