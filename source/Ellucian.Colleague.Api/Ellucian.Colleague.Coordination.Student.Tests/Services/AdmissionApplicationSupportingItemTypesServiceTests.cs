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
    public class AdmissionApplicationSupportingItemTypesServiceTests
    {
        private const string admissionApplicationSupportingItemTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string admissionApplicationSupportingItemTypesCode = "AMC1COLN";
        private ICollection<Ellucian.Colleague.Domain.Base.Entities.CommunicationCode> _applicationStatusesCollection;
        private AdmissionApplicationSupportingItemTypesService _admissionApplicationSupportingItemTypesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserMock;
        private Mock<IRoleRepository> _roleRepository;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserMock = new Mock<ICurrentUserFactory>();
            _roleRepository = new Mock<IRoleRepository>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            _applicationStatusesCollection = new List<Ellucian.Colleague.Domain.Base.Entities.CommunicationCode>()
                {
                    new Ellucian.Colleague.Domain.Base.Entities.CommunicationCode("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AMC1COLN", "Outreach/College Night"),
                    new Ellucian.Colleague.Domain.Base.Entities.CommunicationCode("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AMC1COMP", "Outreach/Company Visit"),
                    new Ellucian.Colleague.Domain.Base.Entities.CommunicationCode("d2253ac7-9931-4560-b42f-1fccd43c952e", "AMC1HSV", "Outreach/High School Visit"),
                    new Ellucian.Colleague.Domain.Base.Entities.CommunicationCode("e2253ac7-9931-4560-b42f-1fccd43c952f", "AMC1NACA", "Outreach/NACAC Fair"),
                    new Ellucian.Colleague.Domain.Base.Entities.CommunicationCode("g2253ac7-9931-4560-b42f-1fccd43c952h", "AMC1OINT", "Outreach/Off-Campus Interview"),
                    new Ellucian.Colleague.Domain.Base.Entities.CommunicationCode("i2253ac7-9931-4560-b42f-1fccd43c952j", "AMC5GC", "Referral/Guidance Counselor")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetAdmissionApplicationSupportingItemTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_applicationStatusesCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetAdmissionApplicationSupportingItemTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_applicationStatusesCollection);

            _admissionApplicationSupportingItemTypesService = new AdmissionApplicationSupportingItemTypesService(_adapterRegistryMock.Object, _currentUserMock.Object,
               _roleRepository.Object, baseConfigurationRepository, _referenceRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _admissionApplicationSupportingItemTypesService = null;
            _applicationStatusesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemTypesService_GetAdmissionApplicationSupportingItemTypesAsync()
        {
            var results = await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemTypesService_GetAdmissionApplicationSupportingItemTypesAsync_Count()
        {
            var results = await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesAsync(true);
            Assert.AreEqual(6, results.Count());
        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemTypesService_GetAdmissionApplicationSupportingItemTypesAsync_Properties()
        {
            var result =
                (await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesAsync(true)).FirstOrDefault(x => x.Code == admissionApplicationSupportingItemTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemTypesService_GetAdmissionApplicationSupportingItemTypesAsync_Expected()
        {
            var expectedResults = _applicationStatusesCollection.FirstOrDefault(c => c.Guid == admissionApplicationSupportingItemTypesGuid);
            var actualResult =
                (await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesAsync(true)).FirstOrDefault(x => x.Id == admissionApplicationSupportingItemTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationSupportingItemTypesService_GetAdmissionApplicationSupportingItemTypesByGuidAsync_Empty()
        {
            await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationSupportingItemTypesService_GetAdmissionApplicationSupportingItemTypesByGuidAsync_Null()
        {
            await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationSupportingItemTypesService_GetAdmissionApplicationSupportingItemTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAdmissionApplicationSupportingItemTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemTypesService_GetAdmissionApplicationSupportingItemTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _applicationStatusesCollection.First(c => c.Guid == admissionApplicationSupportingItemTypesGuid);
            var actualResult =
                await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesByGuidAsync(admissionApplicationSupportingItemTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AdmissionApplicationSupportingItemTypesService_GetAdmissionApplicationSupportingItemTypesByGuidAsync_Properties()
        {
            var result =
                await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesByGuidAsync(admissionApplicationSupportingItemTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}