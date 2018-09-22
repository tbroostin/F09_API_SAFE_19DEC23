// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using System.Threading.Tasks;
using System.IO;
using System.Threading.Tasks;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class VendorTypesServiceTests
    {

        // test VendorTypes Service.
        private VendorTypesService vendorTypesService;
        private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepoMock;
        private IColleagueFinanceReferenceDataRepository colleagueFinanceRefRepo;
        Mock<IAdapterRegistry> adapterRegistryMock;
        ICurrentUserFactory currentUserFactory;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;

        private readonly IConfigurationRepository configurationRepository;
        
        IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.VendorType> vendorTypes;

        [TestInitialize]
        public void Initialize()
        {
            colleagueFinanceReferenceDataRepoMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            colleagueFinanceRefRepo = colleagueFinanceReferenceDataRepoMock.Object;
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            currentUserFactory = new CurrentUserSetup.PersonUserFactory();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();

            vendorTypes = new TestColleagueFinanceReferenceDataRepository().GetVendorTypesAsync(false).Result;

            vendorTypesService = new VendorTypesService(colleagueFinanceRefRepo, loggerMock.Object, configurationRepository, adapterRegistryMock.Object, currentUserFactory,roleRepositoryMock.Object );

    }

        [TestCleanup]
        public void Cleanup()
        {
            vendorTypesService = null;
            colleagueFinanceReferenceDataRepoMock = null;
            colleagueFinanceRefRepo = null;
            vendorTypes = null;
            loggerMock = null;
        }

        [TestMethod]
        public async Task vendorTypesService__GetAllAsync()
        {
            colleagueFinanceReferenceDataRepoMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypes);

            var results = await vendorTypesService.GetVendorTypesAsync(It.IsAny<bool>());
            Assert.AreEqual(vendorTypes.ToList().Count, (results.Count()));

            foreach (var vendorType in vendorTypes)
            {
                var result = results.FirstOrDefault(i => i.Id == vendorType.Guid);

                Assert.AreEqual(vendorType.Code, result.Code);
                Assert.AreEqual(vendorType.Description, result.Title);
                Assert.AreEqual(vendorType.Guid, result.Id);
            }
        }

        [TestMethod]
        public async Task vendorTypesService__GetByIdAsync()
        {
            colleagueFinanceReferenceDataRepoMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypes);

            string id = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
            var vendorType = vendorTypes.FirstOrDefault(i => i.Guid == id);

            var result = await vendorTypesService.GetVendorTypeByIdAsync(id);

            Assert.AreEqual(vendorType.Code, result.Code);
            Assert.AreEqual(vendorType.Description, result.Title);
            Assert.AreEqual(vendorType.Guid, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task vendorTypesService__GetByIdAsync_KeyNotFoundException()
        {
            colleagueFinanceReferenceDataRepoMock.Setup(i => i.GetVendorTypesAsync(true)).ReturnsAsync(vendorTypes);
            var result = await vendorTypesService.GetVendorTypeByIdAsync("123");
        }
    }
}
