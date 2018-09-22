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
    public class FinancialAidAcademicProgressTypesServiceTests
    {
        private const string financialAidAcademicProgressTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string financialAidAcademicProgressTypesCode = "AT";
        private ICollection<SapType> _financialAidAcademicProgressTypesCollection;
        private FinancialAidAcademicProgressTypesService _financialAidAcademicProgressTypesService;
        
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
           

            _financialAidAcademicProgressTypesCollection = new List<SapType>()
                {
                    new SapType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new SapType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new SapType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetSapTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_financialAidAcademicProgressTypesCollection);

            _financialAidAcademicProgressTypesService = new FinancialAidAcademicProgressTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _financialAidAcademicProgressTypesService = null;
            _financialAidAcademicProgressTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressTypesService_GetFinancialAidAcademicProgressTypesAsync()
        {
            var results = await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<FinancialAidAcademicProgressTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressTypesService_GetFinancialAidAcademicProgressTypesAsync_Count()
        {
            var results = await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressTypesService_GetFinancialAidAcademicProgressTypesAsync_Properties()
        {
            var result =
                (await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesAsync(true)).FirstOrDefault(x => x.Code == financialAidAcademicProgressTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressTypesService_GetFinancialAidAcademicProgressTypesAsync_Expected()
        {
            var expectedResults = _financialAidAcademicProgressTypesCollection.FirstOrDefault(c => c.Guid == financialAidAcademicProgressTypesGuid);
            var actualResult =
                (await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesAsync(true)).FirstOrDefault(x => x.Id == financialAidAcademicProgressTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FinancialAidAcademicProgressTypesService_GetFinancialAidAcademicProgressTypesByGuidAsync_Empty()
        {
            await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FinancialAidAcademicProgressTypesService_GetFinancialAidAcademicProgressTypesByGuidAsync_Null()
        {
            await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FinancialAidAcademicProgressTypesService_GetFinancialAidAcademicProgressTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetSapTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressTypesService_GetFinancialAidAcademicProgressTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _financialAidAcademicProgressTypesCollection.First(c => c.Guid == financialAidAcademicProgressTypesGuid);
            var actualResult =
                await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesByGuidAsync(financialAidAcademicProgressTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task FinancialAidAcademicProgressTypesService_GetFinancialAidAcademicProgressTypesByGuidAsync_Properties()
        {
            var result =
                await _financialAidAcademicProgressTypesService.GetFinancialAidAcademicProgressTypesByGuidAsync(financialAidAcademicProgressTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}