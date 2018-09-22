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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AdmissionDecisionTypesServiceTests
    {
        private const string admissionDecisionTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string admissionDecisionTypesCode = "Started";
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType> _decisionTypeesCollection;
        private AdmissionDecisionTypesService _admissionDecisionTypesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserMock;
        private Mock<IRoleRepository> _roleRepository;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserMock = new Mock<ICurrentUserFactory>();
            _roleRepository = new Mock<IRoleRepository>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            _decisionTypeesCollection = new List<Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "Started", "Started"){ AdmissionApplicationStatusTypesCategory = AdmissionApplicationStatusTypesCategory.Started },
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "Submitted", "Submitted"){ AdmissionApplicationStatusTypesCategory = AdmissionApplicationStatusTypesCategory.Submitted },
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("d2253ac7-9931-4560-b42f-1fccd43c952e", "Readyforreview", "Readyforreview"){ AdmissionApplicationStatusTypesCategory = AdmissionApplicationStatusTypesCategory.Readyforreview },
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("e2253ac7-9931-4560-b42f-1fccd43c952f", "Decisionmade", "Decisionmade"){ AdmissionApplicationStatusTypesCategory = AdmissionApplicationStatusTypesCategory.Decisionmade },
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("g2253ac7-9931-4560-b42f-1fccd43c952h", "Enrollmentcomplete", "Enrollmentcomplete"){ AdmissionApplicationStatusTypesCategory = AdmissionApplicationStatusTypesCategory.Enrollmentcomplete },
                    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("i2253ac7-9931-4560-b42f-1fccd43c952j", "ET", "Ectra Terrestrial"){ AdmissionApplicationStatusTypesCategory = AdmissionApplicationStatusTypesCategory.NotSet }
                };


            _referenceRepositoryMock.Setup(repo => repo.GetAdmissionDecisionTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_decisionTypeesCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetAdmissionDecisionTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_decisionTypeesCollection);

            _admissionDecisionTypesService = new AdmissionDecisionTypesService(_referenceRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserMock.Object,
               _roleRepository.Object, _loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _admissionDecisionTypesService = null;
            _decisionTypeesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesAsync()
        {
            var results = await _admissionDecisionTypesService.GetAdmissionDecisionTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisionType2>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesAsync_Count()
        {
            var results = await _admissionDecisionTypesService.GetAdmissionDecisionTypesAsync(true);
            Assert.AreEqual(6, results.Count());
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesAsync_Properties()
        {
            var result =
                (await _admissionDecisionTypesService.GetAdmissionDecisionTypesAsync(true)).FirstOrDefault(x => x.Code == admissionDecisionTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesAsync_Expected()
        {
            var expectedResults = _decisionTypeesCollection.FirstOrDefault(c => c.Guid == admissionDecisionTypesGuid);
            var actualResult =
                (await _admissionDecisionTypesService.GetAdmissionDecisionTypesAsync(true)).FirstOrDefault(x => x.Id == admissionDecisionTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesByGuidAsync_Empty()
        {
            await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesByGuidAsync_Null()
        {
            await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAdmissionDecisionTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _decisionTypeesCollection.First(c => c.Guid == admissionDecisionTypesGuid);
            var actualResult =
                await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync(admissionDecisionTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesByGuidAsync_Properties()
        {
            var result =
                await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync(admissionDecisionTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypes2Async()
        {
            var results = await _admissionDecisionTypesService.GetAdmissionDecisionTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisionType2>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypes2Async_Count()
        {
            var results = await _admissionDecisionTypesService.GetAdmissionDecisionTypesAsync(true);
            Assert.AreEqual(6, results.Count());
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypes2Async_Properties()
        {
            var result =
                (await _admissionDecisionTypesService.GetAdmissionDecisionTypesAsync(true)).FirstOrDefault(x => x.Code == admissionDecisionTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypes2Async_Expected()
        {
            var expectedResults = _decisionTypeesCollection.FirstOrDefault(c => c.Guid == admissionDecisionTypesGuid);
            var actualResult =
                (await _admissionDecisionTypesService.GetAdmissionDecisionTypesAsync(true)).FirstOrDefault(x => x.Id == admissionDecisionTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesByGuid2Async_Empty()
        {
            await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesByGuid2Async_Null()
        {
            await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesByGuid2Async_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAdmissionDecisionTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesByGuid2Async_Expected()
        {
            var expectedResults =
                _decisionTypeesCollection.First(c => c.Guid == admissionDecisionTypesGuid);
            var actualResult =
                await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync(admissionDecisionTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AdmissionDecisionTypesService_GetAdmissionDecisionTypesByGuid2Async_Properties()
        {
            var result =
                await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync(admissionDecisionTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}