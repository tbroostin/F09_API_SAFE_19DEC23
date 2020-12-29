//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
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
    public class TaxFormsBaseServiceTests
    {
        private const string taxFormsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string taxFormsCode = "AT";
        private ICollection<TaxForms2> _taxFormsCollection;
        private TaxFormsBaseService _taxFormsService;
        
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
           

            _taxFormsCollection = new List<TaxForms2>()
                {
                    new TaxForms2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "A1"),
                    new TaxForms2("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "A2"),
                    new TaxForms2("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "A3")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetTaxFormsBaseAsync(It.IsAny<bool>()))
                .ReturnsAsync(_taxFormsCollection);

            _taxFormsService = new TaxFormsBaseService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _taxFormsService = null;
            _taxFormsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task TaxFormsService_GetTaxFormsAsync()
        {
            var results = await _taxFormsService.GetTaxFormsAsync(true);
            Assert.IsTrue(results is IEnumerable<Dtos.TaxForms>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task TaxFormsService_GetTaxFormsAsync_Count()
        {
            var results = await _taxFormsService.GetTaxFormsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task TaxFormsService_GetTaxFormsAsync_Properties()
        {
            var result =
                (await _taxFormsService.GetTaxFormsAsync(true)).FirstOrDefault(x => x.Code == taxFormsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task TaxFormsService_GetTaxFormsAsync_Expected()
        {
            var expectedResults = _taxFormsCollection.FirstOrDefault(c => c.Guid == taxFormsGuid);
            var actualResult =
                (await _taxFormsService.GetTaxFormsAsync(true)).FirstOrDefault(x => x.Id == taxFormsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task TaxFormsService_GetTaxFormsByGuidAsync_Empty()
        {
            await _taxFormsService.GetTaxFormsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task TaxFormsService_GetTaxFormsByGuidAsync_Null()
        {
            await _taxFormsService.GetTaxFormsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task TaxFormsService_GetTaxFormsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetTaxFormsBaseAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _taxFormsService.GetTaxFormsByGuidAsync("99");
        }

        [TestMethod]
        public async Task TaxFormsService_GetTaxFormsByGuidAsync_Expected()
        {
            var expectedResults =
                _taxFormsCollection.First(c => c.Guid == taxFormsGuid);
            var actualResult =
                await _taxFormsService.GetTaxFormsByGuidAsync(taxFormsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task TaxFormsService_GetTaxFormsByGuidAsync_Properties()
        {
            var result =
                await _taxFormsService.GetTaxFormsByGuidAsync(taxFormsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}