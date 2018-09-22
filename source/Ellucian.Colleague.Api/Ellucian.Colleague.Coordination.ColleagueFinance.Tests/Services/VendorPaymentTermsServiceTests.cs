//Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class VendorPaymentTermsService2Tests
    {
        private const string vendorPaymentTermsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string vendorPaymentTermsCode = "AT";
        private ICollection<VendorTerm> _vendorTermCollection;
        private VendorPaymentTermsService _vendorPaymentTermsService;
        private Mock<ILogger> _loggerMock;
        private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
        ICurrentUserFactory currentUserFactory;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<IAdapterRegistry> adapterRegistryMock;
        private readonly IConfigurationRepository configurationRepository;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            currentUserFactory = new CurrentUserSetup.PersonUserFactory();
            roleRepositoryMock = new Mock<IRoleRepository>();
            _vendorTermCollection = new List<VendorTerm>()
            {
                new VendorTerm("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                new VendorTerm("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                new VendorTerm("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
            };


            _referenceRepositoryMock.Setup(repo => repo.GetVendorTermsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_vendorTermCollection);

            _vendorPaymentTermsService = new VendorPaymentTermsService(_referenceRepositoryMock.Object, _loggerMock.Object, configurationRepository,
                adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object);
    }

        [TestCleanup]
        public void Cleanup()
        {
            _vendorPaymentTermsService = null;
            _vendorTermCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task VendorPaymentTermsService_GetVendorPaymentTermsAsync()
        {
            var results = await _vendorPaymentTermsService.GetVendorPaymentTermsAsync(true);
            Assert.IsTrue(results is IEnumerable<VendorPaymentTerms>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task VendorPaymentTermsService_GetVendorPaymentTermsAsync_Count()
        {
            var results = await _vendorPaymentTermsService.GetVendorPaymentTermsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task VendorPaymentTermsService_GetVendorPaymentTermsAsync_Properties()
        {
            var result =
                (await _vendorPaymentTermsService.GetVendorPaymentTermsAsync(true)).FirstOrDefault(x => x.Code == vendorPaymentTermsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task VendorPaymentTermsService_GetVendorPaymentTermsAsync_Expected()
        {
            var expectedResults = _vendorTermCollection.FirstOrDefault(c => c.Guid == vendorPaymentTermsGuid);
            var actualResult =
                (await _vendorPaymentTermsService.GetVendorPaymentTermsAsync(true)).FirstOrDefault(x => x.Id == vendorPaymentTermsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task VendorPaymentTermsService_GetVendorPaymentTermsByGuidAsync_Empty()
        {
            await _vendorPaymentTermsService.GetVendorPaymentTermsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task VendorPaymentTermsService_GetVendorPaymentTermsByGuidAsync_Null()
        {
            await _vendorPaymentTermsService.GetVendorPaymentTermsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task VendorPaymentTermsService_GetVendorPaymentTermsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetVendorTermsAsync(It.IsAny<bool>()))
                .Throws<InvalidOperationException>();

            await _vendorPaymentTermsService.GetVendorPaymentTermsByGuidAsync("99");
        }

        [TestMethod]
        public async Task VendorPaymentTermsService_GetVendorPaymentTermsByGuidAsync_Expected()
        {
            var expectedResults =
                _vendorTermCollection.First(c => c.Guid == vendorPaymentTermsGuid);
            var actualResult =
                await _vendorPaymentTermsService.GetVendorPaymentTermsByGuidAsync(vendorPaymentTermsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task VendorPaymentTermsService_GetVendorPaymentTermsByGuidAsync_Properties()
        {
            var result =
                await _vendorPaymentTermsService.GetVendorPaymentTermsByGuidAsync(vendorPaymentTermsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}