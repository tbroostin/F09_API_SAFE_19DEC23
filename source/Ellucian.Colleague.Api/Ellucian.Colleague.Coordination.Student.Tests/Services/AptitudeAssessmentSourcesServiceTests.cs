//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AptitudeAssessmentSourcesServiceTests
    {
        private const string aptitudeAssessmentSourcesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string aptitudeAssessmentSourcesCode = "AT";
        private ICollection<TestSource> _aptitudeAssessmentSourcesCollection;
        private AptitudeAssessmentSourcesService _aptitudeAssessmentSourcesService;
        
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _aptitudeAssessmentSourcesCollection = new List<TestSource>()
                {
                    new TestSource("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new TestSource("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new TestSource("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetTestSourcesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_aptitudeAssessmentSourcesCollection);

            _aptitudeAssessmentSourcesService = new AptitudeAssessmentSourcesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _aptitudeAssessmentSourcesService = null;
            _aptitudeAssessmentSourcesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task AptitudeAssessmentSourcesService_GetAptitudeAssessmentSourcesAsync()
        {
            var results = await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesAsync(true);
            Assert.IsTrue(results is IEnumerable<AptitudeAssessmentSources>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AptitudeAssessmentSourcesService_GetAptitudeAssessmentSourcesAsync_Count()
        {
            var results = await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task AptitudeAssessmentSourcesService_GetAptitudeAssessmentSourcesAsync_Properties()
        {
            var result =
                (await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesAsync(true)).FirstOrDefault(x => x.Code == aptitudeAssessmentSourcesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task AptitudeAssessmentSourcesService_GetAptitudeAssessmentSourcesAsync_Expected()
        {
            var expectedResults = _aptitudeAssessmentSourcesCollection.FirstOrDefault(c => c.Guid == aptitudeAssessmentSourcesGuid);
            var actualResult =
                (await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesAsync(true)).FirstOrDefault(x => x.Id == aptitudeAssessmentSourcesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AptitudeAssessmentSourcesService_GetAptitudeAssessmentSourcesByGuidAsync_Empty()
        {
            await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AptitudeAssessmentSourcesService_GetAptitudeAssessmentSourcesByGuidAsync_Null()
        {
            await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AptitudeAssessmentSourcesService_GetAptitudeAssessmentSourcesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetTestSourcesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AptitudeAssessmentSourcesService_GetAptitudeAssessmentSourcesByGuidAsync_Expected()
        {
            var expectedResults =
                _aptitudeAssessmentSourcesCollection.First(c => c.Guid == aptitudeAssessmentSourcesGuid);
            var actualResult =
                await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesByGuidAsync(aptitudeAssessmentSourcesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task AptitudeAssessmentSourcesService_GetAptitudeAssessmentSourcesByGuidAsync_Properties()
        {
            var result =
                await _aptitudeAssessmentSourcesService.GetAptitudeAssessmentSourcesByGuidAsync(aptitudeAssessmentSourcesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}