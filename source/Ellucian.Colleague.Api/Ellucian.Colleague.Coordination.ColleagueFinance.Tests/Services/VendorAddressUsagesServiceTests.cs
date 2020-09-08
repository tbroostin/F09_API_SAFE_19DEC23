//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
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

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class VendorAddressUsagesServiceTests
    {
        private const string vendorAddressUsagesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        //private const string vendorAddressUsagesCode = "AT";
        private ICollection<IntgVendorAddressUsages> _vendorAddressUsagesCollection;
        private VendorAddressUsagesService _vendorAddressUsagesService;

        private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _vendorAddressUsagesCollection = new List<IntgVendorAddressUsages>()
                {
                    new IntgVendorAddressUsages("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new IntgVendorAddressUsages("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new IntgVendorAddressUsages("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetIntgVendorAddressUsagesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_vendorAddressUsagesCollection);

            _vendorAddressUsagesService = new VendorAddressUsagesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _vendorAddressUsagesService = null;
            _vendorAddressUsagesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task VendorAddressUsagesService_GetVendorAddressUsagesAsync()
        {
            var results = await _vendorAddressUsagesService.GetVendorAddressUsagesAsync(true);
            Assert.IsTrue(results is IEnumerable<VendorAddressUsages>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task VendorAddressUsagesService_GetVendorAddressUsagesAsync_Count()
        {
            var results = await _vendorAddressUsagesService.GetVendorAddressUsagesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        //[TestMethod]
        //public async Task VendorAddressUsagesService_GetVendorAddressUsagesAsync_Properties()
        //{
        //    var result =
        //        (await _vendorAddressUsagesService.GetVendorAddressUsagesAsync(true)).FirstOrDefault(x => x.Code == vendorAddressUsagesCode);
        //    Assert.IsNotNull(result.Id);
        //    //Assert.IsNotNull(result.Code);
        //    Assert.IsNull(result.Description);

        //}

        [TestMethod]
        public async Task VendorAddressUsagesService_GetVendorAddressUsagesAsync_Expected()
        {
            var expectedResults = _vendorAddressUsagesCollection.FirstOrDefault(c => c.Guid == vendorAddressUsagesGuid);
            var actualResult =
                (await _vendorAddressUsagesService.GetVendorAddressUsagesAsync(true)).FirstOrDefault(x => x.Id == vendorAddressUsagesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            //Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorAddressUsagesService_GetVendorAddressUsagesByGuidAsync_Empty()
        {
            await _vendorAddressUsagesService.GetVendorAddressUsagesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorAddressUsagesService_GetVendorAddressUsagesByGuidAsync_Null()
        {
            await _vendorAddressUsagesService.GetVendorAddressUsagesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task VendorAddressUsagesService_GetVendorAddressUsagesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetIntgVendorAddressUsagesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _vendorAddressUsagesService.GetVendorAddressUsagesByGuidAsync("99");
        }

        [TestMethod]
        public async Task VendorAddressUsagesService_GetVendorAddressUsagesByGuidAsync_Expected()
        {
            var expectedResults =
                _vendorAddressUsagesCollection.First(c => c.Guid == vendorAddressUsagesGuid);
            var actualResult =
                await _vendorAddressUsagesService.GetVendorAddressUsagesByGuidAsync(vendorAddressUsagesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            //Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task VendorAddressUsagesService_GetVendorAddressUsagesByGuidAsync_Properties()
        {
            var result =
                await _vendorAddressUsagesService.GetVendorAddressUsagesByGuidAsync(vendorAddressUsagesGuid);
            Assert.IsNotNull(result.Id);
            //Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
        }
    }
}