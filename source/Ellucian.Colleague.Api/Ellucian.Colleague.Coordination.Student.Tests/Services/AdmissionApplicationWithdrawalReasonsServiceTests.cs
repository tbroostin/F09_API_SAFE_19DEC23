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
    public class AdmissionApplicationWithdrawalReasonsServiceTests
    {
        private const string admissionApplicationWithdrawalReasonsGuid = "761597be-0a12-4aa8-8ffe-afc04b62da41";
        private const string admissionApplicationWithdrawalReasonsCode = "AC";
        private ICollection<WithdrawReason> _withdrawReasonsCollection;
        private AdmissionApplicationWithdrawalReasonsService _admissionApplicationWithdrawalReasonsService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepositoryMock; 

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();


            _withdrawReasonsCollection = new List<WithdrawReason>()
                {
                    new Domain.Student.Entities.WithdrawReason("761597be-0a12-4aa8-8ffe-afc04b62da41", "AC", "Academic Reasons"),
                    new Domain.Student.Entities.WithdrawReason("8cc60bb6-1e0e-45f1-bf10-b53d6809275e", "FP", "Financial Problems"),
                    new Domain.Student.Entities.WithdrawReason("6196cc8c-6e2c-4bb5-8859-b2553b24c772", "MILIT", "Serve In The Armed Forces"),
                };


            _referenceRepositoryMock.Setup(repo => repo.GetWithdrawReasonsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_withdrawReasonsCollection);

            _admissionApplicationWithdrawalReasonsService = new AdmissionApplicationWithdrawalReasonsService(
                                                            _referenceRepositoryMock.Object,
                                                            _loggerMock.Object,
                                                            _adapterRegistryMock.Object,
                                                            _currentUserFactoryMock.Object,
                                                            _roleRepositoryMock.Object,
                                                            _configurationRepositoryMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _admissionApplicationWithdrawalReasonsService = null;
            _withdrawReasonsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task AdmissionApplicationWithdrawalReasonsService_GetAdmissionApplicationWithdrawalReasonsAsync()
        {
            var results = await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsAsync(true);
            Assert.IsTrue(results is IEnumerable<AdmissionApplicationWithdrawalReasons>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AdmissionApplicationWithdrawalReasonsService_GetAdmissionApplicationWithdrawalReasonsAsync_Count()
        {
            var results = await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task AdmissionApplicationWithdrawalReasonsService_GetAdmissionApplicationWithdrawalReasonsAsync_Properties()
        {
            var result =
                (await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsAsync(true)).FirstOrDefault(x => x.Code == admissionApplicationWithdrawalReasonsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task AdmissionApplicationWithdrawalReasonsService_GetAdmissionApplicationWithdrawalReasonsAsync_Expected()
        {
            var expectedResults = _withdrawReasonsCollection.FirstOrDefault(c => c.Guid == admissionApplicationWithdrawalReasonsGuid);
            var actualResult =
                (await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsAsync(true)).FirstOrDefault(x => x.Id == admissionApplicationWithdrawalReasonsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationWithdrawalReasonsService_GetAdmissionApplicationWithdrawalReasonsByGuidAsync_Empty()
        {
            await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationWithdrawalReasonsService_GetAdmissionApplicationWithdrawalReasonsByGuidAsync_Null()
        {
            await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionApplicationWithdrawalReasonsService_GetAdmissionApplicationWithdrawalReasonsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetWithdrawReasonsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsByGuidAsync("99");
        }

        [TestMethod]
        public async Task AdmissionApplicationWithdrawalReasonsService_GetAdmissionApplicationWithdrawalReasonsByGuidAsync_Expected()
        {
            var expectedResults =
                _withdrawReasonsCollection.First(c => c.Guid == admissionApplicationWithdrawalReasonsGuid);
            var actualResult =
                await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsByGuidAsync(admissionApplicationWithdrawalReasonsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AdmissionApplicationWithdrawalReasonsService_GetAdmissionApplicationWithdrawalReasonsByGuidAsync_Properties()
        {
            var result =
                await _admissionApplicationWithdrawalReasonsService.GetAdmissionApplicationWithdrawalReasonsByGuidAsync(admissionApplicationWithdrawalReasonsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}