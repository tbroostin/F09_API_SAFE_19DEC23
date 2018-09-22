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
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{    
    [TestClass]
    public class AccountingCodeCategoriesServiceTests
    {
        private const string accountingCodeCategoriesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string accountingCodeCategoriesCode = "AT";
        private ICollection<ArCategory> _accountingCodeCategoriesCollection;
        private AccountingCodeCategoriesService _accountingCodeCategoriesService;
        
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
           

            _accountingCodeCategoriesCollection = new List<ArCategory>()
                {
                    new ArCategory("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new ArCategory("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new ArCategory("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetArCategoriesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_accountingCodeCategoriesCollection);

            _accountingCodeCategoriesService = new AccountingCodeCategoriesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _accountingCodeCategoriesService = null;
            _accountingCodeCategoriesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesAsync()
        {
            var results = await _accountingCodeCategoriesService.GetAccountingCodeCategoriesAsync(true);
            Assert.IsTrue(results is IEnumerable<AccountingCodeCategory>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesAsync_Count()
        {
            var results = await _accountingCodeCategoriesService.GetAccountingCodeCategoriesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesAsync_EmptyCollection()
        {
            _accountingCodeCategoriesCollection = new List<ArCategory>();
            _referenceRepositoryMock.Setup(repo => repo.GetArCategoriesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_accountingCodeCategoriesCollection);

            var results = await _accountingCodeCategoriesService.GetAccountingCodeCategoriesAsync(true);
            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesAsync_NullCollection()
        {
            _accountingCodeCategoriesCollection = null;
            _referenceRepositoryMock.Setup(repo => repo.GetArCategoriesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_accountingCodeCategoriesCollection);

            var results = await _accountingCodeCategoriesService.GetAccountingCodeCategoriesAsync(true);
            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesAsync_Properties()
        {
            var result =
                (await _accountingCodeCategoriesService.GetAccountingCodeCategoriesAsync(true)).FirstOrDefault(x => x.Code == accountingCodeCategoriesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesAsync_Expected()
        {
            var expectedResults = _accountingCodeCategoriesCollection.FirstOrDefault(c => c.Guid == accountingCodeCategoriesGuid);
            var actualResult =
                (await _accountingCodeCategoriesService.GetAccountingCodeCategoriesAsync(true)).FirstOrDefault(x => x.Id == accountingCodeCategoriesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesByGuidAsync_Empty()
        {
            await _accountingCodeCategoriesService.GetAccountingCodeCategoryByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesByGuidAsync_Null()
        {
            await _accountingCodeCategoriesService.GetAccountingCodeCategoryByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetArCategoriesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _accountingCodeCategoriesService.GetAccountingCodeCategoryByGuidAsync("99");
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesByGuidAsync_Expected()
        {
            var expectedResults =
                _accountingCodeCategoriesCollection.First(c => c.Guid == accountingCodeCategoriesGuid);
            var actualResult =
                await _accountingCodeCategoriesService.GetAccountingCodeCategoryByGuidAsync(accountingCodeCategoriesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesService_GetAccountingCodeCategoriesByGuidAsync_Properties()
        {
            var result =
                await _accountingCodeCategoriesService.GetAccountingCodeCategoryByGuidAsync(accountingCodeCategoriesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}