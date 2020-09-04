/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
//using Ellucian.Colleague.Domain.Base.;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
//using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class VendorHoldReasonsServiceTests : ColleagueFinanceServiceTestsSetup
    {
        public Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepository;
        VendorHoldReasonsService vendorHoldReasonsService;
        List<Dtos.VendorHoldReasons> vendorHoldReasonDtoList = new List<Dtos.VendorHoldReasons>();
        List<Domain.ColleagueFinance.Entities.VendorHoldReasons> vendorHoldReasonEntityList = new List<Domain.ColleagueFinance.Entities.VendorHoldReasons>();
        string id = "03ef76f3-61be-4990-8a99-9a80282fc420";

        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;

            MockInitialize();
            BuildData();
            referenceDataRepository = new Mock<IColleagueFinanceReferenceDataRepository>();
            vendorHoldReasonsService = new VendorHoldReasonsService(referenceDataRepository.Object, adapterRegistryMock.Object,
                GLCurrentUserFactory, _configurationRepository, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            referenceDataRepository = null;
            vendorHoldReasonsService = null;
            vendorHoldReasonDtoList = null;
            vendorHoldReasonEntityList = null;
        }

        [TestMethod]
        public async Task VendorHoldReasons_GetAll()
        {
            referenceDataRepository.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ReturnsAsync(vendorHoldReasonEntityList);

            var actuals = await vendorHoldReasonsService.GetVendorHoldReasonsAsync(It.IsAny<bool>());

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = vendorHoldReasonDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task VendorHoldReasons_GetAll_True()
        {
            referenceDataRepository.Setup(i => i.GetVendorHoldReasonsAsync(true)).ReturnsAsync(vendorHoldReasonEntityList);

            var actuals = await vendorHoldReasonsService.GetVendorHoldReasonsAsync(true);

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = vendorHoldReasonDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task VendorHoldReasons_GetById()
        {
            var expected = vendorHoldReasonDtoList.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            referenceDataRepository.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ReturnsAsync(vendorHoldReasonEntityList);

            var actual = await vendorHoldReasonsService.GetVendorHoldReasonsByGuidAsync(id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.IsNull(actual.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task VendorHoldReasons_GetById_InvalidOperationException()
        {
            referenceDataRepository.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ReturnsAsync(vendorHoldReasonEntityList);
            var actual = await vendorHoldReasonsService.GetVendorHoldReasonsByGuidAsync("abc");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task VendorHoldReasons_GetById_Exception()
        {
            referenceDataRepository.Setup(i => i.GetVendorHoldReasonsAsync(true)).ThrowsAsync(new Exception());
            var actual = await vendorHoldReasonsService.GetVendorHoldReasonsByGuidAsync(It.IsAny<string>());
        }

        private void BuildData()
        {
            vendorHoldReasonEntityList = new List<Domain.ColleagueFinance.Entities.VendorHoldReasons>() 
            {
                new Domain.ColleagueFinance.Entities.VendorHoldReasons("03ef76f3-61be-4990-8a99-9a80282fc420", "OB", "Out of Business"),
                new Domain.ColleagueFinance.Entities.VendorHoldReasons("d2f4f0af-6714-48c7-88d5-1c40cb407b6c", "DISC", "Vendor Discontinued"),
                new Domain.ColleagueFinance.Entities.VendorHoldReasons("c517d7a5-f06a-42c8-85ab-b6320e1c0c2a", "QUAL", "Quality Hold"),
                new Domain.ColleagueFinance.Entities.VendorHoldReasons("6c591aaa-5d33-4b19-b5e9-f6cf8956ef0a", "DISP", "Disputed Transactions"),
            };
            foreach (var entity in vendorHoldReasonEntityList)
            {
                vendorHoldReasonDtoList.Add(new Dtos.VendorHoldReasons()
                {
                    Id = entity.Guid,
                    Code = entity.Code,
                    Title = entity.Description,
                    Description = null,

                   
                });
            }
        }
    }
}
