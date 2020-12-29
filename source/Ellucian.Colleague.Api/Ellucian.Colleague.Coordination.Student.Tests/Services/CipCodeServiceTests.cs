//Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    public class CipCodeServiceTests
    {
        private const string cipCodesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string cipCodesCode = "AT";
        private ICollection<CipCode> _cipCodesCollection;
        private CipCodeService _cipCodesService;

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


            _cipCodesCollection = new List<CipCode>()
                {
                    new CipCode("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", 2020),
                    new CipCode("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", 2020),
                    new CipCode("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", 2020)
                };


            _referenceRepositoryMock.Setup(repo => repo.GetCipCodesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_cipCodesCollection);

            _cipCodesService = new CipCodeService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _cipCodesService = null;
            _cipCodesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task CipCodeService_GetCipCodesAsync()
        {
            var results = await _cipCodesService.GetCipCodesAsync(true);
            Assert.IsTrue(results is IEnumerable<Dtos.CipCode>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task CipCodeService_GetCipCodesAsync_Count()
        {
            var results = await _cipCodesService.GetCipCodesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task CipCodeService_GetCipCodesAsync_Properties()
        {
            var result =
                (await _cipCodesService.GetCipCodesAsync(true)).FirstOrDefault(x => x.Code == cipCodesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.RevisionYear);
        }

        [TestMethod]
        public async Task CipCodeService_GetCipCodesAsync_Expected()
        {
            var expectedResults = _cipCodesCollection.FirstOrDefault(c => c.Guid == cipCodesGuid);
            var actualResult =
                (await _cipCodesService.GetCipCodesAsync(true)).FirstOrDefault(x => x.Id == cipCodesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            Assert.AreEqual(expectedResults.RevisionYear, actualResult.RevisionYear);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CipCodeService_GetCipCodesByGuidAsync_Empty()
        {
            await _cipCodesService.GetCipCodeByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CipCodeService_GetCipCodesByGuidAsync_Null()
        {
            await _cipCodesService.GetCipCodeByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CipCodeService_GetCipCodesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetCipCodesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _cipCodesService.GetCipCodeByGuidAsync("99");
        }

        [TestMethod]
        public async Task CipCodeService_GetCipCodesByGuidAsync_Expected()
        {
            var expectedResults =
                _cipCodesCollection.First(c => c.Guid == cipCodesGuid);
            var actualResult =
                await _cipCodesService.GetCipCodeByGuidAsync(cipCodesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            Assert.AreEqual(expectedResults.RevisionYear, actualResult.RevisionYear);
        }

        [TestMethod]
        public async Task CipCodeService_GetCipCodesByGuidAsync_Properties()
        {
            var result =
                await _cipCodesService.GetCipCodeByGuidAsync(cipCodesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.RevisionYear);
        }
    }
}