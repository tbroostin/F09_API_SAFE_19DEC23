//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
    public class FinancialAidAcademicProgressStatusesServiceTests
    {
        private const string financialAidAcademicProgressStatusesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string financialAidAcademicProgressStatusesCode = "AT";
        private ICollection<SapStatuses> _financialAidAcademicProgressStatusesCollection;
        private FinancialAidAcademicProgressStatusesService _financialAidAcademicProgressStatusesService;
        
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
           

            _financialAidAcademicProgressStatusesCollection = new List<SapStatuses>()
                {
                    new SapStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new SapStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new SapStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetSapStatusesAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(_financialAidAcademicProgressStatusesCollection);

            _financialAidAcademicProgressStatusesService = new FinancialAidAcademicProgressStatusesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _financialAidAcademicProgressStatusesService = null;
            _financialAidAcademicProgressStatusesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressStatusesService_GetFinancialAidAcademicProgressStatusesAsync()
        {
            var results = await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), true);
            Assert.IsTrue(results is IEnumerable<FinancialAidAcademicProgressStatuses>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressStatusesService_GetFinancialAidAcademicProgressStatusesAsync_Count()
        {
            var results = await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressStatusesService_GetFinancialAidAcademicProgressStatusesAsync_Properties()
        {
            var result =
                (await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), true)).FirstOrDefault(x => x.Code == financialAidAcademicProgressStatusesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressStatusesService_GetFinancialAidAcademicProgressStatusesAsync_Expected()
        {
            var expectedResults = _financialAidAcademicProgressStatusesCollection.FirstOrDefault(c => c.Guid == financialAidAcademicProgressStatusesGuid);
            var actualResult =
                (await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesAsync(It.IsAny<Dtos.EnumProperties.RestrictedVisibility>(), true)).FirstOrDefault(x => x.Id == financialAidAcademicProgressStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressStatusesService_GetFinancialAidAcademicProgressStatusesAsync_With_RestrictedVisibility()
        {
            var expectedResults = _financialAidAcademicProgressStatusesCollection.FirstOrDefault(c => c.Guid == financialAidAcademicProgressStatusesGuid);
            var actualResult =
                (await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesAsync(Dtos.EnumProperties.RestrictedVisibility.Yes, true)).FirstOrDefault(x => x.Id == financialAidAcademicProgressStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FinancialAidAcademicProgressStatusesService_GetFinancialAidAcademicProgressStatusesByGuidAsync_Empty()
        {
            await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FinancialAidAcademicProgressStatusesService_GetFinancialAidAcademicProgressStatusesByGuidAsync_Null()
        {
            await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FinancialAidAcademicProgressStatusesService_GetFinancialAidAcademicProgressStatusesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetSapStatusesAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesByGuidAsync("99");
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressStatusesService_GetFinancialAidAcademicProgressStatusesByGuidAsync_Expected()
        {
            var expectedResults =
                _financialAidAcademicProgressStatusesCollection.First(c => c.Guid == financialAidAcademicProgressStatusesGuid);
            var actualResult =
                await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesByGuidAsync(financialAidAcademicProgressStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressStatusesService_GetFinancialAidAcademicProgressStatusesByGuidAsync_Properties()
        {
            var result =
                await _financialAidAcademicProgressStatusesService.GetFinancialAidAcademicProgressStatusesByGuidAsync(financialAidAcademicProgressStatusesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}