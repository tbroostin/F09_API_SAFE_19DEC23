//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class TaxFormComponentsServiceTests
    {
        private const string taxFormComponentsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string taxFormComponentsCode = "AT";
        private ICollection<BoxCodes> _taxFormComponentsCollection;
        private TaxFormComponentsService _taxFormComponentsService;
        
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _taxFormComponentsCollection = new List<BoxCodes>()
                {
                    new BoxCodes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "W2"),
                    new BoxCodes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "W2"),
                    new BoxCodes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "W2")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetAllBoxCodesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_taxFormComponentsCollection);
            _referenceRepositoryMock.Setup(repo => repo.GetTaxFormsGuidAsync(It.IsAny<string>())).ReturnsAsync(taxFormComponentsGuid);

            _taxFormComponentsService = new TaxFormComponentsService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _taxFormComponentsService = null;
            _taxFormComponentsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task TaxFormComponentsService_GetTaxFormComponentsAsync()
        {
            var results = await _taxFormComponentsService.GetTaxFormComponentsAsync(true);
            Assert.IsTrue(results is IEnumerable<TaxFormComponents>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task TaxFormComponentsService_GetTaxFormComponentsAsync_Count()
        {
            var results = await _taxFormComponentsService.GetTaxFormComponentsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task TaxFormComponentsService_GetTaxFormComponentsAsync_Properties()
        {
            var result =
                (await _taxFormComponentsService.GetTaxFormComponentsAsync(true)).FirstOrDefault(x => x.Code == taxFormComponentsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task TaxFormComponentsService_GetTaxFormComponentsAsync_Expected()
        {
            var expectedResults = _taxFormComponentsCollection.FirstOrDefault(c => c.Guid == taxFormComponentsGuid);
            var actualResult =
                (await _taxFormComponentsService.GetTaxFormComponentsAsync(true)).FirstOrDefault(x => x.Id == taxFormComponentsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task TaxFormComponentsService_GetTaxFormComponentsByGuidAsync_Empty()
        {
            await _taxFormComponentsService.GetTaxFormComponentsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task TaxFormComponentsService_GetTaxFormComponentsByGuidAsync_Null()
        {
            await _taxFormComponentsService.GetTaxFormComponentsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task TaxFormComponentsService_GetTaxFormComponentsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAllBoxCodesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _taxFormComponentsService.GetTaxFormComponentsByGuidAsync("99");
        }

        [TestMethod]
        public async Task TaxFormComponentsService_GetTaxFormComponentsByGuidAsync_Expected()
        {
            var expectedResults =
                _taxFormComponentsCollection.First(c => c.Guid == taxFormComponentsGuid);
            var actualResult =
                await _taxFormComponentsService.GetTaxFormComponentsByGuidAsync(taxFormComponentsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task TaxFormComponentsService_GetTaxFormComponentsByGuidAsync_Properties()
        {
            var result =
                await _taxFormComponentsService.GetTaxFormComponentsByGuidAsync(taxFormComponentsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}