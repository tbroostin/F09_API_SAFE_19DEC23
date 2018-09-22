//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class ContractTypesServiceTests
    {
        private const string contractTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string contractTypesCode = "AT";
        private ICollection<HrStatuses> _contractTypesCollection;
        private ContractTypesService _contractTypesService;

        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _contractTypesCollection = new List<HrStatuses>()
                {
                    new HrStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new HrStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new HrStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetHrStatusesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_contractTypesCollection);

            _contractTypesService = new ContractTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _contractTypesService = null;
            _contractTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task ContractTypesService_GetContractTypesAsync()
        {
            var results = await _contractTypesService.GetContractTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<ContractTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task ContractTypesService_GetContractTypesAsync_Count()
        {
            var results = await _contractTypesService.GetContractTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task ContractTypesService_GetContractTypesAsync_Properties()
        {
            var result =
                (await _contractTypesService.GetContractTypesAsync(true)).FirstOrDefault(x => x.Code == contractTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task ContractTypesService_GetContractTypesAsync_Expected()
        {
            var expectedResults = _contractTypesCollection.FirstOrDefault(c => c.Guid == contractTypesGuid);
            var actualResult =
                (await _contractTypesService.GetContractTypesAsync(true)).FirstOrDefault(x => x.Id == contractTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ContractTypesService_GetContractTypesByGuidAsync_Empty()
        {
            await _contractTypesService.GetContractTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ContractTypesService_GetContractTypesByGuidAsync_Null()
        {
            await _contractTypesService.GetContractTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ContractTypesService_GetContractTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetHrStatusesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _contractTypesService.GetContractTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task ContractTypesService_GetContractTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _contractTypesCollection.First(c => c.Guid == contractTypesGuid);
            var actualResult =
                await _contractTypesService.GetContractTypesByGuidAsync(contractTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task ContractTypesService_GetContractTypesByGuidAsync_Properties()
        {
            var result =
                await _contractTypesService.GetContractTypesByGuidAsync(contractTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}